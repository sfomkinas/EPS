using EPS.Data;
using EPS.Helper;
using EPS.Protocol;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace EPS
{
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.  

            var ip = System.Configuration.ConfigurationManager.AppSettings["SocketIP"] ?? "localhost";
            int port = 0;
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["SocketPort"] ?? "11000", out port);

            IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  

                //state convertuoti į bitus
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the
                    // client. Display it on the console. 

                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                      content.Length, content);

                    var formatter = new BinaryFormatter();
                    var netStream = new NetworkStream(handler);
                    HeaderRequest data = (HeaderRequest)formatter.Deserialize(netStream);

                    NetworkStream responseStream = new NetworkStream(handler);
                    var headerResponse = new HeaderResponse();
                    HeaderResponse responseH = (HeaderResponse)formatter.Deserialize(responseStream);
                    switch (data.Action)
                    {
                        case ActionEnum.GenerateCodes:
                            GenereteCodesRequest generate = (GenereteCodesRequest)formatter.Deserialize(netStream);
                            generate.Read(netStream);

                            headerResponse.Action = ActionEnum.GenerateCodes;
                            responseH.Write(responseStream);

                            var resultGenerate = CardCodesHelper.GenerateCodes(generate.Count, generate.Length);
                            var generateResposne = new GenereteCodesResponse();
                            generateResposne.Notification = resultGenerate.Notification;
                            generateResposne.Write(responseStream);
                            break;
                        case ActionEnum.UseCode:
                            UseCodeRequest use = (UseCodeRequest)formatter.Deserialize(netStream);
                            use.Read(netStream);

                            headerResponse.Action = ActionEnum.UseCode;
                            responseH.Write(responseStream);

                            var resultUse = CardCodesHelper.UseCode(use.Code);
                            var useCodeResponse = new UseCodeResponse();
                            useCodeResponse.Result = resultUse.UseCodeStatus;
                            useCodeResponse.Notification = resultUse.Notification;
                            useCodeResponse.Write(responseStream);

                            break;
                        case ActionEnum.CheckCode:
                            break;
                        default:
                            break;
                    }

                    // Echo the data back to the client.  
                    Send(handler, responseStream.ToString());
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(String[] args)
        {
            //CardCodesHelper.GenerateCodes(5, 8);
            StartListening();
            return 0;
        }
    }
}
