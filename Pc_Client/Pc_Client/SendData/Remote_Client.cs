using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pc_Client.SendData
{
    public delegate void Remote_Receive(Socket s, byte[] packetData);
    
    public class Remote_Client
    {
          #region Member
        private Socket client;
        private Remote_Receive receive_Function;                   //리시브 델리게이트
   
        private string server_IP = string.Empty;
        private int server_Port = 0;
   
        System.Threading.ManualResetEvent start = new System.Threading.ManualResetEvent(false);
          #endregion

        public Remote_Client(Remote_Receive receive_Function)
        {
            this.receive_Function = receive_Function;
        }

        public bool Check()
        {
            if (client == null)
                return false;

            if (!client.Connected)
                return false;

            return true;
        }

        public bool Start(string ip, int port)
        {

            server_IP = ip;
            server_Port = port;

            start.Reset();
            Thread ReceiveT = new Thread(new ParameterizedThreadStart(SeverStartThread));
            ReceiveT.IsBackground = true;
            ReceiveT.Start(ReceiveT);

            if (start.WaitOne())
            {
                Console.WriteLine("============================");
                Console.WriteLine("원격 접속 성공");
                Console.WriteLine("============================");

                Thread thread = new Thread(new ParameterizedThreadStart(ReceiveThread));
                thread.IsBackground = true;
                thread.Start(client);

                //서버로부터 보내기=========================================================
                //TODO : PC_NAME도 같이 보내야한다.
                //IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                //string myIP = IPHost.AddressList[0].ToString();
                //=====================================================================
            }
            return true;
        }


        #region funtion

        public void ReceiveThread(object s)
        {
            byte[] data;
            Socket client = (Socket)s;
            try
            {
                while (true)
                {
                    //======================================================
                    ReceiveData(out data);
                    receive_Function(client, data);
                    //======================================================
                    string temp = Encoding.Default.GetString(data);

                   // if (temp == "REMOTE_END") { client.Close(); }

                }
            }
            catch (Exception)
            {
                if (Thread.CurrentThread.IsAlive) {
                    Thread.CurrentThread.Abort();
                }
                Console.WriteLine("==============================");
                Console.WriteLine("서버가 종료되었습니다.. 재 접속 시도중...");
                Console.WriteLine("==============================");
                client.Close();
            }

            finally
            {
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("원격 소켓이 종료됨 : {0}주소, {1}포트 종료", ip.Address, ip.Port);
                //TODO : receive_Function()을 이용한, admin계정이 닫혔으면 제거하기.
                //string[] packetType = new string[1];
                //string[] packetData = new string[1];
                //packetType[0] = "REMOTE_END";
                //packetData[0] = ip.Address.ToString();

                //receive_Function(client, data);
            
                client.Shutdown(SocketShutdown.Both);
                client.Close();         //  소켓 연결 끊기
            }
        }
        public void SendData(byte[] data)
        {
            try
            {
                int total = 0;
                int size = data.Length;
                int left_data = size;
                int send_data = 0;

                // 전송할 데이터의 크기 전달
                byte[] data_size = new byte[4];
                data_size = BitConverter.GetBytes(size);
                send_data = this.client.Send(data_size);

                // 실제 데이터 전송
                while (total < size)
                {
                    send_data = this.client.Send(data, total, left_data, SocketFlags.None);
                    total += send_data;
                    left_data -= send_data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public int ReceiveData(out byte[] data)
        {
            try
            {
                int total = 0;
                int size = 0;
                int left_data = 0;
                int recv_data = 0;

                // 수신할 데이터 크기 알아내기 
                byte[] data_size = new byte[4];
                recv_data = client.Receive(data_size, 0, 4, SocketFlags.None);
                size = BitConverter.ToInt32(data_size, 0);
                left_data = size;

                data = new byte[size];

                // 실제 데이터 수신
                while (total < size)
                {
                    recv_data = client.Receive(data, total, left_data, 0);
                    if (recv_data == 0) break;
                    total += recv_data;
                    left_data -= recv_data;
                }
                return size;
            }
            catch (Exception ex)
            {
                Thread.CurrentThread.Abort();
                data = new byte[1];
                return 0;
            }
        }

        //재접속 스레드.
        public void SeverStartThread(object s)
        {
            Thread thread = s as Thread;

            while (true)
            {
                try
                {
                    IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(server_IP), server_Port);

                    if (client != null)
                    {
                        client.Close();
                        client = null;
                    }
                    client = new Socket(AddressFamily.InterNetwork,
                                             SocketType.Stream, ProtocolType.Tcp);
                    client.Connect(ipep);  //  server_IP 서버 7000번 포트에 접속시도
                    start.Set();
                    return;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "원격 연결 오류");
                    if (client == null)
                        continue;

                    if (!client.Connected)
                        continue;

                    client.Close();
                    continue;
                }

            }
        }
        #endregion
        
         
    }
}
