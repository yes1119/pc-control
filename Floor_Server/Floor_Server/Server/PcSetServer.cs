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
    public class PcSetServer
    {
        private Socket server = null;                      //서버소켓
        public List<Socket> clients = new List<Socket>();  //클라이언트 소캣
        private Receive receive_Function;                   //리시브 델리게이트

        // 최초 스타트
        public void Start(int port, Receive receive_Function)
        {
            this.receive_Function = receive_Function;
     
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);

            if (server == null)
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Bind(ipep);
                server.Listen(40);
            }

            Thread thread = new Thread(new ParameterizedThreadStart(ListenThread));
            thread.IsBackground = true;
            thread.Start();
        }

        // 클라이언트 접속 대기
        public void ListenThread(object s)
        {
            try
            {
                while (true)
                {
                    Socket client = server.Accept();  // 클라이언트 접속 대기

                    clients.Add(client);


                    IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                    Thread thread = new Thread(new ParameterizedThreadStart(ReceiveThread));
                    thread.IsBackground = true;
                    thread.Start(client);
                }
            }
            finally
            {
                server.Shutdown(SocketShutdown.Both);
                server.Close();
            }
        }

        // 받는 쓰레드.
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
                        int retval = ReceiveData(client, out data);
                        if (retval != 0)   // 수신한 문자열이 있으면 화면에 출력
                        {
                            string temp = Encoding.Default.GetString(data);

                            string[] str = temp.Split('+');
                            string[] packetData = str[1].Split('#');

                            IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                            //======================================================
                            receive_Function(client, str, packetData);
                        }
                        else
                        { 
                            break; 
                        }
                }
            }
            finally
            {
                clients.Remove(client);
                client.Shutdown(SocketShutdown.Both);
                client.Close();         //  소켓 연결 끊기
            }
        }

        public int ReceiveData(Socket client, out byte[] data)
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
               // System.Windows.Forms.MessageBox.Show(ex.Message);
                data = new byte[1];
                return 0;
            }
        }

        // 연결 체크
        public void SendCheck(byte[] data)
        {
            Socket client = null;
            // 클라이언트 소켓 유무 확인
            for (int i = 0; i < clients.Count; i++)
            {
                client = clients[i];

                try
                {
                    int total = 0;
                    int size = data.Length;
                    int left_data = size;
                    int send_data = 0;

                    // 전송할 데이터의 크기 전달
                    byte[] data_size = new byte[4];
                    data_size = BitConverter.GetBytes(size);
                    send_data = client.Send(data_size);

                    // 실제 데이터 전송
                    while (total < size)
                    {
                        send_data = client.Send(data, total, left_data, SocketFlags.None);
                        total += send_data;
                        left_data -= send_data;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                    Console.WriteLine("소켓이 종료됨 : {0}주소, {1}포트 종료", ip.Address, ip.Port);
                    clients.Remove(client);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();         //  소켓 연결 끊기
                }
            }
        }

        // 보내기
        public void SendData(string ip, byte[] data)
        {
            Socket client = null;
            for (int i = 0; i < clients.Count; i++)
            {
                IPEndPoint search_Ip = (IPEndPoint)clients[i].RemoteEndPoint;
                if (search_Ip.Address.ToString() == ip)
                {
                    client = clients[i];
                    break;
                }
            }
            if (client == null)
                return;
            //Console.WriteLine(data.Length);
            try
            {
                int total = 0;
                int size = data.Length;
                int left_data = size;
                int send_data = 0;

                // 전송할 데이터의 크기 전달
                byte[] data_size = new byte[4];
                data_size = BitConverter.GetBytes(size);
                send_data = client.Send(data_size);
                // 실제 데이터 전송
                while (total < size)
                {
                    send_data = client.Send(data, total, left_data, SocketFlags.None);
                    Console.WriteLine(send_data);
                    total += send_data;
                    left_data -= send_data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                IPEndPoint _ip = (IPEndPoint)client.RemoteEndPoint;
                //Console.WriteLine("소켓이 종료됨 : {0}주소, {1}포트 종료", _ip.Address, _ip.Port);
                clients.Remove(client);
                client.Shutdown(SocketShutdown.Both);
                client.Close();         //  소켓 연결 끊기
            }
        }
    }
}
