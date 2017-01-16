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
    class CServer
    {    
        private Socket server = null;                      //서버소켓
        private List<Socket> clients = new List<Socket>();  //클라이언트 소캣
        private Receive receive_Function;                   //리시브 델리게이트

        // 최초 스타트
        public void Start(int port, Receive receive_Function)
        {
            this.receive_Function = receive_Function;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipep);
            server.Listen(20);

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

                    Console.WriteLine("========================================");
                    Console.WriteLine("{0}주소, {1}포트 클라이언트접속", ip.Address, ip.Port);
                    Console.WriteLine("========================================");

                    //깨어난 PC는 PC_ON 스레드에서 종료
                    for (int k = 0; k < Program.pc_On_Mac.Count; k++)
                    {
                        if (Program.pc_On_Mac[k].ip == ip.Address.ToString())
                            Program.pc_On_Mac.RemoveAt(k);
                    }


                    Thread thread = new Thread(new ParameterizedThreadStart(ReceiveThread));
                    thread.IsBackground = true;
                    thread.Start(client);
                }
            }
            finally
            {
                server.Close();
            }
        }

        // 받는 쓰레드.
        public void ReceiveThread(object s)
        {//<
            Socket client = (Socket)s;
            try
            {
                while (true)
                {
                    byte[] data = null;
                    int retval = ReceiveData(client, out data);
                    if (retval != 0)   // 수신한 문자열이 있으면 화면에 출력
                    {
                        //받기
                        string temp = Encoding.Default.GetString(data);

                        string[] str = temp.Split('+');
                        string[] packetData = str[1].Split('#');
                        Console.WriteLine("[클라이언트==>플로어서버] :" +temp);

						if (str[0] == "GETPROCESS_RESULT" || str[0] == "NEEDLESS_CLIENT_SOFTWARERESULT")
						{
							string[] str1 = temp.Split('★');
							string[] packetData1 = str1[1].Split('☆');
							receive_Function(client, str, packetData1);
						}

						else 
						{ 
                        receive_Function(client, str, packetData);       //리슨 델리게이트 생성.
						}
					}
                    else
                    {
                        /*
                        IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                        Console.WriteLine("소켓이 종료됨 : {0}주소, {1}포트 종료", ip.Address, ip.Port);
                        //TODO : receive_Function()을 이용한, admin계정이 닫혔으면 제거하기.
                        string[] packetType = new string[2];
                        string[] packetData = new string[2];
                        packetType[0] = "UP";
                        packetType[1] = "CLIENT_LOGOUT";
                        packetData[0] = ip.Address.ToString();
                        receive_Function(client, packetType, packetData);
                        clients.Remove(client);
                        */
                        break;
                    }
                }
            }


            finally
            {//<
                IPEndPoint ip = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("소켓이 종료됨 : {0}주소, {1}포트 종료", ip.Address, ip.Port);
                //TODO : receive_Function()을 이용한, admin계정이 닫혔으면 제거하기.
                string[] packetType = new string[1];
                string[] packetData = new string[1];
                packetType[0] = "CLIENT_LOGOUT";
                packetData[0] = ip.Address.ToString();
                
                receive_Function(client, packetType, packetData);
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
                Console.WriteLine(ex.Message);
                data = new byte[1];
                return 0;
            }
        }

        //연결 체크
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
                    //TODO : receive_Function()을 이용한, admin계정이 닫혔으면 제거하기.
                    string[] packetType = new string[2];
                    string[] packetData = new string[2];
                    packetType[0] = "CLIENT_LOGOUT";
                    packetData[0] = ip.Address.ToString();
                    receive_Function(client, packetType, packetData);
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
            Console.WriteLine(data.Length);
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
                Console.WriteLine("소켓이 종료됨 : {0}주소, {1}포트 종료", _ip.Address, _ip.Port);
                //TODO : receive_Function()을 이용한, admin계정이 닫혔으면 제거하기.
                string[] packetType = new string[2];
                string[] packetData = new string[2];
                packetType[0] = "CLIENT_LOGOUT";
                packetData[0] = _ip.Address.ToString();
                receive_Function(client, packetType, packetData);
                clients.Remove(client);
                client.Shutdown(SocketShutdown.Both);//<
                client.Close();         //  소켓 연결 끊기
            }
        }
    }
}
