using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

//============================
using Module.Server;
using System.Threading;
using System.Net;
using System.Timers;

namespace Module
{
    public delegate void Receive(Socket s, string[] packetType, string[] packetData);

    class Program
    {
        static CServer server = null;       //모듈서버
        static public List<Socket> admins = new List<Socket>();    //관리자 리스트
        static public List<Socket> f_servers = new List<Socket>(); //플로어 서버 리스트
        

        static public void receive(Socket s, string[] packetType, string[] packetData)
        {
            //TOMODULE   ==> 모듈내에서 우선처리
            //TOSERVER   ==> 어드민에서 플로어 서버로
            //TOADMIN    ==> 플로어에서 어드민으로
           
            #region 모듈
            if (packetType[0] == "TOMODULE")
            {
                #region 어드민 로그인,로그아웃
                if (packetType[1] == "ADMIN_LOGIN")
                {
                    IPEndPoint ip = (IPEndPoint)s.RemoteEndPoint;
       
                    for (int i = 0; i < admins.Count; i++)
                    {
                        IPEndPoint ip2 = (IPEndPoint)admins[i].RemoteEndPoint;
                        if (ip.Address.ToString() == ip2.Address.ToString())
                        {
                            Console.WriteLine("~~~~~~~~~~어드민 중복로그인~~~~~~~~~~~~~~");
                            return;
                        }
                    }
               
                    // 연결되지 않은 관리자 체크 후 삭제
                    admin_DisConnect_Check();
                    // 연결 된 소켓 admins 리스트에 추가

                    admins.Add(s);
                   
                    Console.WriteLine("[어드민 " + ip.Address + "//" + ip.Port + "로그인] ==> 카운트 " + admins.Count);
                }

                if (packetType[1] == "ADMIN_LOGOUT")
                {
                    if (admins.Count == 0)
                        return;

                    for (int i = 0; i < admins.Count; i++)
                    {
                        if (s == admins[i])
                        {
                            admins.Remove(s);
                            Console.WriteLine("[어드민 로그아웃] ==> 카운트 : " + admins.Count);
                            return;
                        }
                    }                 
                    
                }
                #endregion

                #region 플로어 로그인,로그아웃
                if (packetType[1] == "FLOOR_LOGIN")
                {
                    IPEndPoint ip = (IPEndPoint)s.RemoteEndPoint;

                    for (int i = 0; i < f_servers.Count; i++)
                    {
                        IPEndPoint ip2 = (IPEndPoint)f_servers[i].RemoteEndPoint;
                        if (ip.Address.ToString() == ip2.Address.ToString())
                        {
                            Console.WriteLine("~~~~~~~~~~플로어 중복로그인~~~~~~~~~~~~~");
                            return;
                        }
                    }
                        
                    // 연결되지 않은 플로어서버 체크 후 삭제
                    floor_DisConnect_Check();
                    // 연결 된 소켓 f_servers 리스트에 추가
                    f_servers.Add(s);
                    
                    Console.WriteLine("[플로어 " + ip.Address + "//" +ip.Port+  "로그인] ==> 카운트 : " + f_servers.Count);
                }

                
               
                if (packetType[1] == "FLOOR_LOGOUT")
                {
                    if (f_servers.Count == 0)
                        return;

                    for (int i = 0; i < f_servers.Count; i++)
                    {
                        if (s == f_servers[i])
                        {
                            f_servers.Remove(s);
                            Console.WriteLine("[플로어 로그아웃] ==> 카운트 : " + f_servers.Count);
                            return;
                        }
                    }

                }
                #endregion
                
                //업데이트보내기
                #region ADMIN_FLOOR_ADD db생성
               
                if (packetType[1] == "ADMIN_FLOOR_ADD")
                {
                    List<CFloor> floors = null;
                    MyDB mdb = new MyDB(packetData[0], packetData[1]);
                    mdb.SearchDB();
                    mdb.CreateDatabase();
                    
                    floors = mdb.SelectFloor(packetData[1]);

                    int floorTypeLen = 4;

                    string sendData = string.Empty;

                    sendData += "ADMIN_FLOOR_ADD+";    //패킷 타입.
                    sendData += (floors.Count * floorTypeLen).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                    for (int i = 0; i < floors.Count; i++)
                    {
                        sendData += floors[i].Floor_NAME + "#";
                        sendData += floors[i].Floor_IP + "#";     //정보.     
                        sendData += floors[i].Floor_NUM + "#";         //정보.             
                        sendData += floors[i].Floor_Power;

                        if (i + 1 != floors.Count)
                            sendData += "#";
                    }

                    sendData += "&";    //마지막 .

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }

                    if (sendData == null || admins.Count == 0)
                    {
                        Console.WriteLine("업데이트 오류(디비생성) ==> 메세지가 없거나 로그인한 어드민이 없습니다.");
                        return;
                    }

                    sendData = string.Empty;
                    //여기서 업데이트 실행한다.
                    MyDB mdb2 = new MyDB();
                    mdb.SearchDB();
                    sendData = mdb.admin_Upadate();

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }
                }
                #endregion

                //업데이트보내기
                #region ADMIN_FLOOR_MIN db삭제
                if (packetType[1] == "ADMIN_FLOOR_MIN")
                {
                    string f_ip = string.Empty;
                    List<CFloor> floors = null;

                    MyDB mdb = new MyDB();
                    mdb.SearchDB();
                    floors = mdb.FloorsGet();

                    MyDB mdb1 = new MyDB(packetData[0]);
                    mdb1.SearchDB();
                    mdb1.DeleteDatabase();
                   
                     
                    for (int i = 0; i < floors.Count; i++)
                    {
                        if (packetData[0] == floors[i].Floor_IP)
                        {
                            f_ip = floors[i].Floor_IP;
 
                        }
                    }
                              
                    string sendData = string.Empty;
                    sendData += "ADMIN_FLOOR_MIN+";    //패킷 타입.
                    sendData += f_ip;
                    sendData += "&";    //마지막 .

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }

                    sendData = string.Empty;
                    //여기서 업데이트 실행한다.
                    MyDB mdb2 = new MyDB();
                    mdb.SearchDB();
                    sendData = mdb.admin_Upadate();

                    if (sendData == null || admins.Count == 0)
                    {
                        Console.WriteLine("업데이트 오류(디비삭제) ==> 메세지가 없거나 로그인한 어드민이 없습니다.");
                        return;
                    }

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }

                }
                #endregion

                //업데이트보내기
                #region 층 정보 얻기
                if (packetType[1] == "ADMIN_FLOORS_GET")
                {
                    List<CFloor> floors = null;

                    MyDB mdb = new MyDB();
                    mdb.SearchDB();
                    floors = mdb.FloorsGet();

                    int floorTypeLen = 4;

                    string sendData = string.Empty;
                    sendData += "ADMIN_FLOORS_GET+";    //패킷 타입.
                    sendData += (floors.Count * floorTypeLen).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                    for (int i = 0; i < floors.Count; i++)
                    {
                        sendData += floors[i].Floor_NUM + "#";         //정보.
                        sendData += floors[i].Floor_IP + "#";     //정보.
                        sendData += floors[i].Floor_NAME + "#";
                        sendData += floors[i].Floor_Power;
                        if (i + 1 != floors.Count)          //마지막 정보에는 #을 제외하기 위한 코드
                            sendData += "#";
                    }
                    sendData += "&";    //마지막 .

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }

                    sendData = string.Empty;
                    //여기서 업데이트 실행한다.
                    MyDB mdb2 = new MyDB();
                    mdb.SearchDB();
                    sendData = mdb.admin_Upadate();

                    if (sendData == null || admins.Count == 0)
                    {
                        Console.WriteLine("업데이트(플로어겟) 오류 ==> 메세지가 없거나 로그인한 어드민이 없습니다.");
                        return;
                    }

                    for (int k = 0; k < admins.Count; k++)
                    {
                        server.Send(admins[k], sendData);
                    }

                }
                #endregion 

            }
            #endregion

            #region toserver
            if (packetType[0] == "TOSERVER")
            {
                //================================
                //메세지분류
                string sendData = string.Empty;
                sendData += packetType[1] + "+";

                for (int i = 0; i < packetData.Length; i++)
                {
                    sendData += packetData[i];
                    if (i + 1 < packetData.Length)
                        sendData += "#";
                }
                sendData += "&";
                //=================================

                if (f_servers.Count == 0 || sendData ==null)
                {
                    Console.WriteLine("플로우가 없다 or 메세지 값이 널.................");
                    return;
                }

                for (int k = 0; k < f_servers.Count; k++) 
                {
                    IPEndPoint FloorIp = (IPEndPoint)f_servers[k].RemoteEndPoint;
                    if (FloorIp.Address.ToString() == packetType[2])
                    {
                        server.Send(f_servers[k], sendData);
                        break;
                    }
                }


                Console.WriteLine("[모듈]===>[플로어] 보낸다: " + sendData);
                Console.WriteLine("--------------------------------------------------------");
            }

            #endregion

            #region toadmin
            if (packetType[0] == "TOADMIN")
            {
                string sendData = string.Empty;
                sendData += packetType[1] + "+";

                for (int i = 0; i < packetData.Length; i++)              
                {
                    if (packetType[1] == "NEEDLESS_SOFTWARELIST") //서버로 보내는 경우
                    {
                           sendData += packetData[i];
                            if (i + 1 < packetData.Length)
                                sendData += "☆";
                    }

                    else 
                    { 
                    sendData += packetData[i];
                    if (i + 1 < packetData.Length)
                        sendData += "#";
                    }
                }
                sendData += "&";


                if (packetType[1] == "UPDATE")
                {
                    //Console.WriteLine
                    sendData = string.Empty;
                    //여기서 업데이트 실행한다.
                    MyDB mdb = new MyDB();
                    mdb.SearchDB();
                    sendData=mdb.admin_Upadate();                    
                }

                if (sendData == null || admins.Count ==0)
                {
                    Console.WriteLine("업데이트 오류 ==> 메세지가 없거나 로그인한 어드민이 없습니다.");
                    return;
                }
                for (int k = 0; k < admins.Count; k++)
                {
                    server.Send(admins[k], sendData);
                }
                Console.WriteLine("[모듈]===>[어드민] 보낸다: " + sendData);
                Console.WriteLine("--------------------------------------------------------");

            }
            #endregion

        }

        #region 관리자 소켓 체크후 연결되지 않았다면 삭제.
        static void admin_DisConnect_Check()
        {
            List<Socket> deleteList = new List<Socket>();
            for (int i = 0; i < admins.Count; i++)
            {
                if (admins[i].Connected == false)
                    deleteList.Add(admins[i]);
            }

            for (int k = 0; k < deleteList.Count; k++)
                admins.Remove(deleteList[k]);

            deleteList.Clear();
        }
        #endregion

        #region 플로어서버 소켓 체크후 연결되지 않았다면 삭제.
        static void floor_DisConnect_Check()
        {
            List<Socket> deleteList = new List<Socket>();
            for (int i = 0; i < f_servers.Count; i++)
            {
                if (f_servers[i].Connected == false)
                    deleteList.Add(f_servers[i]);
            }

            for (int k = 0; k < deleteList.Count; k++)
                f_servers.Remove(deleteList[k]);

            deleteList.Clear();
        }
        #endregion


        static void Main(string[] args)
        {

            Console.WriteLine("====통합모듈 시작====");
            server = new CServer(receive, 10000);
            Thread workerThread = new Thread(server.StartListening);
            workerThread.IsBackground = true;
            workerThread.Start();

            while (true)
            {

            }

        }
    }
}
