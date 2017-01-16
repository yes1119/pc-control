﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pc_Client.SendData
{
    public static class AsynchronousSocketListener
    {
        #region Memebers
        public static string acceptIP; //해당 얻어올 IP
        public static bool accept_Check = false; //
        public static int port;
        private static int signal;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private delegate void AddClientHandler(IPEndPoint IpEndPoint);
        private delegate void CompleteSendHandler();
        private delegate void RemoveItemHandler(string ipAddress);
        private delegate void EnableSendHandler();
        private const int c_clientSockets = 100;
        private const int c_bufferSize = 5242880;
        public static IList<Socket> Clients = new List<Socket>();
        public static IDictionary<Socket, IPEndPoint> ClientsToSend = new Dictionary<Socket, IPEndPoint>();
        public static string FileToSend { get; set; }

        //=======================================================================
        //추가 내용. 2015 02 04
        static Thread sendThread;
        //=======================================================================
        #endregion

        public static void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // Use IPv4 as the network protocol,if you want to support IPV6 protocol, you can update here.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //대기소켓 생성

            try
            {
                listener.Bind(localEndPoint); //바인딩
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            listener.Listen(c_clientSockets); //릿슨

            //loop listening the client.
            while (true)
            {
                allDone.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                if (allDone.WaitOne())
                {
                    if (Clients.Count == 0)
                        continue;

                    Console.WriteLine("BeginAccept성공");

                    //=======================================================================
                    //추가 내용. 2015 02 04
                    List<Socket> deleteSocket = new List<Socket>();
                    for (int i = 0; i < Clients.Count; i++)
                    {
                        if (Clients[i].Connected == false)
                            deleteSocket.Add(Clients[i]);
                    }

                    for (int k = 0; k < deleteSocket.Count; k++)
                        Clients.Remove(deleteSocket[k]);

                    foreach (Socket handler1 in AsynchronousSocketListener.Clients)
                    {
                        IPEndPoint ipEndPoint1 = (IPEndPoint)handler1.RemoteEndPoint;
                        string address = ipEndPoint1.ToString();

                        AsynchronousSocketListener.ClientsToSend.Remove(handler1);

                        AsynchronousSocketListener.ClientsToSend.Add(handler1, ipEndPoint1);
                    }
                   

                    sendThread = new Thread(new ThreadStart(AsynchronousSocketListener.Send));
                    sendThread.IsBackground = true;
                    sendThread.Start();
                    //=======================================================================
                }
            }


        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);//클라이언트와 통신할 통신소켓 생성

            IPEndPoint ipEndPoint = handler.RemoteEndPoint as IPEndPoint;

            Console.WriteLine("주소 : {0} 포트 : {1}가 접속하였습니다.", ipEndPoint.Address, ipEndPoint.Port);
            if (accept_Check == false && acceptIP == ipEndPoint.Address.ToString())
            {
                Clients.Add(handler);
                accept_Check = true;
            }
            else
            {
                Console.WriteLine("주소 : {0} 포트 : {1}와 연결을 종료합니다.", ipEndPoint.Address, ipEndPoint.Port);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

            allDone.Set();
        }


        #region Send
        public static void SendFileInfo()
        {
            string fileName = FileToSend.Replace("\\", "/");
            IList<Socket> closedSockets = new List<Socket>();
            IList<String> removedItems = new List<String>();

            while (fileName.IndexOf("/") > -1)
            {
                fileName = fileName.Substring(fileName.IndexOf("/") + 1);
            }

            FileInfo fileInfo = new FileInfo(FileToSend);
            long fileLen = fileInfo.Length;

            byte[] fileLenByte = BitConverter.GetBytes(fileLen);

            byte[] fileNameByte = Encoding.Unicode.GetBytes(fileName);

            byte[] clientData = new byte[4 + fileNameByte.Length + 8];

            byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);

            fileNameLen.CopyTo(clientData, 0);
            fileNameByte.CopyTo(clientData, 4);
            fileLenByte.CopyTo(clientData, 4 + fileNameByte.Length);

            // Send the file name and length to the clients. 
            foreach (KeyValuePair<Socket, IPEndPoint> kvp in ClientsToSend)
            {
                Socket handler = kvp.Key;
                IPEndPoint ipEndPoint = kvp.Value;
                try
                {
                    handler.Send(clientData);
                }
                catch
                {
                    if (!handler.Connected)
                    {
                        closedSockets.Add(handler);

                        removedItems.Add(ipEndPoint.ToString());
                    }
                }
            }

            // Remove the clients which are disconnected.
            RemoveClient(closedSockets);
            closedSockets.Clear();
            removedItems.Clear();
        }

        public static void Send()
        {
            int readBytes = 0;
            byte[] buffer = new byte[c_bufferSize];
            IList<Socket> closedSockets = new List<Socket>();
            IList<String> removedItems = new List<String>();
            int count = 0;

            // Send file information to the clients.
            SendFileInfo();

            // Blocking read file and send to the clients asynchronously.
            using (FileStream stream = new FileStream(FileToSend, FileMode.Open))
            {
                do
                {
                    sendDone.Reset();
                    signal = 0;
                    stream.Flush();
                    readBytes = stream.Read(buffer, 0, c_bufferSize);

                    if (ClientsToSend.Count == 0)
                    {
                        sendDone.Set();
                    }

                    foreach (KeyValuePair<Socket, IPEndPoint> kvp in ClientsToSend)
                    {
                        Socket handler = kvp.Key;
                        IPEndPoint ipEndPoint = kvp.Value;
                        try
                        {
                            count++;
                            handler.BeginSend(buffer, 0, readBytes, SocketFlags.None, new AsyncCallback(SendCallback), handler);
                            Console.WriteLine("{0}", count);

                        }
                        catch
                        {
                            if (!handler.Connected)
                            {
                                Console.WriteLine("전송 실패...");
                                readBytes = 0;
                                closedSockets.Add(handler);
                                signal++;
                                removedItems.Add(ipEndPoint.ToString());

                                // Signal if all the clients are disconnected.
                                if (signal >= ClientsToSend.Count)
                                {
                                    sendDone.Set();
                                }
                            }
                        }
                    }
                    sendDone.WaitOne();

                    // Remove the clients which are disconnected.
                    RemoveClient(closedSockets);
                    closedSockets.Clear();
                    removedItems.Clear();
                }
                while (readBytes > 0);

            }

            // Disconnect all the connection when the file has send to the clients completely.
            Console.WriteLine("완료!!");
            accept_Check = false;
            ClientDisconnect();
        }
        private static void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                handler = (Socket)ar.AsyncState;
                signal++;
                int bytesSent = handler.EndSend(ar);

                // Close the socket when all the data has sent to the client.
                if (bytesSent == 0)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (ArgumentException argEx)     //무슨에러일까?
            {
                Console.WriteLine(argEx.Message);
                //accept_Check = false;         
                Console.WriteLine("종료1");

            }
            catch (SocketException)
            {
                Console.WriteLine("종료2");
                accept_Check = false;

                //실패 메시지 패킷 보내기
                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                string myIP = IPHost.AddressList[0].ToString();

                string fileName = FileToSend.Replace("\\", "/");
                while (fileName.IndexOf("/") > -1)
                {
                    fileName = fileName.Substring(fileName.IndexOf("/") + 1);
                }

                string sendDataer = string.Empty;
                sendDataer += "CLIENT_SERVER_FAIL+";    //패킷 타입.
                sendDataer += myIP + "#";
                sendDataer += acceptIP + "#";
                sendDataer += fileName;       

                byte[] packeter = Encoding.Default.GetBytes(sendDataer);
                AsynchronousClient.send_Function(packeter);
                      

                // Close the socket if the client disconnected.
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                //=======================================================================
                //2015 02 04 추가내용
                sendThread.Abort();
                //=======================================================================

            }
            finally
            {
                // Signal when the file chunk has sent to all the clients successfully. 
                if (signal >= ClientsToSend.Count)
                {
                    sendDone.Set();
                    Console.WriteLine("파일 전송 중!!");
                }

            }

        }
        #endregion
        #region RemoveClient
        private static void RemoveClient(IList<Socket> listSocket)
        {
            if (listSocket.Count > 0)
            {
                foreach (Socket socket in listSocket)
                {
                    Clients.Remove(socket);
                    ClientsToSend.Remove(socket);
                }
            }
        }
        #endregion

        #region FunTion
        private static void ClientDisconnect()
        {
            Clients.Clear();
            ClientsToSend.Clear();
        }
        #endregion
    }
}
