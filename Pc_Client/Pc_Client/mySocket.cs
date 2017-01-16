using Pc_Client.SendData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pc_Client
{
    public delegate void Receive(Socket s,string packetType, string[] packetData);
   // public delegate void Send222(byte[] data);
    class mySocket
    {
        #region Member
        private Socket client;
        private Receive receive_Function;
        //private Send222 send_Function;


        private Funtion fun;
        private string server_IP = string.Empty;
        private int server_Port = 0;
        System.Threading.ManualResetEvent start = new System.Threading.ManualResetEvent(false);
        #endregion

        public mySocket()
        {     
            AsynchronousClient.send_Function = SendData;
            CControl.send_Function = SendData;
            Funtion.device_send = SendData;
        }

        public bool Check()
        {
            if (client == null)
                return false;

            if (!client.Connected)
                return false;

            return true;
        }

        public bool Start(string ip, int port, Receive receive_Function)
        {
            fun = new Funtion();
            this.receive_Function = receive_Function;
         
            server_IP = ip;
            server_Port = port;

            start.Reset();
            Thread ReceiveT = new Thread(new ParameterizedThreadStart(SeverStartThread));
            ReceiveT.IsBackground = true;
            ReceiveT.Start(ReceiveT);

            if (start.WaitOne())
            {
                Console.WriteLine("============================");
                Console.WriteLine("접속 성공");
                Console.WriteLine("============================");

                Thread thread = new Thread(new ParameterizedThreadStart(ReceiveThread));
                thread.IsBackground = true;
                thread.Start(client);

                //서버로부터 보내기=========================================================
                //TODO : PC_NAME도 같이 보내야한다.
                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                string myIP = IPHost.AddressList[0].ToString();

                string sendData1 = string.Empty;
                sendData1 += "CLIENT_LOGIN+";
                sendData1 += myIP + "#";     //서버 주소.
                sendData1 += fun.GetMacAddress().ToString() + "#";          // MAC 주소
                sendData1 += fun.GetPcName().ToString()+"#";                    // PC 이름
                sendData1 += Window1.Client_type + "#";
                sendData1 += Funtion.f_registry() + "#";
                sendData1 += MainWindow.software;
                

                //byte로 변환후 보내기.
                byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                SendData(packet1);
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
                    //받을때
                    ReceiveData(out data);
                    
                    string temp = Encoding.Default.GetString(data);
                    

                    string[] str = temp.Split('+');
                    string[] packetData;
                    if(str[0] == "NEEDLESS_CLIENT_SOFTWARE")
                    {
                        packetData = temp.Split('★');
                        packetData = packetData[1].Split('☆');
                    }
                    else
                    {
                        packetData = str[1].Split('#');
                    }
                    receive_Function(client, str[0], packetData);
                    
                    
                    //======================================================
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("==============================");
                Console.WriteLine("서버가 종료되었습니다.. 재 접속 시도중...");
                Console.WriteLine("==============================");
                client.Close();
                Start(server_IP, server_Port, receive_Function);
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
                Console.WriteLine(total);
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
                Console.WriteLine(ex.Message);
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
                    Thread.Sleep(100);
                    client.Connect(ipep);  //  server_IP 서버 7000번 포트에 접속시도
                    start.Set();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "연결 오류");
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
