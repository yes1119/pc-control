using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floor_Server.Server
{
    public delegate void Send(string data);

    public class StateObject
    {

        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 16384;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    class CClient
    {
        private string myip = string.Empty;
        private string serverIP = string.Empty;
        private int port = 0;
        public Socket client = null;        //private로 변경해야된다.
        private string sendmsg;

        // ManualResetEvent instances signal completion.
        public ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private Receive receive = null;

        //연결상태를 확인.
        public bool connect_State()
        {
            if (client.Connected)
                return true;
            else
                return false;
        }

        public CClient(string serverIP, int port, Receive receive_Fun)
        {
            this.port = port;
            this.serverIP = serverIP;
            this.receive = receive_Fun;
            // Center_Server.UserControls.Window1.dbsend += Send;
            // Center_Server.UserControls.WindowAccount.dbsend1 += Send;
        }

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                IPHostEntry ipHostInfo = Dns.Resolve(serverIP);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);

                connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("통합모듈과 연결 완료 ({0})",
                    client.RemoteEndPoint.ToString());
                Console.WriteLine("--------------------------------------------------------");

                Thread workerThread = new Thread(Receive);
                workerThread.IsBackground = true;
                workerThread.Start(client);

                //==========================================================
                //플로어 로그인.
                //모듈로보냄
                string sendData0 = string.Empty;
                sendData0 += "TOMODULE+";
                sendData0 += "FLOOR_LOGIN+";
                sendData0 += "&";
                Send(client, sendData0);

                connectDone.Set();
            }
            catch (Exception)
            {
                Console.WriteLine("재접속 중...");
                Socket client = (Socket)ar.AsyncState;
                IPHostEntry ipHostInfo = Dns.Resolve(serverIP);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
            }
        }

        private void Receive(object client)
        {
            Socket s = client as Socket;
            while (true)
            {
                try
                {
                    receiveDone.Reset();
                    // Create the state object.
                    StateObject state = new StateObject();
                    state.workSocket = s;

                    // Begin receiving the data from the remote device.
                    s.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    receiveDone.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("서버와의 연결 종료 :"+ ex.Message);
                    s.Shutdown(SocketShutdown.Both);
                    s.Close();
                    StartClient();
                }
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

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));


                    //받은 데이터 타입 여기에 나열.
                    string response = state.sb.ToString();
                    string[] str = response.Split('&');
                    for (int i = 0; i < str.Length - 1; i++)        //마지막은 항상 비어있다.
                    {
                        Console.WriteLine("[통합모듈]===>[플로어] : " + str[i]);
                        Console.WriteLine("--------------------------------------------------------");
                        string[] packetType = str[i].Split('+');
                        string[] packetData = packetType[1].Split('#');
                        receive(client, packetType, packetData);

                    }
                    state.sb.Clear();
                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);

                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        string response = state.sb.ToString();
                        Console.WriteLine(response);
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("서버와의 연결 종료" + e.Message);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                StartClient();
            }
        }

        public void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        public void Send(String data)
        {
            if (client.Connected != true)
                return;

            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.Default.GetBytes(data);
            sendmsg = data;

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("[플로어]===>[통합모듈] :" + sendmsg + " {0} bytes 를 보냈다.", bytesSent);
                Console.WriteLine("--------------------------------------------------------");

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
