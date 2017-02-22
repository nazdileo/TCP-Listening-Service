using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace ListeningService
{
    public partial class ListeningService : ServiceBase
    {
        static int handShakeCount = 0;
        static int activeConnections = 0;

        public ListeningService()
        { 
            InitializeComponent(); 
        }

        protected override void OnStart(string[] args)
        {
            StartListening();
        }

        protected override void OnStop()
        {
        }

        private static void StartListening()
        {
            TcpListener listener = null;
            try
            {
                int portNumber = Convert.ToInt16(ConfigurationManager.AppSettings["PortNumber"]);

                listener = new TcpListener(IPAddress.Any, portNumber);	// listen on specific tcp port
                listener.Start();

                // listen for incoming connections
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();  // wait for new incoming client connection
                    activeConnections++;				            // new incoming client connection detected, so add to Active Connections counter
                    Thread t = new Thread(ProcessClientRequests);
                    t.Start(client);				                // spawn a new thread to allow the new client connection to be processed independantly
                }
            }
            catch (Exception e)
            {
                // handle exception
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();	// stop the listener
                }
            }
        
        }
        
        // Method handles the network stream communication and other applicable
        // processing between the server and the client applications
        private static void ProcessClientRequests(object argument)
        {
            bool handShake = false;

            TcpClient client = (TcpClient)argument;					            // create the client instance

            try
            {

                StreamReader reader = new StreamReader(client.GetStream());
                StreamWriter writer = new StreamWriter(client.GetStream());

                string inputVal = String.Empty;

                while (!(inputVal = reader.ReadLine()).Equals("TERMINATE"))
                {
                    
                    if (inputVal == "HELO" || handShake)			            // verify successful handshake
                    {
                        switch (inputVal)
                        {
							case "HELO":
								writer.WriteLine("HI");				            // write result back to the client
								handShake = true;
								handShakeCount++;                               // increment the successful handshakes counter
								break;
							case "COUNT":
								writer.WriteLine(handShakeCount.ToString());	// return the successful handshakes counter back to the client
								break;
							case "CONNECTIONS":
								writer.WriteLine(activeConnections.ToString());	// return the number of active connections back to the client
								break;
							case "PRIME":
                                int primeNo = PrimeNumber();
								writer.WriteLine(primeNo.ToString());	    // return a prime number back to the client
								break;
							default:
								break;
						}
	
						writer.Flush();

					}
					else									                    // inform client of no successful handshake 
					{
						writer.WriteLine("No successful handshake");			// write back to the client
						writer.Flush();
					}

				}
                
                // TERMINATE was received from client
                reader.Close();
                writer.WriteLine("BYE");	// write back to the client
                writer.Flush();
                writer.Close();
                client.Close();			    // terminate the connection
                activeConnections--;        // decriment the Active Connections counter

            }
            catch (IOException)
            {
			    // handle exception
		    }
            catch (Exception e)
            {
                // handle exception
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
            }
        }

        // This method returns a randomly generated prime number between 1 and 1000000.
        private static int PrimeNumber()
        {
            Random rnd = new Random();
            bool primeFound = false;
            int primeNum = 1;
            while (!primeFound)
            {
                primeNum = rnd.Next(1, 1000000);    // create a random number between one and one million.
                primeFound = isPrime(primeNum);     // is the random number a prime number?
            }

            return primeNum;

        }
        
        // This method determines whether an integer is a prime number.
        private static bool isPrime(int number)
        {
            
            if (number == 1) return false;
            if (number == 2) return true;
            
            for (int i = 2; i <= Math.Ceiling(Math.Sqrt(number)); ++i)
            {
                if (number % i == 0) return false;
            }

            return true;

        }
    
    }

}
