using System;
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
    public static class AsynchronousClient
    {
        #region Members
        public static Send send_Function = null;

        public static string myIP = string.Empty;
        public static string FILEname = string.Empty;
        private static string fileName;
        private static string fileSavePath = "C:/";
        private static long fileLen;
        private static long downData = 0;
        private static int count = 0;
        private static AutoResetEvent connectDone = new AutoResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        private static bool connected = false;

        private delegate void ProgressChangeHandler();
        private delegate void FileReceiveDoneHandler();
        private delegate void ConnectDoneHandler();
        private delegate void EnableConnectButtonHandler();
        private delegate void SetProgressLengthHandler(int len);
        static Socket clientSocket;
        #endregion

        #region Properties

        //public static Client Client { get; set; }
        public static IPAddress IpAddress { get; set; }
        public static int Port { get; set; }
        public static string FileSavePath
        {
            get
            {
                return fileSavePath;
            }
            set
            {
                fileSavePath = value.Replace("\\", "/");
            }
        }

        #endregion

        public static void StartClient()
        {
            connected = false;
            if (IpAddress == null)
            {
                return;
            }

            IPEndPoint remoteEP = new IPEndPoint(IpAddress, Port);

            // Use IPv4 as the network protocol,if you want to support IPV6 protocol, you can update here.
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Begin to connect the server.
            clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);
            connectDone.WaitOne();

            if (connected)
            {
                // Begin to receive the file after connecting to server successfully.
                Console.WriteLine("접속 성공");
                Receive(clientSocket);
                receiveDone.WaitOne();

                // Notify the user whether receive the file completely.파일을 완전히 받을 때 사용자 에게 알립니다.
                // Client.BeginInvoke(new FileReceiveDoneHandler(Client.FileReceiveDone));

                // Close the socket.
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            else
            {
                Thread.CurrentThread.Abort();
            }
        }
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = (Socket)ar.AsyncState;

                clientSocket.EndConnect(ar);
            }
            catch
            {
                Console.WriteLine("접속 실패...");
                IPEndPoint remoteEP = new IPEndPoint(IpAddress, Port);

                // Use IPv4 as the network protocol,if you want to support IPV6 protocol, you can update here.
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Begin to connect the server.
                clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);
                return;
            }

            connected = true;
            connectDone.Set();
        }
        #region Receive
        private static void Receive(Socket clientSocket)
        {
            StateObject state = new StateObject();
            state.WorkSocket = clientSocket;

            ReceiveFileInfo(clientSocket);

            int progressLen = checked((int)(fileLen / StateObject.BufferSize + 1));
            object[] length = new object[1];
            length[0] = progressLen;
            // Client.BeginInvoke(new SetProgressLengthHandler(Client.SetProgressLength), length);

            // Begin to receive the file from the server.
            //서버에서 파일을 수신 하기 시작합니다.
            try
            {
                clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch
            {
                if (!clientSocket.Connected)
                {
                    HandleDisconnectException();
                }
            }
        }
        private static void ReceiveFileInfo(Socket clientSocket)
        {
            // Get the filename length from the server.
            byte[] fileNameLenByte = new byte[4];
            try
            {
                clientSocket.Receive(fileNameLenByte);
            }
            catch
            {
                if (!clientSocket.Connected)
                {
                    HandleDisconnectException();
                }
            }
            int fileNameLen = BitConverter.ToInt32(fileNameLenByte, 0);

            // Get the filename from the server.
            byte[] fileNameByte = new byte[fileNameLen];

            try
            {
                clientSocket.Receive(fileNameByte);
            }
            catch
            {
                if (!clientSocket.Connected)
                {
                    HandleDisconnectException();
                }
            }

            fileName = Encoding.Unicode.GetString(fileNameByte, 0, fileNameLen);
            fileSavePath = fileSavePath + "/" + fileName;

            // Get the file length from the server.
            byte[] fileLenByte = new byte[8];
            clientSocket.Receive(fileLenByte);
            fileLen = BitConverter.ToInt64(fileLenByte, 0);
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientSocket = state.WorkSocket;
            BinaryWriter writer;

            int bytesRead = clientSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                //If the file doesn't exist, create a file with the filename got from server. If the file exists, append to the file.
                if (!File.Exists(fileSavePath))
                {
                    writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Create));
                }
                else
                {
                    //TODO: 다운받고있는 파일을 우클릭-속성 클릭시 에러 발생.
                    writer = new BinaryWriter(File.Open(fileSavePath, FileMode.Append));
                }

                writer.Write(state.Buffer, 0, bytesRead);
                writer.Flush();
                writer.Close();

                downData += bytesRead;
                double result = (double.Parse(downData.ToString()) / double.Parse(fileLen.ToString())) * 100;
                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                string myIP = IPHost.AddressList[0].ToString();
                //클라서버에게 전달
                if (result > double.Parse(count.ToString()))
                {

                    // Notify the progressBar to change the position.
                    //Client.BeginInvoke(new ProgressChangeHandler(Client.ProgressChanged));

                    string[] data_Type = fileName.Split('.');

                    Console.WriteLine(result.ToString() + " : " + count.ToString());
                    count++;
                    int result1 = (int)result;
                    if (result1 % 3 == 0)
                    {
                        string sendDataer = string.Empty;
                        sendDataer += "CLIENT_FILE_PROGRESS+";    //패킷 타입.
                        sendDataer += myIP + "#";
                        sendDataer += result1.ToString() + "#";
                        sendDataer += FILEname + "#"; //파일이름
                        sendDataer += IpAddress; //서버 아이피

                        byte[] packeter = Encoding.Default.GetBytes(sendDataer);
                        send_Function(packeter);
                    }

                }
                // Recursively receive the rest file.
                try
                {
                    if (result >= 100)
                    {
                        Thread.Sleep(100);
                        fileLen = 0;
                        downData = 0;
                        count = 0;
                        Console.WriteLine("다운로드 완료");
                        int result1 = (int)result;

                        //Floor서버에게 전달.
                        string sendDataer = string.Empty;
                        sendDataer += "CLIENT_FILE_PROGRESS+";    //패킷 타입.
                        sendDataer += myIP + "#";
                        sendDataer += result1.ToString() + "#";
                        sendDataer += FILEname + "#"; //파일이름
                        sendDataer += IpAddress; //서버 아이피

                        byte[] packeter = Encoding.Default.GetBytes(sendDataer);
                        send_Function(packeter);                   

                        Thread.CurrentThread.Abort();
                        clientSocket.EndDisconnect(ar);

                    }
                    clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                catch
                {
                    if (!clientSocket.Connected)
                    {
                        Console.WriteLine("종료1");

                        fileLen = 0;
                        downData = 0;
                        count = 0;

                        string sendDataer = string.Empty;
                        sendDataer += "CLIENT_CLIENT_FAIL+";    //패킷 타입.
                        sendDataer += myIP + "#";
                        sendDataer += IpAddress + "#";
                        sendDataer += fileName;                      


                        byte[] packeter = Encoding.Default.GetBytes(sendDataer);
                        send_Function(packeter);

                        //파일 삭제
                        FileInfo fileInfo = new FileInfo("D:\\" + fileName);
                        fileInfo.Delete();
                    }
                }
            }
            else
            {
                // Signal if all the file received.
                receiveDone.Set();
                if (count < 100)
                {
                    while (count <= 100)
                    {
                        count++;
                    }
                }
            }
        }
        private static void HandleDisconnectException()
        {
            Thread.CurrentThread.Abort();
        }
        #endregion
    }
}
