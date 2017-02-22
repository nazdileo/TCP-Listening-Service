=== TCP Listening Service ===

This is a windows service that listens for incoming client connections on tcp port 55555.
Multiple clients may connect to the server at the same time because it is multithreaded.  
When a new incoming client connection is detected, a TcpClient connection thread is spawned 
and passed to a method that will process the client connection independantly, handling the
network stream communication and other applicable processing between the server and the connected
client.  This frees up the server to go back and listen for another incoming client connection.

Design Decisions:

When the windows service is started, client processing logic that uses the TcpListener object is 
created that listens for incoming client connections on port 55555.  Within a loop that repeats 
indefinitely, the AcceptTcpClient method is used to accept a pending connection request.  This is
a blocking method that blocks until it receives an incoming connection.  As soon as it receives 
an incoming connection, it will unblock and return a TcpClient object.

Using the Thread class, a tcpClient connection is spawned into its' own thread so that it can be 
processed independantly, and allow the server to go back and listen for other incoming connections.  
The spawned TcpClient object thread will be passed to a method to process all requests from that 
client connection.  The TcpClient.GetStream method is used to obtain the underlying NetworkStream 
of the TcpClient object.  The NetworkStream provides methods for sending and receiving with the 
service until the client requests to terminate the connection.

Trade-offs:

Use AcceptTcpClientAsync method of the TcpListener class instead of AcceptTcpClient method because 
the operation will not block other client connection requests.

Notes:

This service gives no consideration to memory usage, or limiting the number of active client connections.
