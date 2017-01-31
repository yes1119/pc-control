using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Admin.Client
{
    public delegate void Receive(string packetType, string[] packetData);
    public delegate void Send(string data);
    
    public class StateObject
    {
        string sendmsg = string.Empty;
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
        //스태틱으로 리시브 함수 호출.
        string sendmsg = string.Empty;
        public static event Receive receive_E;

        private string serverIP = string.Empty;
        private int port = 0;
        public Socket client = null;        //private로 변경해야된다.

        // ManualResetEvent instances signal completion.
        public ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        //연결상태를 확인.
        public bool connect_State()
        {
            if (client.Connected)
                return true;
            else
                return false;
        }


        public CClient(string serverIP, int port)
        {
            this.port = port;
            this.serverIP = serverIP;
            Admin.UserControls.Window1.dbsend += Send;
            Admin.UserControls.WindowAccount.dbsend1 += Send;
            Admin.Dialog_Get.Software.dbsend1 += Send;
            Admin.Dialog_Get.PcTimerSelectDel.dbsend2 += Send;
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
                //관리자 계정 로그인.
                string sendData = string.Empty;
                sendData += "TOMODULE+";
                sendData += "ADMIN_LOGIN+";
                sendData += "&";
                Send(client, sendData);
                //==========================================================

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine("재접속 중... : " + e.Message);
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
                catch (Exception e)
                {
                    Console.WriteLine("통합모듈과의 연결 종료 :" + e.Message);
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
                        Console.WriteLine("[통합모듈]===>[관리자] : " + str[i]);
                        Console.WriteLine("--------------------------------------------------------");
                        string[] packetType = str[i].Split('+');
                        string[] packetData = packetType[1].Split('#');

						if (packetType[0] == "NEEDLESS_SOFTWARELIST")
						{
							string[] packetData1 = str[i].Split('☆');
							packetData1[0] = packetData1[0].Split('+')[1];
							receive_E(packetType[0], packetData1);
						}
						else
						{
							receive_E(packetType[0], packetData);
						}
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
                Console.WriteLine("통합모듈과의 연결 종료" +e.Message);
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
                Console.WriteLine("[관리자]===>[통합모듈] : " + sendmsg + " {0} bytes 를 보냈다.", bytesSent);
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
