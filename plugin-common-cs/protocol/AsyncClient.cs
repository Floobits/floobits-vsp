using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using SysEncoding = System.Text.Encoding;

namespace Floobits.Common.Client
{
    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    }

    public class AsynchronousClient
    {
        private Socket client;
        private string host;
        private int port;
        private int toMillis;
        public delegate void loggerDel(string msg, params object[] args);
        private loggerDel logger;
        public delegate void receiverDel(string msg);
        private receiverDel receiver;

        private ManualResetEvent connectDone =
            new ManualResetEvent(false);

        public AsynchronousClient(string host, int port, int toMillis, loggerDel logger, receiverDel receiver)
        {
            this.host = host;
            this.port = port;
            this.toMillis = toMillis;
            this.logger = logger;
            this.receiver = receiver;
        }

        public void Connect()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                if (connectDone.WaitOne(toMillis))
                {
                    StartReceive();
                }
                else
                {
                    client = null;
                }
            }
            catch (Exception e)
            {
                client = null;
                Console.WriteLine(e.ToString());
            }
        }

        public void Shutdown()
        {
            // Release the socket.
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            client = null;
        }

        public bool isConnected()
        {
            return client != null;
        }

        protected void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                client.EndConnect(ar);

                logger("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                logger(e.ToString());
            }
        }

        protected void StartReceive()
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                logger(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                logger("Read {0} bytes from server.", bytesRead);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    receiver(SysEncoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                logger(e.ToString());
            }
        }

        public void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = SysEncoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                logger("Sent {0} bytes to server.", bytesSent);
            }
            catch (Exception e)
            {
                logger(e.ToString());
            }
        }
    }

}