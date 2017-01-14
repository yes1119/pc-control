using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Module.Server
{
    public delegate void Receive(Socket s, string[] packetType, string[] packetData);
    class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 100000;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    class CServer
    {
        private int port;

        private List<Socket> clients = new List<Socket>();    //모듈과 연결된 관리자 또는 or 서버

        private Receive receive_Function;

        public ManualResetEvent allDone = new ManualResetEvent(false);

        public CServer(Receive receive_Function, int port)
        {
            this.receive_Function = receive_Function;
            this.port = port;
        }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
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
                    Thread.Sleep(100);
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
                Console.WriteLine(e.ToString() + " : " + e.Message + "33333333333333333333333");
            }

            finally
            {
                string[] arr = new string[2];
                arr[0] = "TOMODULE";
                arr[1] = "ADMIN_LOGOUT";
                receive_Function(listener, arr, arr);

                string[] arr2 = new string[2];
                arr2[0] = "TOMODULE";
                arr2[1] = "FLOOR_LOGOUT";
                receive_Function(listener, arr2, arr2);

                clients.Remove(listener);
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
 
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

       

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            clients.Add(handler);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
        
        public void ReadCallback(IAsyncResult ar)
        {   
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.Default.GetString(
                        state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.

                    //데이터 한번에 받았을 경우를 대비하여 끊는다.
                    content = state.sb.ToString();
                    string[] str = content.Split('&');
                    for (int i = 0; i < str.Length - 1; i++)      
                    {
                        Console.WriteLine("[받은 메세지] :"+str[i]);          //리시브 함수 호출.
                        Console.WriteLine("--------------------------------------------------------");
                        string[] packetType = str[i].Split('+');

                        if (packetType[0] == "TOSERVER") //서버로 보내는 경우
                        {
                            string[] packetData = packetType[3].Split('#');
                            receive_Function(handler, packetType, packetData);
                        }

                        else //어드민->모듈, 플로어->어드민보내는경우
                        {
                            IPEndPoint ipInfo = (IPEndPoint)handler.RemoteEndPoint;
                            string[] packetData1 = packetType[2].Split('#');

                            if (packetType[1] == "NEEDLESS_SOFTWARELIST")
                            {
                                string[] packetData2 = str[i].Split('★');
                                string[] packetData3 = packetData2[1].Split('☆');

                                receive_Function(handler, packetType, packetData3);
                            }
                            else
                            {
                                receive_Function(handler, packetType, packetData1);       //리슨 델리게이트 생성.
                            }

                        }              
                    }

                    state.sb.Clear();

                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
             
            catch(Exception e)
            {
                IPEndPoint ipInfo = (IPEndPoint)handler.RemoteEndPoint;
                Console.Write("오류메세지11111111111 : " + e.Message);
                Console.WriteLine("  // 아이피:{0}, 포트: {1}와 연결이 끊어짐... ", ipInfo.Address, ipInfo.Port);

                string[] arr = new string[2];
                arr[0] = "TOMODULE";
                arr[1] = "ADMIN_LOGOUT";
                receive_Function(handler, arr, arr);

                string[] arr2 = new string[2];
                arr2[0] = "TOMODULE";
                arr2[1] = "FLOOR_LOGOUT";
                receive_Function(handler, arr2, arr2);
                
                clients.Remove(handler);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

        public void RemoveSock(Socket sock)
        {
            //clients.Remove(sock);
            //sock.Shutdown(SocketShutdown.Both);
            //sock.Close();
 
        }

        public void Send(Socket handler, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        public void SendtoAdmin(Socket handler, string data)
        {

            for (int i = 0; i < clients.Count; i++)
            {
                if (handler == clients[i])
                {
                    // Convert the string data to byte data using ASCII encoding.
                    byte[] byteData = Encoding.Default.GetBytes(data);

                    // Begin sending the data to the remote device.
                    handler.BeginSend(byteData, 0, byteData.Length, 0,
                        new AsyncCallback(SendCallback), handler);

                    break;
                }               
            }
           
        }
        
        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;     //  handler = 관리자 서버 소켓

            try
            {
                // Complete sending the data to the remote device. 여기서 메세지가 보내짐
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString() + " : " + e.Message);

                IPEndPoint ipInfo = (IPEndPoint)handler.RemoteEndPoint;
                Console.Write("오류메세지222222 : " + e.Message);
                Console.WriteLine("  // 아이피:{0}, 포트: {1}와 연결이 끊어짐... ", ipInfo.Address, ipInfo.Port);

                string[] arr = new string[2];
                arr[0] = "TOMODULE";
                arr[1] = "ADMIN_LOGOUT";
                receive_Function(handler, arr, arr);

                string[] arr2 = new string[2];
                arr2[0] = "TOMODULE";
                arr2[1] = "FLOOR_LOGOUT";
                receive_Function(handler, arr2, arr2);
              
                clients.Remove(handler);
                handler.Shutdown(SocketShutdown.Both);      //handler 소켓 - 보내기 불가
                handler.Close();

            }
        }

    }
}
