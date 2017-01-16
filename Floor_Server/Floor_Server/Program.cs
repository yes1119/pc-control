using Floor_Server.DataSend;
using Floor_Server.DB;
using Floor_Server.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Floor_Server
{
    public delegate void Receive(Socket s, string[] packetType, string[] packetData);
    public delegate void SendConnect(string floor_num, string room_num);

    public class file_pc
    {
        public List<pcinfo> pcs = new List<pcinfo>();
        public string file_path;
        public int i_check = 0;
    }

    public class pcinfo
    {
        public string ip;
        public bool check = false;
    }
    public class CPcOn
    {
        public string ip;
        public string mac;
    }


    class Program_Key
    {
        public string Key_Name { get; set; }
        public string Key_Path { get; set; }
        public string Key_Value { get; set; }
        public string Key_Sleep { get; set; }
    }
  

    class Program
    {
        static public string MyIp = null;
        static string[] packetData1;
        static System.Threading.ManualResetEvent dataSendEvent = new System.Threading.ManualResetEvent(false);
        static System.Threading.ManualResetEvent keySendEvent = new System.Threading.ManualResetEvent(false);
        static bool serverConnect = false;
        static public PcSetServer pc_server = null;       //층 서버
        static public CServer server = null;       //층 서버
        static public CClient client = null;       //모듈에 접속하는 클라
        static string ConString = string.Empty;     //석진
        static string[] persent;
        static bool Date_AlarmConnect = false;
        static bool Days_AlarmConnect = false;

        public static List<file_pc> fpc = new List<file_pc>();
        public static List<file_pc> kpc = new List<file_pc>();
        public static List<Program_Key> ks = new List<Program_Key>();
        static public List<CNoneFilePc> none_File_Pcs = new List<CNoneFilePc>();  //Exe파일 없는 리스트.
        public static List<CPcOn> pc_On_Mac = new List<CPcOn>();

        static System.Threading.ManualResetEvent pc_On_Event = new System.Threading.ManualResetEvent(false);

        //================================================================================================
        public static List<On_Date> on_Date = new List<On_Date>();//ㅅㅁ(수정)
        public static List<Off_Date> off_Date = new List<Off_Date>();//ㅅㅁ(수정)
        public static List<On_Days> on_Days = new List<On_Days>();//ㅅㅁ(수정)
        public static List<Off_Days> off_Days = new List<Off_Days>();//ㅅㅁ(수정)
        static SortedList<string, Stopwatch> sw = new SortedList<string, Stopwatch>();      //청원

        public static List<Ip_on> Date_ip_On = new List<Ip_on>();   //날짜켜기알람 ip 저장 ㅅㅁ(수정)
        public static List<Ip_off> Date_ip_Off = new List<Ip_off>();   //날짜끄기알람 ip저장 ㅅㅁ(수정)
        public static List<Ip_on> Days_ip_On = new List<Ip_on>();   //날짜켜기알람 ip 저장 ㅅㅁ(수정)
        public static List<Ip_off> Days_ip_Off = new List<Ip_off>();   //날짜끄기알람 ip저장 ㅅㅁ(수정)
        public static Thread onoffthread;
        static System.DateTime startDate;   //날짜를 저장
        static System.Timers.Timer timer;    //알람타이머
        public static string pc_set_ip = string.Empty;
        public static string pc_filepath = string.Empty;
        //================================================================================================
      
        //=======================================================
        //메시지 타입
        //ADMIN  : 관리 PC 에서 온 경우.
        //CLIENT  : 클라이언트 에서 온 경우.
        static public void receive(Socket s, string[] packetType, string[] packetData)
        {
            #region ADMIN

            #region 강의실 정보 얻기
            if (packetType[0] == "ADMIN_ROOMS_GET")
            {
                int floor_Num = int.Parse(packetData[0]);
                string floor_Name = packetData[1];

                try
                {
                    admin_Rooms_Get(floor_Name, floor_Num);

                    admin_Upadate();
                    insert_Log(floor_Name, "PC모니터링", "ADMIN_ROOMS_GET", floor_Name + " 층 강의실 정보 얻음 성공");
                    Console.WriteLine("ADMIN_ROOMS_GET 성공");
                }
                catch (Exception e)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                    Console.WriteLine("ADMIN_ROOMS_GET 실패 : " + e.Message);
                }
            }
            #endregion

            #region 강의실 추가
            if (packetType[0] == "ADMIN_ROOM_ADD")
            {
                string floor_Num = packetData[0];  //층 넘버
                string room_Name = packetData[1];  //룸 이름
                string room_Num = packetData[2];   //룸 번호
                string ip_Start = packetData[3];   //첫번째 pc ip
                string ip_End = packetData[4];     //마지막 pc ip
                string width_Com = packetData[5];  //pc 폭;
                string pc_type = "0";    //pc 타입

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    string[] first_Ip = ip_Start.Split('.');
                    string[] last_Ip = ip_End.Split('.');
                    int first = int.Parse(first_Ip[3]);
                    int last = int.Parse(last_Ip[3]);
                    int width = 134 - 5;
                    int height = 104 - 5;
                    int x = 0;
                    int y = 0;
                    int max_X = int.Parse(width_Com);

                    admin_Room_Add(floor_Num, room_Num, room_Name);

                    //=====================================================
                    //PC 생성.

                    for (int i = first; i <= last; i++)
                    {
                        string insert_Ip = first_Ip[0] + "." + first_Ip[1] + "." + first_Ip[2] + "." + i.ToString();
                        admin_PC_Add(insert_Ip, room_Num, room_Name, x * width, y * height, pc_type);

                        x++;
                        if (max_X == x)
                        {
                            y++;
                            x = 0;
                        }
                    }
                    //=====================================================
                    db.CloseDB();
                    admin_Upadate();
                    insert_Log(room_Name, "PC셋팅", "ADMIN_ROOM_ADD", room_Name + " 강의실 추가 성공");
                    Console.WriteLine("ADMIN_ROOM_ADD 성공");
                }
                catch (Exception e)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                    Console.WriteLine("ADMIN_ROOM_ADD 실패 : " + e.Message);
                }
            }
            #endregion

            #region 강의실 삭제
            if (packetType[0] == "ADMIN_ROOM_MIN")
            {
                string floor_Num = packetData[0];
                string room_Num = packetData[1];
                string room_Name = packetData[2];
                try
                {
                    admin_Room_Min(floor_Num, room_Name, room_Num);

                    admin_Upadate();
                    insert_Log(room_Name, "PC셋팅", "ADMIN_ROOM_MIN", room_Name + " 강의실 삭제 성공");
                    Console.WriteLine("ADMIN_ROOM_MIN 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_ROOM_MIN 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            //시스템 지점
            #region PC 초기 설정
            // 연결 시 
            if (packetType[0] == "PC_SET_START")
            {
                string floor_ip = packetData[0];
                pc_set_ip = packetData[0];
                string floor_Num = packetData[1];
                string room_Num = packetData[2];
                string room_Name = packetData[3];

                try
                {
                   
                    #region DB에서 등록된 PC 및 지점 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CComputer> computer = new List<CComputer>();
                    computer = db.selectPC_For_RoomNum(int.Parse(room_Num));

                    db.CloseDB();
                    #endregion

                    string sendData = string.Empty;
                    sendData += "PC_POINT_GET+";    //패킷 타입.

                    for (int i = 0; i < computer.Count; i++)
                    {
						if (computer[i].pointer_name == "설정X" || computer[i].pointer_name == "") 
                            computer[i].pointer_name = "미등록";
						
                        sendData += computer[i].pc_ip + "#";
                        sendData += computer[i].pointer_name + "#";
                        sendData += computer[i].power + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    Thread.Sleep(500);
                    pc_server.SendData(floor_ip, packet);
                    
                    //insert_Log(room_Name, "PC셋팅", "ADMIN_ROOM_MIN", room_Name + " 강의실 삭제 성공");
                    //Console.WriteLine("ADMIN_ROOM_MIN 성공");
                }

                catch (Exception e)
                {
                    //Console.WriteLine("ADMIN_ROOM_MIN 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
         

            if (packetType[0] == "PROGRAM_LIST")
            {
                try
                {
                    #region DB에서 등록된 PC 및 지점 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CComputer> pls = new List<CComputer>();
                    pls = db.ClientList(packetData[0]);

                    db.CloseDB();
                    #endregion
                }
                catch (Exception ex) { }
            }

            #region 리스트 띄우기

            if (packetType[0] == "POINTER_SHOW")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 복구지점 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFloor> fls = new List<CFloor>();
                    List<CPoint> rls = new List<CPoint>();

                    fls = db.Get_Floor_Info(packetData[0]);
                    rls = db.PointList(fls[0].Floor_NUM.ToString());

                    db.CloseDB();
                    #endregion

                    sendData += "POINTER_SHOW+";

                    for (int i = 0; i < rls.Count; i++)
                    {
                        sendData += rls[i].Pointer_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }

            if (packetType[0] == "PROGRAM_SHOW_SET")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 복구 지점의 프로그램 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CProgram> pls = new List<CProgram>();
                    pls = db.Get_Program_Info(packetData[1]);

                    db.CloseDB();
                    #endregion

                    sendData += "PROGRAM_SHOW_SET+";

                    for (int i = 0; i < pls.Count; i++)
                    {
                        sendData += pls[i].Program_Name + "#";
                        sendData += pls[i].Program_Path + "#";
                        sendData += pls[i].Program_Length + "#";
                        sendData += pls[i].Key_State + "#";
                        sendData += pls[i].Key_File_Name + "#";
                        sendData += pls[i].Key_File_Path + "#";
                        sendData += pls[i].Pointer_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }

			if (packetType[0] == "CLIENT_SET_POINTER")
			{
				int length = 2;
				string sendData = string.Empty;

				try
				{
					#region DB에서 PC의 복구 지점 수정하기
					CMyDB db = new CMyDB(ConString);
						
				
					db.ConnectDB();

					for (int i = 1; i < packetData.Length - 1; i += length)
					{
						List<CComputer> pc = new List<CComputer>();
						pc= db.SelectPC_For_IP(packetData[i+1]);
						if (pc[0].pointer_name == "초기값")
						{
							sendData += "TOADMIN+";
							sendData += "CLIENT_SET_POINTER+";    //패킷 타입.
							sendData += "이미 초기 지점이 등록되어 있습니다." + "#";

							sendData += "&";    //마지막 .

							client.Send(sendData);
							return;
						}
						
						db.Pc_Pointer_Update(packetData[i+1], "초기값");
						db.Pc_STATE_Update("1", packetData[i+1]);
					}

					db.CloseDB();
					#endregion

					sendData += "TOADMIN+";
					sendData += "CLIENT_SET_POINTER+";    //패킷 타입.
					sendData += "초기 지점으로 등록했습니다." + "#";

					sendData += "&";    //마지막 .

					client.Send(sendData);
				}
				catch (Exception ex)
				{
					string sendData1 = string.Empty;

					sendData1 += "CLIENT_SET_POINTER+";
					sendData1 += "[PC 초기 지점 수정 실패]";
					sendData1 += ex.Message + "#";

					client.Send(sendData);
				}
			}

			if (packetType[0] == "PC_POINTER_SET")
			{
				int length = 1;
				string sendData = string.Empty;

				try
				{
					#region DB에서 PC의 복구 지점 수정하기
					CMyDB db = new CMyDB(ConString);
					db.ConnectDB();

					for (int i = 1; i < packetData.Length - 1; i += length)
					{
						db.Pc_Pointer_Update(packetData[i], packetData[0]);
						db.Pc_STATE_Update("2",packetData[i]);
					}

					db.CloseDB();
					#endregion

					sendData += "PC_POINTER_SET_RESULT+";
					sendData += "PC 복구 지점 수정 성공#";

					byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

					pc_server.SendData(packetData[0], packet);
				}
				catch (Exception ex)
				{
					string sendData1 = string.Empty;

					sendData1 += "PC_POINTER_SET_RESULT+";
					sendData1 += "[PC 복구 지점 수정 실패]";
					sendData1 += ex.Message + "#";

					byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

					pc_server.SendData(packetData[0], packet);
				}
			}

            if (packetType[0] == "PROGRAM_SHOW")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 복구 지점의 프로그램 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CProgram> pls = new List<CProgram>();
                    pls = db.Get_Program_Info(packetData[1]);

                    db.CloseDB();
                    #endregion

                    sendData += "PROGRAM_SHOW+";

                    for (int i = 0; i < pls.Count; i++)
                    {
                        sendData += pls[i].Program_Name + "#";
                        sendData += pls[i].Program_Path + "#";
                        sendData += pls[i].Program_Length + "#";
                        sendData += pls[i].Key_State + "#";
                        sendData += pls[i].Key_File_Name + "#";
                        sendData += pls[i].Key_File_Path + "#";
                        sendData += pls[i].Pointer_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }

            if (packetType[0] == "POINTER_SHOW2")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 복구 지점얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFloor> fls = new List<CFloor>();
                    List<CPoint> pls = new List<CPoint>();

                    fls = db.Get_Floor_Info(packetData[0]);
                    pls = db.Get_Pointer_Info(fls[0].Floor_NUM.ToString());

                    db.CloseDB();
                    #endregion

                    sendData += "POINTER_SHOW2+";

                    for (int i = 0; i < pls.Count; i++)
                    {
                        sendData += pls[i].Pointer_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }
            // 복구 지점 등록하기
            if (packetType[0] == "PROGRAM_ADD")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 PC 및 지점 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFloor> fls = new List<CFloor>();
                    List<CRoom> rls = new List<CRoom>();
                    List<CProgram> pls = new List<CProgram>();

                    fls = db.Get_Floor_Info(packetData[0]);
                    rls = db.Get_Room_Info(fls[0].Floor_NUM.ToString());
                    pls = db.ProgramList(rls[0].Room_num.ToString());

                    db.CloseDB();
                    #endregion

                    sendData += "POINT_ADD+";

                    for (int i = 0; i < pls.Count; i++)
                    {
                        sendData += pls[i].Program_Name + "#";
                        sendData += pls[i].Key_State + "#";
                        sendData += pls[i].Key_File_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }

            if (packetType[0] == "ADD_KEYFILE")
            {
                string sendData = string.Empty;
                string program_name = string.Empty;

                int length = 4;
                try
                {
                    #region 프로그램 키 등록
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();


                    for (int i = 1; i < packetData.Length - 1; i += length)
                    {
                        CProgram cp = new CProgram();
                        cp.Key_State = "True";
                        cp.Key_File_Name = packetData[i];
                        cp.Key_File_Path = packetData[i + 1];
                        cp.Program_Name = packetData[i + 2];
                        cp.Pointer_Name = packetData[i + 3];

                        db.Key_State_Update(cp);
                        db.Key_File_Name_Update(cp);
                        db.Key_File_Path_Update(cp);

                        program_name = packetData[i + 2];
                    }

                    db.CloseDB();
                    #endregion

                    sendData += "ADD_KEYFILE_RESULT+";
                    sendData += "프로그램 키 등록 성공#";
                    sendData += program_name + "#";

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex)
                {
                    string sendData1 = string.Empty;

                    sendData1 += "ADD_KEYFILE_RESULT+";
                    sendData1 += "[프로그램 키 등록 실패] -";
                    sendData1 += ex.Message + "#";

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
            }

            // 복구 지점 등록하기
            if (packetType[0] == "ADD_PROGRAM")
            {
                string sendData = string.Empty;
                int length = 6;
                try
                {
                    #region 지점 등록 및 프로그램 등록
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFloor> fls = new List<CFloor>();
                    fls = db.Get_Floor_Info(packetData[0]);

                    for (int i = 1; i < packetData.Length - 1; i += length)
                    {
                        CProgram cp = new CProgram();
                        cp.Program_Name = packetData[i];
                        cp.Program_Path = packetData[i + 1];
                        cp.Program_Length = packetData[i + 2];
                        cp.Key_State = packetData[i + 3];
                        cp.Key_File_Name = packetData[i + 4];
                        cp.Pointer_Name = packetData[i + 5];

                        CPoint p = new CPoint();
                        p.Pointer_Name = packetData[i + 5];
                        p.Floor_Num = fls[0].Floor_NUM.ToString();

                        db.Point_Insert(p);
                        db.Program_Insert(cp);
                    }

                    db.CloseDB();
                    #endregion

                    sendData += "ADD_PROGRAM_RESULT+";
                    sendData += "지점 및 프로그램 등록 성공#";

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex)
                {
                    string sendData1 = string.Empty;

                    sendData1 += "ADD_PROGRAM_RESULT+";
                    sendData1 += "[지점 및 프로그램 등록 실패] -";
                    sendData1 += ex.Message + "#";

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
            }

            // 선택한 지점으로 설정하기
            if (packetType[0] == "PROGRAM_SET")
            {
                string sendData = string.Empty;

                try
                {
                    #region DB에서 등록된 PC 및 지점 얻기
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFloor> fls = new List<CFloor>();
                    List<CPoint> rls = new List<CPoint>();

                    fls = db.Get_Floor_Info(packetData[0]);
                    rls = db.PointList(fls[0].Floor_NUM.ToString());

                    db.CloseDB();
                    #endregion

                    sendData += "POINT_SET+";

                    for (int i = 0; i < rls.Count; i++)
                    {
                        sendData += rls[i].Pointer_Name + "#";
                    }

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.Message); }
            }
            #endregion

            #region 리스트 삭제하기
            if (packetType[0] == "DELETE_PROGRAM")
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                string floor_ip = packetData[0];
                string pointer_name = packetData[1];
                string program_name = packetData[2];

                List<CFloor> fls = new List<CFloor>();
                List<CPoint> pls = new List<CPoint>();

                fls = db.Get_Floor_Info(floor_ip);
                string floor_num = fls[0].Floor_NUM.ToString();
                pls = db.PointList(floor_num);

                for (int i = 0; i < pls.Count; i++)
                {
                    if (pls[i].Pointer_Name == pointer_name)
                    {
                        db.delete_program(program_name);

                        string sendData = string.Empty;
                        sendData += "DELETE_PROGRAM_COMPLETE" + "+";
                        sendData += "프로그램 삭제 성공" + "#";
                        sendData += pointer_name + "#";
                        sendData += program_name;

                        byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                        pc_server.SendData(packetData[0], packet);

                    }
                }
            }

            if (packetType[0] == "DELETE_POINTER")
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                string floor_ip = packetData[0];
                string pointer_name = packetData[1];

                List<CFloor> fls = new List<CFloor>();
                List<CPoint> pls = new List<CPoint>();

                fls = db.Get_Floor_Info(floor_ip);
                string floor_num = fls[0].Floor_NUM.ToString();
                pls = db.PointList(floor_num);

                for (int i = 0; i < pls.Count; i++)
                {
                    if (pls[i].Pointer_Name == pointer_name)
                    {
                        db.delete_program2(pointer_name);
                        db.delete_pointer(pointer_name);

                        string sendData = string.Empty;
                        sendData += "DELETE_POINTER_COMPLETE" + "+";
                        sendData += "삭제되었습니다." + "#";
                        sendData += pointer_name + "#";


                        byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                        pc_server.SendData(packetData[0], packet);
                    }

                }
            }
            #endregion

            #region 추가하기
            if (packetType[0] == "INSERT_PROGRAM")
            {

                try
                {
                    #region 프로그램 등록
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    CProgram cp = new CProgram();
                    cp.Program_Name = packetData[1];
                    cp.Program_Path = packetData[2];
                    cp.Program_Length = packetData[3];
                    cp.Key_State = packetData[4];
                    cp.Key_File_Name = packetData[5];
                    cp.Pointer_Name = packetData[6];

                    db.Program_Insert(cp);

                    db.CloseDB();
                    #endregion

                    string sendData = string.Empty;
                    sendData += "INSERT_PROGRAM_COMPLETE+";
                    sendData += "프로그램 등록 성공#";

                    byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                    pc_server.SendData(packetData[0], packet);
                }
                catch (Exception ex)
                {
                    string sendData1 = string.Empty;
                    sendData1 += "INSERT_PROGRAM_COMPLETE+";
                    sendData1 += "[프로그램 등록 실패] -";
                    sendData1 += ex.Message + "#";

                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);   //전송

                    pc_server.SendData(packetData[0], packet1);
                }
            }
            #endregion
            #endregion

            #region PC 정보
            if (packetType[0] == "ADMIN_PCS_GET")
            {
                string room_Num = packetData[0];
                string room_Name = packetData[1];

                try
                {
                    admin_PCs_Get(s, room_Num, room_Name);

                    admin_Upadate();

                    insert_Log(room_Name, "PC모니터링", "ADMIN_PCS_GET", room_Name + " 강의실 PC 정보 얻음 성공");
                    Console.WriteLine("ADMIN_PCS_GET 성공");

                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PCS_GET 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC 추가
            if (packetType[0] == "ADMIN_PC_ADD")
            {
                string room_Num = packetData[0];
                string room_Name = packetData[1];
                string pc_IP = packetData[2];
                string pc_type = "0";    //pc 타입

                //TODO : 다른 방 또는 층 서버에 PC정보가 있는지 체크 후 생성한다.
                try
                {
                    //생성 및 전송,
                    admin_PC_Add(pc_IP, room_Num, room_Name, pc_type);

                    //TODO : PC 접속 체크 패킷 보내야된다.
                    client_ReLogin(pc_IP);

                    admin_Upadate();
                    insert_Log(pc_IP, "PC셋팅", "ADMIN_PC_ADD", pc_IP + " PC 추가 성공");
                    Console.WriteLine("ADMIN_PC_ADD 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PC_ADD 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC 삭제
            if (packetType[0] == "ADMIN_PC_MIN")
            {
                string room_Num = packetData[0];
                int maxCount = int.Parse(packetData[1]);

                try
                {
                    List<string> IPs = new List<string>();
                    for (int i = 2; i < maxCount + 2; i++)
                        IPs.Add(packetData[i]);

                    admin_PC_Min(room_Num, maxCount, IPs);

                    admin_Upadate();
                    for (int i = 0; i < IPs.Count; i++)
                        insert_Log(IPs[i], "PC셋팅", "ADMIN_PC_MIN", IPs[i] + " PC 삭제 성공");
                    Console.WriteLine("ADMIN_PC_ADD 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PC_ADD 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC켜기
            if (packetType[0] == "ADMIN_PC_ON")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    for (int i = 0; i < packetData.Length; i++)
                    {
						
                        string ip = packetData[i];
                        string mac = db.GetMac_For_IP(ip);
						Console.WriteLine(ip);
						Console.WriteLine(mac);
                        //===================================================================
                        //PC 오류 찾기.
                        if (mac == "0")
                        {
                            insert_Log(ip, "PC제어", "PCADMIN_PC_ON", "MAC주소 없음. Client PC 재접속 요망");
                            continue;
                        }

                        List<CComputer> com = db.SelectPC_For_IP(ip);
                        if (com.Count == 0)
                        {
                            insert_Log(ip, "PC제어", "ADMIN_PC_ON", "DB에 존재하지 않음");
                            continue;
                        }
                        if (com[0].power == 1)
                        {
                            insert_Log(ip, "PC제어", "ADMIN_PC_ON", "PC가 이미 켜져 있음");
                            continue;
                        }
                        //===================================================================

                        //===================================================================
                        //PC켜기
                        int count = 0;

                        for (int k = 0; k < pc_On_Mac.Count; k++)
                        {
                            if (pc_On_Mac[k].ip == ip)
                                count++;
                        }

                        if (count == 0)
                        {
                            CPcOn pcOn = new CPcOn();
                            pcOn.ip = ip;
                            pcOn.mac = mac;

                            pc_On_Mac.Add(pcOn);
                            pc_On_Event.Set();
                            Console.WriteLine("켜져라");
                            insert_Log(ip, "PC제어", "ADMIN_PC_ON", ip + " PC 켜기 성공");
                            Console.WriteLine("ADMIN_PC_ON 성공");
                        }
                        //===================================================================

                    }
                    db.CloseDB();

                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PC_ON 실패 : " + e.Message);
                    // insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC좌표 수정.
            if (packetType[0] == "ADMIN_PC_POSITION_SET")
            {
                int typeLen = int.Parse(packetData[0]);
                try
                {

                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    for (int i = 1; i < (typeLen * 2) + 1; i += 2)
                    {
                        if (packetData[i] == string.Empty)
                            break;

                        string ip = packetData[i];
                        string coordinate = packetData[i + 1];
                        string[] data = coordinate.Split(':');

                        double swap_X = double.Parse(data[0]);
                        double swap_Y = double.Parse(data[1]);
                        int swapInt_X = (int)swap_X;
                        int swapInt_Y = (int)swap_Y;
                        db.update_Coordinate_Pc(ip, swapInt_X.ToString() + ":" + swapInt_Y.ToString());
                        insert_Log(ip, "PC셋팅", "ADMIN_PC_POSITION_SET", ip + " PC 좌표 수정 성공");
                    }
                    db.CloseDB();
                    Console.WriteLine("ADMIN_PC_POSITION_SET 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PC_POSITION_SET 실패 : " + e.Message);
                    //  insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC 좌표 수정2
            if (packetType[0] == "ADMIN_COM_COORDINATE_UPDATE")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    string ip = packetData[0];
                    string coordinate = packetData[1];
                    string after_coordinate = packetData[2];

                    db.update_Coordinate_Pc(ip, after_coordinate);
                    insert_Log(ip, "PC셋팅", "ADMIN_COM_COORDINATE_UPDATE", ip + " PC 좌표 수정 성공");

                    db.CloseDB();
                }
                catch (Exception)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region PC끄기
            if (packetType[0] == "ADMIN_PC_OFF")
            {

                try
                {
                    for (int i = 0; i < packetData.Length - 1; i++)
                    {
                        string sendData = string.Empty;
                        sendData += "SHUTDOWN+";    //패킷 타입.
                        byte[] packet = Encoding.Default.GetBytes(sendData);   //전송
                        server.SendData(packetData[i+1], packet);
                        insert_Log(packetData[i + 1], "PC제어", "ADMIN_PC_OFF", packetData[i + 1] + " PC 좌표 수정 성공");
                    }
                    Console.WriteLine("ADMIN_PC_OFF 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_PC_OFF 실패 : " + e.Message);
                    // insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }

            }
            #endregion

            #region 클라이언트 파일 보내기
            if (packetType[0] == "PROGRAM_SEND")
            {
                try
                {
                    int length = 1;
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    for (int i = 1; i < packetData.Length - 1; i += length)
                    {
                        file_pc pc = new file_pc();

                        pc.file_path = packetData[0];
                        pc_filepath = packetData[0];
                        pcinfo st = new pcinfo();
                        st.ip = packetData[i];
                        db.Update_pc_image_check(packetData[i], 1);
                        List<CComputer> cp = db.SelectPC_For_IP(packetData[i]);
                        db.insert_nonefilepc(cp[0].room_num, packetData[0], packetData[i], "0", 0, 0, "0:0");

                        pc.pcs.Add(st);

                        fpc.Add(pc);
                    }
                    db.CloseDB();
                    dataSendEvent.Set();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("PROGRAM_SEND 실패 : " + ex.Message);
                }
            }
            #endregion            

			#region 모든 프로그램 설치 메시지
			if (packetType[0] == "FINISHFILESEND")
			{
				try
				{
					for (int i = 1; i < packetData.Length - 1; i++)
					{
						string sendData = string.Empty;

						sendData += "FINISHFILESEND+";
						sendData += packetData[0];

						byte[] packet = Encoding.Default.GetBytes(sendData);
						server.SendData(packetData[i], packet);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("모든 프로그램 설치 메시지 실패 : " + ex.Message);
				}
			}
			#endregion

            #region 클라이언트 KEY 성공 여부받기
            if (packetType[0] == "CLIENT_KEY_COMPLETE")
            {
                try
                {
                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_KEY_RESULT+";
                    sendData1 += packetData[0] + "#";

                    byte[] packet = Encoding.Default.GetBytes(sendData1);   //전송

                    pc_server.SendData(MyIp, packet);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("클라이언트 KEY 성공 여부받기 실패 : " + ex.Message);
                }
            }
            #endregion

			#region 클라이언트 복원 지점 설정 완료
			if (packetType[0] == "CLIENT_SETTING_FINISH")
			{
				try
				{
					string sendData1 = string.Empty;
					sendData1 += "CLIENT_SETTING_FINISH+";
					sendData1 += packetData[0] + "#";
					sendData1 += packetData[1] + "#";

					byte[] packet = Encoding.Default.GetBytes(sendData1);   //전송

					pc_server.SendData(MyIp, packet);
				}
				catch (Exception ex)
				{
					Console.WriteLine("클라이언트 복원 지점 설정 완료 실패 : " + ex.Message);
				}
			}
			#endregion
           
            #region 클라이언트 KEY 이벤트 보내기
            if (packetType[0] == "KEY_SEND")
            {
                try
                {
                    FileStream fs = new FileStream(packetData[0], FileMode.Open, FileAccess.Read);

                    StreamReader sr = new StreamReader(fs);
                    string line = string.Empty;

                    Program_Key pk = new Program_Key();
                    pk.Key_Path = packetData[0];

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] str = line.Split('/'); //str[0]은 키코드 [1]은 Thread.Sleep값

                        pk.Key_Value += str[0] + "/";
                        pk.Key_Sleep += str[1] + "/";
                        pk.Key_Name = Path.GetFileName(pk.Key_Path);
                    }

                    ks.Add(pk);
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    List<CProgram> pr = new List<CProgram>();
                    pr = db.Get_Program_Info2(pk.Key_Name);
                    db.CloseDB();

                    for (int i = 1; i < packetData.Length - 1; i++)
                    {
                        string none_Pc_IP = packetData[i];

                        IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
                        string myip = host.AddressList[0].ToString();

                        string sendData = string.Empty;
                        sendData += "KEY_SEND_TO_CLIENT+";    //패킷 타입.
                        sendData += myip + "#";
                        sendData += Path.GetFileName(pr[0].Program_Name) + "#";
                        sendData += pk.Key_Sleep + "#";
                        sendData += pk.Key_Value + "#";

                        byte[] packet = Encoding.Default.GetBytes(sendData);
                        server.SendData(none_Pc_IP, packet);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("클라이언트 KEY 이벤트 보내기 실패 : " + ex.Message);
                }
            }
            #endregion

            #region 층 이름 수정
            if (packetType[0] == "ADMIN_FLOOR_NAME_UPDATE")
            {
                string floor_Num = packetData[0];
                string floor_Name = packetData[1];
                string after_Name = packetData[2];

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    db.update_Floor_Name(floor_Num, after_Name);
                    db.CloseDB();

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "ADMIN_FLOOR_NAME_UPDATE" + "+";
                    sendData += 1 + "#";
                    sendData += floor_Num + "#";
                    sendData += floor_Name + "#";
                    sendData += after_Name;

                    sendData += "&";    //마지막 .

                    client.Send(sendData);

                    admin_Upadate();
                    insert_Log(floor_Name, "PC셋팅", "ADMIN_FLOOR_NAME_UPDATE", floor_Name + " 에서 " + after_Name + " 로 층 이름 변경 성공");
                    Console.WriteLine("ADMIN_FLOOR_NAME_UPDATE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_FLOOR_NAME_UPDATE 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 룸 번호 변경
            if (packetType[0] == "ADMIN_ROOM_NUM_UPDATE")
            {
                string before_Num = packetData[0];
                string after_Num = packetData[1];

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    db.update_Room_Num(before_Num, after_Num);
                    db.CloseDB();

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "ADMIN_ROOM_NUM_UPDATE" + "+";
                    sendData += 1 + "#";
                    sendData += before_Num + "#";
                    sendData += after_Num;

                    sendData += "&";    //마지막 .

                    client.Send(sendData);

                    admin_Upadate();
                    insert_Log(before_Num, "PC셋팅", "ADMIN_FLOOR_NAME_UPDATE", before_Num + " 에서 " + after_Num + " 로 강의실 번호 변경 성공");
                    Console.WriteLine("ADMIN_ROOM_NUM_UPDATE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_ROOM_NUM_UPDATE 실패 : " + e.Message);
                    //  insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 룸 이름 변경
            if (packetType[0] == "ADMIN_ROOM_NAME_UPDATE")
            {
                string room_Num = packetData[0];
                string room_Name = packetData[1];
                string after_Name = packetData[2];
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    db.update_Room_Name(room_Num, after_Name);
                    db.CloseDB();

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "ADMIN_ROOM_NAME_UPDATE" + "+";
                    sendData += 1 + "#";
                    sendData += room_Num + "#";
                    sendData += room_Name + "#";
                    sendData += after_Name;

                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(room_Name, "PC셋팅", "ADMIN_FLOOR_NAME_UPDATE", room_Name + " 에서 " + after_Name + " 로 강의실 이름 변경 성공");
                    admin_Upadate();
                    Console.WriteLine("ADMIN_ROOM_NAME_UPDATE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ADMIN_ROOM_NAME_UPDATE 실패 : " + e.Message);
                    // insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            #endregion

            //지원 0121
            #region floor 서버 파일 체크
            if (packetType[0] == "FLOOR_FILE_CHECK")
            {
                FileInfo fileInfo;
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CFormat_File_Exe_Check_S> Exe_Check_S = db.Select_Exe_FileName();
                    fileInfo = new FileInfo("C:\\" + Exe_Check_S[0].format_file_downloding_name_s);

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "FLOOR_EXE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";

                    int check = 0;

                    if (fileInfo.Exists == true)//파일이 존재할때.
                    {
                        insert_Log(packetData[0], "PC셋팅", "FLOOR_EXE_FILE_CHECK", "Floor 서버 파일 체크 성공");
                        check = 1;
                        sendData += check.ToString() + "#";     //정보
                        check = 0;
                    }
                    else
                    {
                        sendData += check.ToString() + "#";     //정보
                    }

                    sendData += "&";
                    db.CloseDB();
                    client.Send(sendData);
                }
                catch (Exception)
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "FLOOR_EXE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";
                    sendData += 0 + "#";     //정보
                    sendData += "&";
                    client.Send(sendData);
                    insert_Log(packetData[0], "PC셋팅", "FLOOR_IMAGE_FILE_CHECK", "Floor 서버 고스트 파일이 없습니다.");
                }

                Thread.Sleep(10);
                //============================================================================================
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    List<CFormat_File_Image_Check_S> Image_Check_S = db.Select_Image_FileName();
                    string sendData1 = string.Empty;
                    sendData1 += "TOADMIN+";
                    sendData1 += "FLOOR_IMAGE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData1 += packetData[0] + "#";
                    int check = 0;

                    string[] FileRoute = new string[5];
                    for (int i = 0; i < Image_Check_S.Count(); i++)
                    {
                        FileRoute[i] = Image_Check_S[i].format_file_none_name_s;
                        fileInfo = new FileInfo("C:\\" + FileRoute[i]);

                        if (fileInfo.Exists == true)//파일이 존재할때.
                        {
                            insert_Log(packetData[0], "PC셋팅", "FLOOR_IMAGE_FILE_CHECK", "Floor 서버 파일 체크 성공");

                            check = 1;
                            sendData1 += check.ToString() + "#";     //정보
                            check = 0;
                        }
                        else
                        {
                            sendData1 += check.ToString() + "#";     //정보
                        }
                    }
                    sendData1 += "&";
                    db.CloseDB();
                    client.Send(sendData1);
                }
                catch (Exception)
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "FLOOR_IMAGE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";
                    sendData += 0 + "#";     //정보
                    sendData += "&";
                    client.Send(sendData);
                    insert_Log(packetData[0], "PC셋팅", "FLOOR_IMAGE_FILE_CHECK", "Floor 서버 이미지 파일이 없습니다.");
                }
            }
            #endregion
            //지원 0121
            #region floor 서버 고스트 파일 체크
            if (packetType[0] == "FLOOR_EXE_FILE_CHECK")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "FLOOR_EXE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";
                    FileInfo fileInfo;
                    int check = 0;
                    string FileRoute = string.Empty;

                    FileRoute = packetData[1];
                    fileInfo = new FileInfo("C:\\" + FileRoute);

                    if (fileInfo.Exists == true)//파일이 존재할때.
                    {
                        db.insert_exe_file_server(packetData[1]);
                        insert_Log(packetData[0], "PC셋팅", "FLOOR_EXE_FILE_CHECK", "Floor 서버 고스트 파일 체크 성공");

                        check = 1;
                        sendData += check.ToString() + "#";     //정보
                        check = 0;
                    }
                    else
                    {
                        sendData += check.ToString() + "#";     //정보
                    }

                    sendData += "&";
                    db.CloseDB();
                    client.Send(sendData);

                    Console.WriteLine("FLOOR_EXE_FILE_CHECK 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("FLOOR_EXE_FILE_CHECK 실패 : " + e.Message);
                }
            }
            #endregion

            #region floor 서버 이미지 파일 체크
            if (packetType[0] == "FLOOR_IMAGE_FILE_CHECK")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "FLOOR_IMAGE_FILE_CHECK_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";
                    FileInfo fileInfo;
                    int check = 0;
                    string[] FileRoute = new string[packetData.Length - 1];

                    for (int i = 0; i < FileRoute.Length; i++)
                    {
                        FileRoute[i] = packetData[i + 1];
                        fileInfo = new FileInfo("C:\\" + FileRoute[i]);

                        if (fileInfo.Exists == true)//파일이 존재할때.
                        {
                            db.insert_image_file_server(packetData[i + 1], (i + 1).ToString());
                            insert_Log(packetData[0], "PC셋팅", "FLOOR_IMAGE_FILE_CHECK", "Floor 서버 파일 체크 성공");

                            check = 1;
                            sendData += check.ToString() + "#";     //정보
                            check = 0;
                        }
                        else
                        {
                            sendData += check.ToString() + "#";     //정보
                        }
                    }
                    sendData += "&";
                    db.CloseDB();
                    client.Send(sendData);

                    Console.WriteLine("FLOOR_IMAGE_FILE_CHECK 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("FLOOR_IMAGE_FILE_CHECK 실패 : " + e.Message);
                }
            }
            #endregion

            #region 소프트웨어 사용량 확인하기 위한 이름 입력(===============창근변경==================)
            if (packetType[0] == "ADMIN_SOFTWARE_USE_NAME_INSERT")
            {
                string software_name = packetData[0];
                int room_num = int.Parse(packetData[1]);

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CComputer> pc = db.SelectCPc(room_num);

                    for (int i = 0; i < pc.Count(); i++)
                    {
                        software_usename_insert(software_name, pc[i].pc_ip);
                    }
                    string msg = room_num.ToString() + "층";

                    insert_Log(msg, "PC모니터링", "ADMIN_SOFTWARE_USE_NAME_INSERT", "소프트웨어 사용량 List 추가 성공");

                    db.CloseDB();
                }
                catch (Exception)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            #endregion

            #region 소프트웨어 사용량 확인하기 위한 이름 삭제(===============창근변경==================)
            if (packetType[0] == "ADMIN_SOFTWARE_USE_NAME_DELETE")
            {
                string software_name = packetData[0];
                int room_num = int.Parse(packetData[1]);
                string sendData = string.Empty;

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    List<CComputer> pc = db.SelectCPc(room_num);

                    for (int i = 0; i < pc.Count(); i++)
                    {
                        db.delete_software_use_name(software_name, pc[i].pc_ip);

                        software_use_send(pc[i].pc_ip);
                    }

                    string msg = room_num.ToString() + "층";

                    insert_Log(msg, "PC모니터링", "ADMIN_SOFTWARE_USE_NAME_DELETE", "소프트웨어 사용량 List 삭제 성공");
                    db.CloseDB();
                }
                catch (Exception)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            #endregion

            #region 선미 타이머
            #region 전체 컴퓨터 켜기 타이머(알람) 설정
            if (packetType[0] == "ALL_ON_PC_TIMER")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    // List<CArlarm_on> Arlarms_client = null;
                    string ip = string.Empty;
                    int on_time_count = 1;

                    string date = string.Empty;
                    int am_pm = 0;
                    string year = string.Empty;
                    string month = string.Empty;
                    string day = string.Empty;
                    int days = 0;
                    string time = string.Empty;

                    string[] arlarms = packetData[packetData.Length - 1].Split('@');


                    if (arlarms[0] != "Days")       //날짜로 알람
                    {
                        date = arlarms[0];
                        am_pm = int.Parse(arlarms[1]);
                        DateTime date1 = DateTime.Parse(date);
                        year = date1.Year.ToString();
                        month = date1.Month.ToString();
                        day = date1.Day.ToString();


                        time = arlarms[2];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());


                        for (int i = 0; i < packetData.Length - 1; i++) //pc_ip 들만 for문 돌게함
                        {
                            ip = packetData[i];
                            db.Insert_pc_ON_TIME(ip, year, month, day, days, am_pm, time, test_time);
                            db.Update_pc_ON_TIME_check(ip, on_time_count);
                            Information_Alram();
                        }
                    }
                    else                         //요일로 알람
                    {
                        time = arlarms[9];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        am_pm = int.Parse(arlarms[1]);


                        for (int a = 2; a < arlarms.Length - 1; a++)  //[2]~[8]까지 요일(true,false)
                        {
                            if (arlarms[a] == "True")
                            {
                                for (int i = 0; i < packetData.Length - 1; i++) //pc_ip 들만 for문 돌게함
                                {

                                    if (a == 2)
                                    {
                                        arlarms[2] = "1";
                                    }
                                    else if (a == 3)
                                    {
                                        arlarms[3] = "2";
                                    }
                                    else if (a == 4)
                                    {
                                        arlarms[4] = "3";
                                    }
                                    else if (a == 5)
                                    {
                                        arlarms[5] = "4";
                                    }
                                    else if (a == 6)
                                    {
                                        arlarms[6] = "5";
                                    }
                                    else if (a == 7)
                                    {
                                        arlarms[7] = "6";
                                    }
                                    else if (a == 8)
                                    {
                                        arlarms[8] = "7";
                                    }
                                    days = int.Parse(arlarms[a]);
                                    ip = packetData[i];

                                    db.Insert_pc_ON_TIME(ip, year, month, day, days, am_pm, time, test_time);
                                    db.Update_pc_ON_TIME_check(ip, on_time_count);

                                    Information_Alram();
                                }

                            }
                        }
                    }
                    db.CloseDB();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("전체 컴퓨터 켜기 타이머(알람) 설정 실패 : " + ex.Message);
                }
            }
            #endregion

            #region 전체 컴퓨터 끄기 타이머

            if (packetType[0] == "전체_컴퓨터_끄기_타이머_설정_패킷")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    string ip = string.Empty;
                    int off_time_count = 1;

                    string date = string.Empty;
                    int am_pm = 0;
                    string year = string.Empty;
                    string month = string.Empty;
                    string day = string.Empty;
                    int days = 0;
                    string time = string.Empty;
                    string[] arlarms = packetData[packetData.Length - 1].Split('@');


                    if (arlarms[0] != "Days")       //날짜로 알람
                    {
                        date = arlarms[0];
                        am_pm = int.Parse(arlarms[1]);
                        DateTime date1 = DateTime.Parse(date);
                        year = date1.Year.ToString();
                        month = date1.Month.ToString();
                        day = date1.Day.ToString();

                        time = arlarms[2];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        for (int i = 0; i < packetData.Length - 1; i++) //pc_ip 들만 for문 돌게함
                        {

                            ip = packetData[i];

                            db.Insert_pc_OFF_TIME(ip, year, month, day, days, am_pm, time, test_time);
                            db.Update_pc_OFF_TIME_check(ip, off_time_count);

                            Information_Alram();
                        }
                    }
                    else                         //요일로 알람
                    {
                        time = arlarms[9];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        am_pm = int.Parse(arlarms[1]);

                        for (int a = 2; a < arlarms.Length - 1; a++)  //[2]~[8]까지 요일(true,false)
                        {
                            if (arlarms[a] == "True")
                            {
                                for (int i = 0; i < packetData.Length - 1; i++) //pc_ip 들만 for문 돌게함
                                {
                                    if (a == 2)
                                    {
                                        arlarms[2] = "1";
                                    }
                                    else if (a == 3)
                                    {
                                        arlarms[3] = "2";
                                    }
                                    else if (a == 4)
                                    {
                                        arlarms[4] = "3";
                                    }
                                    else if (a == 5)
                                    {
                                        arlarms[5] = "4";
                                    }
                                    else if (a == 6)
                                    {
                                        arlarms[6] = "5";
                                    }
                                    else if (a == 7)
                                    {
                                        arlarms[7] = "6";
                                    }
                                    else if (a == 8)
                                    {
                                        arlarms[8] = "7";
                                    }
                                    days = int.Parse(arlarms[a]);
                                    ip = packetData[i];

                                    db.Insert_pc_OFF_TIME(ip, year, month, day, days, am_pm, time, test_time);
                                    db.Update_pc_OFF_TIME_check(ip, off_time_count);

                                    Information_Alram();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("전체 컴퓨터 끄기 타이머 실패 : " + ex.Message);
                }
            }
            #endregion

            #region 전체 컴퓨터 타이머 삭제 ㅅㅁ
            if (packetType[0] == "전체_컴퓨터_타이머_삭제_패킷")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    //--------------------------------------------------
                    List<On_Date> on_Date = new List<On_Date>();
                    List<Off_Date> off_Date = new List<Off_Date>();
                    List<On_Days> on_Days = new List<On_Days>();
                    List<Off_Days> off_Days = new List<Off_Days>();
                    on_Date = db.Select_Ondate();
                    off_Date = db.Select_Offdate();
                    on_Days = db.Select_All_Ondays();
                    off_Days = db.Select_All_Offdays();
                    //--------------------------------------------------

                    string ip = string.Empty;
                    int off_time_count = 0;
                    string[] check = packetData[packetData.Length - 1].Split('@');    //on,off 체크 여부를 가져옴.
                    string on_check = check[0];
                    string off_check = check[1];

                    for (int i = 0; i < packetData.Length - 1; i++) //pc_ip 들만 for문 돌게함
                    {
                        ip = packetData[i];
                        if (on_Date.Count != 0 || on_Days.Count != 0)           //켜기알람삭제
                        {
                            if (on_check == "True")
                            {

                                db.delete_Pc_On();
                                db.Update_pc_ON_TIME_check(ip, off_time_count);
                            }
                        }

                        if (on_Days.Count != 0 || off_Days.Count != 0)      //끄기알람 삭제
                        {
                            if (off_check == "True")
                            {

                                db.delete_Pc_Off();
                                db.Update_pc_OFF_TIME_check(ip, off_time_count);
                            }
                        }
                    }
                    db.CloseDB();
                }
                catch (Exception e)
                {
                    Console.WriteLine("전체컴퓨터 타이머삭제오류:" + e.Message);
                }
            }
            #endregion

            #region 해당 컴퓨터 켜기 타이머 설정
            if (packetType[0] == "해당_컴퓨터 켜기_타이머_설정_패킷")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    // List<CArlarm_on> Arlarms_client = null;

                    int on_time_count = 1;
                    string date = string.Empty;
                    string year = string.Empty;
                    string month = string.Empty;
                    string day = string.Empty;
                    int days = 0;
                    string time = string.Empty;
                    string pc_Ip = packetData[1];
                    int am_pm = int.Parse(packetData[3]);

                    if (packetData[2] != "Days")   //날짜로 알람([4]은 날짜 시간, [12]은 요일 시간)
                    {
                        date = packetData[2];

                        time = packetData[4];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        DateTime date1 = DateTime.Parse(date);
                        year = date1.Year.ToString();
                        month = date1.Month.ToString();
                        day = date1.Day.ToString();


                        db.Insert_pc_ON_TIME(pc_Ip, year, month, day, days, am_pm, time, test_time);
                        db.Update_pc_ON_TIME_check(pc_Ip, on_time_count);

                        Information_Alram();

                    }
                    else                        //요일로 알람
                    {
                        time = packetData[12];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        for (int i = 5; i < packetData.Length - 1; i++)  //[5]~[11]까지 요일(true,false)
                        {
                            if (packetData[i] == "True")
                            {
                                if (i == 5)
                                {
                                    packetData[5] = "1";
                                }
                                else if (i == 6)
                                {
                                    packetData[6] = "2";
                                }
                                else if (i == 7)
                                {
                                    packetData[7] = "3";
                                }
                                else if (i == 8)
                                {
                                    packetData[8] = "4";
                                }
                                else if (i == 9)
                                {
                                    packetData[9] = "5";
                                }
                                else if (i == 10)
                                {
                                    packetData[10] = "6";
                                }
                                else if (i == 11)
                                {
                                    packetData[11] = "7";
                                }
                                days = int.Parse(packetData[i]);

                                db.Insert_pc_ON_TIME(pc_Ip, year, month, day, days, am_pm, time, test_time);
                                db.Update_pc_ON_TIME_check(pc_Ip, on_time_count);

                                Information_Alram();
                            }
                        }
                    }
                    db.CloseDB();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("해당 컴퓨터 켜기 타이머 설정 실패 : " + ex.Message);
                }
            }
            #endregion

            #region 해당 컴터 끄기 알람설정
            if (packetType[0] == "해당_컴퓨터 끄기_타이머_설정_패킷")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();

                    int on_time_count = 1;
                    string date = string.Empty;
                    string year = string.Empty;
                    string month = string.Empty;
                    string day = string.Empty;
                    int days = 0;
                    string time = string.Empty;
                    string pc_Ip = packetData[1];
                    int am_pm = int.Parse(packetData[3]);

                    if (packetData[2] != "Days")   //날짜로 알람([4]은 날짜 시간, [12]은 요일 시간)
                    {
                        date = packetData[2];

                        time = packetData[4];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        DateTime date1 = DateTime.Parse(date);

                        year = date1.Year.ToString();
                        month = date1.Month.ToString();
                        day = date1.Day.ToString();

                        db.Insert_pc_OFF_TIME(pc_Ip, year, month, day, days, am_pm, time, test_time);
                        db.Update_pc_OFF_TIME_check(pc_Ip, on_time_count);
                        Information_Alram();

                    }
                    else                        //요일로 알람
                    {
                        time = packetData[12];
                        DateTime t = DateTime.Parse(time);
                        string test = t.ToLongTimeString();
                        string[] test2 = test.Split(' ');
                        string[] test3 = test2[1].Split(':');
                        string test4 = test3[0] + test3[1] + test3[2];
                        int test_time = int.Parse(test4.ToString());

                        for (int i = 5; i < packetData.Length - 1; i++)  //[5]~[11]까지 요일(true,false)
                        {

                            if (packetData[i] == "True")
                            {
                                if (i == 5)
                                {
                                    packetData[5] = "1";
                                }
                                else if (i == 6)
                                {
                                    packetData[6] = "2";
                                }
                                else if (i == 7)
                                {
                                    packetData[7] = "3";
                                }
                                else if (i == 8)
                                {
                                    packetData[8] = "4";
                                }
                                else if (i == 9)
                                {
                                    packetData[9] = "5";
                                }
                                else if (i == 10)
                                {
                                    packetData[10] = "6";
                                }
                                else if (i == 11)
                                {
                                    packetData[11] = "7";
                                }
                                days = int.Parse(packetData[i]);

                                db.Insert_pc_OFF_TIME(pc_Ip, year, month, day, days, am_pm, time, test_time);
                                db.Update_pc_OFF_TIME_check(pc_Ip, on_time_count);
                                Information_Alram();
                            }
                        }
                    }
                    db.CloseDB();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("해당 컴터 끄기 알람설정 실패 : " + ex.Message);
                }
            }
            #endregion

            #region 해당 컴퓨터 타이머 선택 삭제
            if (packetType[0] == "DB_Pctimer_select_Delete") //ㅅㅁ
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    //  db.Select_All_On();
                    string ip = packetData[2];
                    int off_time_count = 0;

                    #region 켜기알람삭제할때
                    if (packetData[0] == "On")
                    {
                        if (packetData[1] == "date")
                        {
                            string year = packetData[3];
                            string month = packetData[4];
                            string day = packetData[5];
                            int ampm = int.Parse(packetData[6]);
                            string time = packetData[7];
                            db.delete_Pc_On_date(ip, year, month, day, ampm, time);

                            //여기다 조거문 줘야함 알라몬을 셀렉트 해왔을때 아무것도 없으면 on_time 카운트를 1=>0 으로
                            if (db.Select_Ondate_delete(ip) == "" && db.Select_Ondays_delete(ip) == "")
                            {
                                db.Update_pc_ON_TIME_check(ip, off_time_count);

                                string sendData = string.Empty;
                                sendData += "TOADMIN+";
                                sendData += "DB_Pctimer_Delete_complete+";
                                sendData += "ON" + "#";
                                sendData += ip + "#";
                                sendData += "&";    //마지막 .
                                client.Send(sendData);
                            }


                        }
                        if (packetData[1] == "days")
                        {
                            int days = int.Parse(packetData[3]);
                            int ampm = int.Parse(packetData[4]);
                            string time = packetData[5];
                            db.delete_Pc_On_days(ip, days, ampm, time);

                            //여기다 조거문 줘야함 알라몬을 셀렉트 해왔을때 아무것도 없으면 on_time 카운트를 1=>0 으로
                            if (db.Select_Ondate_delete(ip) == "" && db.Select_Ondays_delete(ip) == "")
                            {
                                db.Update_pc_ON_TIME_check(ip, off_time_count);

                                string sendData = string.Empty;
                                sendData += "TOADMIN+";
                                sendData += "DB_Pctimer_Delete_complete+";
                                sendData += "ON" + "#";
                                sendData += ip + "#";
                                sendData += "&";    //마지막 .

                                client.Send(sendData);
                            }
                        }
                    }
                    #endregion

                    #region 끄기알람삭제할때
                    if (packetData[0] == "Off")
                    {
                        if (packetData[1] == "date")
                        {
                            string year = packetData[3];
                            string month = packetData[4];
                            string day = packetData[5];
                            int ampm = int.Parse(packetData[6]);
                            string time = packetData[7];
                            db.delete_Pc_Off_date(ip, year, month, day, ampm, time);

                            //여기다 조거문 줘야함 알라몬을 셀렉트 해왔을때 아무것도 없으면 on_time 카운트를 1=>0 으로
                            if (db.Select_Offdate_delete(ip) == "" && db.Select_Offdays_delete(ip) == "")
                            {
                                db.Update_pc_OFF_TIME_check(ip, off_time_count);
                                string sendData = string.Empty;
                                sendData += "TOADMIN+";
                                sendData += "DB_Pctimer_Delete_complete+";
                                sendData += "OFF" + "#";
                                sendData += ip + "#";
                                sendData += "&";    //마지막 .

                                client.Send(sendData);

                            }

                        }
                        if (packetData[1] == "days")
                        {

                            int days = int.Parse(packetData[3]);
                            int ampm = int.Parse(packetData[4]);
                            string time = packetData[5];
                            db.delete_Pc_Off_days(ip, days, ampm, time);

                            //여기다 조거문 줘야함 알라몬을 셀렉트 해왔을때 아무것도 없으면 on_time 카운트를 1=>0 으로
                            if (db.Select_Offdate_delete(ip) == "" && db.Select_Offdays_delete(ip) == "")
                            {
                                db.Update_pc_OFF_TIME_check(ip, off_time_count);
                                string sendData = string.Empty;
                                sendData += "TOADMIN+";
                                sendData += "DB_Pctimer_Delete_complete+";
                                sendData += "OFF" + "#";
                                sendData += ip + "#";
                                sendData += "&";    //마지막 .

                                client.Send(sendData);
                            }
                        }

                    }
                    #endregion

                }
                catch (Exception e)
                {
                    Console.WriteLine("해당컴퓨터 타이머삭제오류:" + e.Message);
                }
            }
            #endregion

            #region 해당 타이머(삭제하기위한) 셀렉트
            if (packetType[0] == "DB_Pctimer_select_on") //ㅅㅁ
            {
                try
                {
                    string ip = string.Empty;
                    string str = string.Empty;
                    string str1 = string.Empty;
                    CMyDB mydb = new CMyDB(ConString);
                    mydb.ConnectDB();
                    ip = packetData[0];
                    str = mydb.Select_Ondate_delete(ip);
                    str1 = mydb.Select_Ondays_delete(ip);
                    mydb.CloseDB();
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DB_Pctimer_select_on" + "+";
                    sendData += str;
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    //    insert_Log("", "PC모니터링", "DBSoftList", "소프트웨어 설치여부 List 검색 성공"); 나중에 넣기
                    Console.WriteLine("DB_Pctimer_select_on 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Pctimer_select_on 실패 : " + e.Message);
                }
            }

            if (packetType[0] == "DB_Pctimer_select_off") //ㅅㅁ
            {
                try
                {
                    string ip = string.Empty;
                    string str = string.Empty;
                    string str1 = string.Empty;
                    CMyDB mydb = new CMyDB(ConString);
                    mydb.ConnectDB();
                    ip = packetData[0];
                    str = mydb.Select_Offdate_delete(ip);
                    str1 = mydb.Select_Offdays_delete(ip);
                    mydb.CloseDB();
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DB_Pctimer_select_off" + "+";
                    sendData += str;
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    //    insert_Log("", "PC모니터링", "DBSoftList", "소프트웨어 설치여부 List 검색 성공"); 나중에 넣기
                    Console.WriteLine("DB_Pctimer_select_off 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Pctimer_select_off 실패 : " + e.Message);
                }
            }
            #endregion

            #endregion

            #endregion

            #region CLIENT

            #region 원격제어
            if (packetType[0] == "REMOTE_SEND")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += packetType[0] + "+";    // REMOTE_SEND
                    sendData += packetData[1] + "+";    // floor_ip
                    sendData += "&";

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[1], "PC모니터링", "REMOTE_SEND", "원격제어 성공");
                    //  db.CloseDB();
                    Console.WriteLine("REMOTE_SEND 성공");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("원격제어 실패 : " + ex.Message);
                }
            }
            #endregion

			#region 클라이언트 로그인
			if (packetType[0] == "CLIENT_LOGIN")
			{
				string ip = packetData[0];
				string mac = packetData[1];
				string pc_name = packetData[2];
				string type = packetData[3];
				string panel = packetData[4];
				string software = packetData[5];
				string format_state = string.Empty;

				try
				{
					CMyDB db = new CMyDB(ConString);
					db.ConnectDB();

					client_Login(s, ip, mac, pc_name, type, panel, software);
					List<CComputer> pc = new List<CComputer>();
					pc = db.Get_Format_State(ip);
					format_state = pc[0].format_state;

					admin_Upadate();
					//청원
					string time = DateTime.Now.ToString("HH:mm:ss");
					Stopwatch sw1 = new Stopwatch();
					ip += "!" + time;
					sw.Add(ip, sw1);
					sw1.Start();

					if (format_state == "1")
					{
						string sendData = string.Empty;
						sendData += "GETPROCESS" + "+"; //NEEDLESS_CLIENT_SOFTWARE
						sendData += packetData[0] + "#"; //ip

						byte[] packet = Encoding.Default.GetBytes(sendData);
						server.SendData(packetData[0], packet);
					}

					Console.WriteLine("CLIENT_LOGIN 성공");
					db.CloseDB();
				}
				catch (Exception e)
				{
					Console.WriteLine("CLIENT_LOGIN 실패 : " + e.Message);
					// insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
				}

				#region 청원
				string sendData1 = string.Empty;
				sendData1 += "EVENTCHECK+";    //패킷 타입.
				byte[] packet1 = Encoding.Default.GetBytes(sendData1);   //전송
				server.SendData(packetData[0], packet1);

				#endregion
			}
			#endregion

            #region 청원
            if (packetType[0] == "EVENT_CHECK")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += packetType[0] + "+";
                    sendData += packetData[0] + "#";
                    sendData += "&";

                    client.Send(sendData);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("EVENT_CHECK" + ex.Message);
                }
            }

            if (packetType[0] == "EVENT_CHECK_RESET")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += packetType[0] + "+";
                    sendData += packetData[0] + "#";
                    sendData += "&";

                    client.Send(sendData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EVENT_CHECK_RESET" + ex.Message);
                }
            }
            #endregion

            #region 클라이언트 로그아웃
            if (packetType[0] == "CLIENT_LOGOUT")
            {
                string ip = packetData[0];
                try
                {
                    admin_Client_Logout(ip);
                    int f = 0;
                    foreach (KeyValuePair<string, Stopwatch> kv in sw)
                    {
                        if (kv.Key.Split('!')[0] == ip)
                        {
                            kv.Value.Stop();
                            string time = kv.Value.Elapsed.ToString().Split('.')[0];

                            CMyDB db = new CMyDB(ConString);
                            db.ConnectDB();
                            //SelectRoom_For_RoomNum  //룸 정보 얻기 룸 넘버로부터
                            List<CComputer> com = new List<CComputer>();
                            com = db.SelectPC_For_IP(ip);
                            List<CRoom> room = new List<CRoom>();
                            for (int i = 0; i < com.Count; i++)
                            {
                                if (com[i].pc_ip == ip) //ip확인
                                {
                                    room = db.SelectRoom_For_RoomNum(com[i].room_num);
                                }
                            }

                            db.CloseDB();
                            insert_Log3(ip, "PC 사용시간", room[0].Room_name.ToString(), kv.Key.Split('!')[1], time);

                            sw.RemoveAt(f);
                        }
                        f++;
                    }
                    admin_Upadate();
                }
                catch (Exception)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

			#region 클라이언트 프로세스 목록 추가
			if (packetType[0] == "GETPROCESS_RESULT")
			{
				string ip = packetData[0];

				CMyDB db = new CMyDB(ConString);
				db.ConnectDB();

				for (int i = 1; i < packetData.Length - 1; i++)
				{
					db.Add_Process(ip, packetData[i]);
				}
				insert_Log(ip, "PC모니터링", "GETPROCESS_RESULT", ip + " 로부터 파일 목록 등록 성공");

				db.Update_Format_Pc("0", ip);
				db.CloseDB();
			}
			#endregion

            #region 클라이언트 파일 다운로드 Percent 변화
            if (packetType[0] == "CLIENT_FILE_PROGRESS")
            {
                try
                {
                    string client_IP = packetData[0];
                    string percent = packetData[1];
                    string file_Name = packetData[2];
                    string server_IP = packetData[3];
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    db.Update_PC_Percent(client_IP, percent);
                    if (percent == "100")
                    {
                        db.Update_pc_exe_check(client_IP, 1);
                        db.Update_PC_Percent(client_IP, "0");
                        db.update_Connect(client_IP, "0");
                        db.update_Connect(server_IP, "0");
                        db.update_Complete(client_IP, 1);


                        insert_Log(client_IP, "PC셋팅", "CLIENT_DOWNLOAD_COMPLETE", server_IP + " 로부터 파일 다운로드 완료 성공");

                        if (AsynchronousSocketListener.accept_Check == false)
                            serverConnect = false;
                    }
                    db.CloseDB();
                    //전송
                    admin_File_Progress(client_IP, percent, server_IP, file_Name);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("클라이언트 파일 다운로드 Percent 변화" + ex.Message);
                }
            }
            #endregion

            #region 파일 전송 실패 로그 필요
            if (packetType[0] == "CLIENT_SERVER_FAIL")
            {
                string server_IP = packetData[0];
                string client_IP = packetData[1];
                string file_name = packetData[2];

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    db.update_ExeFile_Pc(client_IP, null);
                    db.Update_PC_Percent(client_IP, "0");
                    db.update_Connect(client_IP, "0");
                    db.update_Connect(server_IP, "0");
                    db.CloseDB();

                    admin_Connect_Update(client_IP, "0");
                    admin_File_Progress(client_IP, "0", server_IP, "");
                    admin_Connect_Update(server_IP, "0");

                    insert_Log(server_IP, "PC셋팅", "CLIENT_SERVER_FAIL", "Client(" + client_IP + ")로부터 응답없음");
                    Console.WriteLine("CLIENT_SERVER_FAIL 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_SERVER_FAIL 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            //로그 필요
            if (packetType[0] == "CLIENT_CLIENT_FAIL")
            {
                string client_IP = packetData[0];
                string server_IP = packetData[1];
                string file_name = packetData[2];

                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    List<CNoneFilePc> Exe_Check = db.File_Check_For_Type("exe");

                    for (int i = 0; i < Exe_Check.Count; i++)
                    {
                        if (Exe_Check[i].pc_Ip == client_IP)
                        {

                            db.update_ExeFile_Pc(client_IP, null);
                            db.Update_PC_Percent(client_IP, "0");
                            db.update_Connect(client_IP, "0");
                            db.update_Connect(server_IP, "0");


                            Exe_Check[i].complete = 0;
                            Exe_Check[i].connect = 0;
                          
                            admin_Connect_Update(client_IP, "0");
                            admin_File_Progress(client_IP, "0", server_IP, "");
                            admin_Connect_Update(server_IP, "0");
                        }
                    }
                    db.CloseDB();
                  
                    insert_Log(client_IP, "PC셋팅", "CLIENT_CLIENT_FAIL", "서버로부터 응답없음(" + server_IP + ")");
                    db.CloseDB();

                    IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                    string myIP = IPHost.AddressList[0].ToString();
                    if (myIP == server_IP)
                    {
                        if (AsynchronousSocketListener.accept_Check == false)
                            serverConnect = false;
                    }
                }
                catch (Exception)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 소프트웨어 사용 시간 결과(===============창근변경==================)
            if (packetType[0] == "CLIENT_SOFTWARE_USE_CHECK_RESULT")
            {
                string ip = packetData[0];
                string name = packetData[1];
                string time = packetData[2];
                try
                {
                    use_software(packetData[0], packetData[1], packetData[2]);
                    insert_Log(ip, "PC모니터링", "CLIENT_SOFTWARE_USE_CHECK_RESULT", ip + " 소프트웨어 사용시간 체크 성공");
                    insert_Log2(ip, "소프트웨어 사용시간", name, time);
                }
                catch (Exception e)
                {
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #endregion

            //===========================================================================

            #region window
            if (packetType[0] == "CLIENT_WINDOWDELETE")//0:패킷 1 ip 2 id 3 pw
            {
                try
                {
                    string sendData = string.Empty;

                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3] + "#"; //id
                    sendData += packetData[4];

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC셋팅", "CLIENT_WINDOWDELETE", packetData[2] + " 전체 윈도우 계정 삭제 성공");
                    Console.WriteLine("CLIENT_WINDOWDELETE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_WINDOWDELETE 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 제어판
            if (packetType[0] == "CLIENT_CONTROL_PANEL")
            {
                try
                {
                    string sendData = string.Empty;

                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //true or false

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC제어", "CLIENT_CONTROL_PANEL", "제어판 제어 실행 or 해제 성공");
                    Console.WriteLine("CLIENT_CONTROL_PANEL 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_CONTROL_PANEL 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            # endregion

            #region 포맷
            if (packetType[0] == "CLIENT_FORMAT")
            {
                try
                {
					string point_name = string.Empty;
                    CMyDB db = new CMyDB(ConString);

                    db.ConnectDB();
                    List<CComputer> pcs = db.SelectPC_For_IP(packetData[2]);
					point_name = pcs[0].pointer_name;

                    db.CloseDB();

					if (packetData[3] == "1")
					{
						if (pcs[0].pc_state == "0")
						{
							string sendData = string.Empty;
							sendData += "TOADMIN+";
							sendData += "ADMIN_FORMAT_RESULT_C+";    //패킷 타입.
							sendData += "복원 지점이 없습니다." + "#";

							sendData += "&";    //마지막 .

							client.Send(sendData);

							insert_Log(packetData[2], "PC셋팅", "CLIENT_FORMAT", "복원지점이 없습니다.");
							return;
						}
						if (pcs[0].pc_state == "1")
						{
							string sendData = string.Empty;
							sendData += packetData[0] + "+";		//CLIENT_FORMAT
							sendData += packetData[2] + "#";		//ip
							sendData += packetData[3] + "#";		//1(선택) or 2(전체)
							sendData += pcs[0].key_state;

							byte[] packet = Encoding.Default.GetBytes(sendData);
							server.SendData(packetData[2], packet);

							Console.WriteLine("CLIENT_FORMAT 시작");

							insert_Log(packetData[2], "PC셋팅", "CLIENT_FORMAT", "초기 지점으로 복원.");
						}
					}

					if (packetData[3] == "2")
					{
						if (pcs[0].pc_state == "2")
						{
							string sendData = string.Empty;
							sendData += packetData[0] + "+";		//CLIENT_FORMAT
							sendData += packetData[2] + "#";		//ip
							sendData += packetData[3] + "#";		//1(선택) or 2(전체)
							sendData += pcs[0].key_state + "#";
							sendData += point_name;

							byte[] packet = Encoding.Default.GetBytes(sendData);
							server.SendData(packetData[2], packet);

							Console.WriteLine("CLIENT_FORMAT 시작");

							insert_Log(packetData[2], "PC셋팅", "CLIENT_FORMAT", "설정 지점으로 복원.");
						}
						else
						{
							string sendData = string.Empty;
							sendData += "TOADMIN+";
							sendData += "ADMIN_FORMAT_RESULT_C+";    //패킷 타입.
							sendData += "설정된 복원 지점이 없습니다." + "#";

							sendData += "&";    //마지막 .

							client.Send(sendData);
						}
					}
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_FORMAT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion


            #region ip변경
            if (packetType[0] == "CLIENT_IPCHANGE")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    //db.update_File_Pc(packetData[2], null, null); //클라이언트 데이터 초기화.

                    string sendData = string.Empty;
                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    //insert_Log(packetData[2], "PC셋팅", "CLIENT_IPCHANGE", packetData[2] + " 에서 " + packetData[3] + " 로 PC IP주소 변경 성공");
                    db.CloseDB();
                    Console.WriteLine("CLIENT_IPCHANGE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_IPCHANGE 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 소프트웨어
            if (packetType[0] == "CLIENT_software")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //소프트웨어

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_software", "소프트웨어 설치여부 판단 성공");
                    Console.WriteLine("CLIENT_software 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_software 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            #endregion

            #region 소프트웨어 제대로 작동        <====선희
            if (packetType[0] == "CLIENT_soft_respond")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //소프트웨어

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_soft_respond", "소프트웨어 작동 여부 판단 성공");
                    Console.WriteLine("CLIENT_soft_respond 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_soft_respond 실패 : " + e.Message);
                }
            }
            #endregion

            #region 디바이스 결과(false일 경우)
            if (packetType[0] == "CLIENT_DEVICE_RESULT")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    CMyDB db = new CMyDB(ConString);

                    string[] str1;
                    string[] str2;
                    string[] str3;
                    str1 = packetData[1].Split('\v'); //keyboard
                    str2 = packetData[2].Split('\v'); //moniter
                    str3 = packetData[3].Split('\v'); //mouse
                    mydb.Device_insert(packetData[0], str1[2], str2[2], str3[2]);

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DEVICE_LOG" + "+";
                    sendData += packetData[0] + "#";
                    sendData += str1[2] + "#";
                    sendData += str2[2] + "#";
                    sendData += str3[2];


                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(packetData[0], "PC모니터링", "CLIENT_DEVICE_RESULT", "디바이스 점검 결과 성공");
                    Console.WriteLine("CLIENT_DEVICE_RESULT 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_DEVICE_RESULT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }

            }
            #endregion

            #region 소프트웨어 결과
            if (packetType[0] == "CLIENT_SOFTWARERESULT")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    Console.WriteLine("프로그램 명 : " + packetData[0].ToString());
                    Console.WriteLine("상태 : " + packetData[1].ToString());
                    Console.WriteLine("IP : " + packetData[2]);
                    mydb.clinet_Ipsate(packetData[0], packetData[1], packetData[2]);

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "SOFTWARE_LOG+";    //패킷 타입.
                    sendData += packetData[2] + "#";
                    sendData += packetData[0] + "#";
                    sendData += packetData[1];
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_SOFTWARERESULT", "소프트웨어 설치 여부 판단 결과 성공");
                    //TODO : Admind으로 전송해야됨 ...
                    // ClientSoftwareresultGet(s, packetData[0], packetData[1], packetData[2]);
                    Console.WriteLine("CLIENT_SOFTWARERESULT 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_SOFTWARERESULT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            #endregion

            #region 소프트웨어 제대로 작동 결과
            if (packetType[0] == "CLIENT_SOFT_RESPOND_RESULT")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    Console.WriteLine("프로그램 명 : " + packetData[0].ToString());
                    Console.WriteLine("상태 : " + packetData[1].ToString());
                    Console.WriteLine("IP : " + packetData[2]);
                    mydb.clinet_Ipsate(packetData[0], packetData[1], packetData[2]);

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "SOFTWARE_RESPOND_LOG+";    //패킷 타입.
                    sendData += packetData[2] + "#";
                    sendData += packetData[0] + "#";
                    sendData += packetData[1];
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_SOFT_RESPOND_RESULT", "소프트웨어 작동 여부 판단 결과 성공");
                    //TODO : Admin으로 전송해야됨 ...
                    Console.WriteLine("CLIENT_SOFT_RESPOND_RESULT 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_SOFT_RESPOND_RESULT 실패 : " + e.Message);
                }
            }

            #endregion

            #region IP체인지 결과
            if (packetType[0] == "CLIENT_IPCHANGESTATE")
            {

                Console.WriteLine("변경 전 IP : " + packetData[0]);
                Console.WriteLine("변경 후 IP : " + packetData[1]);
                Console.WriteLine("맥주소 : " + packetData[2]);
                Console.WriteLine("결과  : " + packetData[3]);
                insert_Log(packetData[0], "PC모니터링", "CLIENT_IPCHANGESTATE", packetData[0] + " 에서 " + packetData[1] + " 로 IP주소 변경 결과 성공");
            }
            #endregion

            #region 윈도우계정로그인설정
            if (packetType[0] == "CLIENT_WindowLogin")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3] + "#"; //account
                    sendData += packetData[4];//password

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC셋팅", "CLIENT_WindowLogin", "윈도우 계정 로그인 설정 성공");
                    Console.WriteLine("CLIENT_WindowLogin 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_WindowLogin 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

			#region 포맷 알림
			if (packetType[0] == "CLIENT_FORMAT_RESULT")
			{
				/*
				Console.WriteLine(packetData[0]); //ip
				Console.WriteLine(packetData[1]); //맥어드레스
				Console.WriteLine(packetData[2]); //문자열
				*/
				try
				{
					string ip = packetData[0];
					string mac = packetData[1];
					string pc_type = packetData[2];
					string format_result = packetData[3];
					string pointer_name = packetData[4];

					CMyDB db = new CMyDB(ConString);
					db.ConnectDB();
					db.Update_Power_Pc(ip, 3);

					List<CComputer> com = new List<CComputer>();
					com = db.SelectPC_For_IP(ip);
					List<CRoom> room = new List<CRoom>();
					for (int i = 0; i < com.Count; i++)
					{
						if (com[i].pc_ip == ip) //ip확인
						{
							room = db.SelectRoom_For_RoomNum(com[i].room_num);
						}
					}

					db.Update_Format_Pc(int.Parse(format_result), ip);
					db.Update_Key_Format_Pc(format_result, ip);
					db.Pc_Pointer_Update(ip, pointer_name);
					db.CloseDB();

					string sendData = string.Empty;
					sendData += "TOADMIN+";
					sendData += "ADMIN_FORMAT_RESULT+";    //패킷 타입.
					sendData += ip + "#";

					sendData += mac;
					sendData += "&";    //마지막 .

					client.Send(sendData);
					insert_Log(ip, "PC셋팅", "ADMIN_FORMAT_RESULT", "포맷 완료 성공");
					insert_Log2(ip, "최근 포맷", room[0].Room_name.ToString(), pc_type);

					Console.WriteLine("포맷 결과 :" + sendData);
					Console.WriteLine("CLIENT_FORMAT_RESULT 성공");
				}
				catch (Exception e)
				{
					Console.WriteLine("CLIENT_FORMAT_RESULT 실패 : " + e.Message);
					//insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
				}
			}
			#endregion

            #region 디바이스
            if (packetType[0] == "CLIENT_DeviceStatus")
            {
                try
                {
                    string sendData = string.Empty;

                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //device name

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);

                    Console.WriteLine("CLIENT_DeviceStatus 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_DeviceStatus 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 디바이스
            if (packetType[0] == "CLIENT_ALL_DEVICE_STATUS")
            {
                try
                {
                    for (int i = 0; i < packetData.Length - 1; i++)
                    {
                        if (packetData[i] == "\b")
                        {
                            packetData1 = packetData[i + 1].Split('\v');
                        }

                    }

                    for (int i = 1; i < packetData.Length - 1; i++)
                    {
                        string sendData = string.Empty;
                        sendData += "CLIENT_DeviceStatus+";    //패킷 타입.
                        string ip = packetData[i];
                        for (int j = 0; j < packetData1.Length; j++)
                            sendData += packetData1[j] + "#";

                        byte[] packet = Encoding.Default.GetBytes(sendData);
                        server.SendData(ip, packet);
                        insert_Log(ip, "PC모니터링", "CLIENT_DeviceStatus", "디바이스 상태 점검 성공");
                    }

                    Console.WriteLine("CLIENT_ALL_DEVICE_STATUS 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_ALL_DEVICE_STATUS 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 상태결과값
            if (packetType[0] == "CLIENT_WINDOW_RESULT")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    CMyDB db = new CMyDB(ConString);

                    mydb.Window_insert(packetData[0], packetData[1], packetData[2], packetData[3]);

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "WINDOW_LOG" + "+";
                    sendData += packetData[0] + "#";
                    sendData += packetData[1] + "#";
                    sendData += packetData[2] + "#";
                    sendData += packetData[3];
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(packetData[0], "PC셋팅", "CLIENT_WINDOW_RESULT", "윈도우 계정 로그인 설정 결과 성공");
                    Console.WriteLine("CLIENT_WINDOW_RESULT 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_WINDOW_RESULT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }

            if (packetType[0] == "CLIENT_WINDOW_DELTE_RESULT")
            {
                CMyDB mydb = new CMyDB();
                CMyDB db = new CMyDB(ConString);

                mydb.Window_update(packetData[0], packetData[1], packetData[2], packetData[3]);

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "WINDOW_LOG" + "+";
                sendData += packetData[0] + "#";
                sendData += packetData[1] + "#";
                sendData += packetData[2] + "#";
                sendData += packetData[3];
                sendData += "&";    //마지막 .
                insert_Log(packetData[0], "PC셋팅", "CLIENT_WINDOW_DELTE_RESULT", "전체 윈도우 계정 삭제 결과 성공");
                client.Send(sendData);

            }
            #endregion

            #region 소프트웨어 리스트 검색
            if (packetType[0] == "DB_Soft_Init")
            {
                try
                {
                    string str1 = string.Empty;
                    CMyDB mydb = new CMyDB();
                    str1 = mydb.soft_Init();

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBSoftList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log("관리자", "PC모니터링", "DBSoftList", "소프트웨어 설치여부 List 검색 성공");
                    Console.WriteLine("DB_Soft_Init 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Soft_Init 실패 : " + e.Message);
                }
            }
            #endregion

            #region 소프트웨어 리스트 추가
            if (packetType[0] == "DB_Soft_Insert")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    string str1 = string.Empty;
                    str1 = mydb.soft_insert(packetData[0]);
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBSoftList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log("", "PC모니터링", "DB_Soft_Insert", "소프트웨어 설치여부 List 추가 성공");
                    Console.WriteLine("DB_Soft_Insert 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Soft_Insert 실패 : " + e.Message);
                }
            }
            #endregion

            #region 소프트웨어 사용 목록
            if (packetType[0] == "DB_Soft_use_select")
            {
                try
                {
                    software_use_send(packetData[0]);
                    insert_Log("Floor", "PC모니터링", "DB_Soft_use_select", "소프트웨어 사용량 List 검색 성공");
                    Console.WriteLine("DB_Soft_Insert 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Soft_Insert 실패 : " + e.Message);
                }
            }
            #endregion

            #region 소프트웨어 리스트 삭제
            if (packetType[0] == "DB_Soft_Delete")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    string str1 = string.Empty;
                    str1 = mydb.soft_Delete(packetData[0]);
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBSoftList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log("", "PC모니터링", "DB_Soft_Delete", "소프트웨어 설치여부 List 삭제 성공");
                    Console.WriteLine("DB_Soft_Delete 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Soft_Delete 실패 : " + e.Message);
                }
            }
            #endregion

            #region 윈도우 어카운트 리스트 검색
            if (packetType[0] == "DB_Window_Init")
            {
                try
                {
                    int i = 0;
                    string str1 = string.Empty;
                    CMyDB mydb = new CMyDB();
                    str1 = mydb.window_Init();

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBWindowList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log("", "PC셋팅", "DB_Window_Init", "윈도우 계정 List 검색 성공");
                    Console.WriteLine("DB_Window_Init 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Window_Init 실패 : " + e.Message);
                }
            }
            #endregion

            #region 윈도우 어카운트 추가
            if (packetType[0] == "DB_Window_Insert")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    string str1 = string.Empty;
                    str1 = mydb.window_insert(packetData[0]);
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBWindowList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막

                    client.Send(sendData);
                    insert_Log("", "PC셋팅", "DB_Window_Insert", "윈도우 계정 List 추가 성공");
                    Console.WriteLine("DB_Window_Insert 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Window_Insert 실패 : " + e.Message);
                }
            }
            #endregion

            #region 윈도우 어카운트 삭제
            if (packetType[0] == "DB_Window_Delete")
            {
                try
                {
                    CMyDB mydb = new CMyDB();
                    string str1 = string.Empty;
                    str1 = mydb.window_Delete(packetData[0]);
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "DBWindowList" + "+";
                    sendData += str1;
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log("", "PC셋팅", "DB_Window_Delete", "윈도우 계정 List 삭제 성공");
                    Console.WriteLine("DB_Window_Delete 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DB_Window_Delete 실패 : " + e.Message);
                }
            }
            #endregion

            #region ^▽^
            #region 불필요 소프트웨어 리스트
            if (packetType[0] == "NEEDLESS_CLIENT_SOFTWARE")
            {
                try
                {
                    string str1 = string.Empty;
                    CMyDB mydb = new CMyDB();
					str1 = mydb.soft_Init1(packetData[2]);
                    string sendData = string.Empty;
					sendData += packetData[0] + "+★"; //NEEDLESS_CLIENT_SOFTWARE
                    sendData += packetData[2] + "☆"; //ip
                    sendData += str1+"☆";

					sendData += "&";

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC모니터링", "NEEDLESS_CLIENT_SOFTWARE", "불필요 소프트웨어 List 검색 성공");
                    Console.WriteLine("NEEDLESS_CLIENT_SOFTWARE 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_software 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 불필요 소프트웨어 리스트 결과
            if (packetType[0] == "NEEDLESS_CLIENT_SOFTWARERESULT")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
					sendData += "NEEDLESS_SOFTWARELIST" + "+★";
                    for (int i = 1; i < packetData.Length - 1; i++)
					{
						sendData += packetData[i] + "☆";
					}
						
					sendData += "&";
                    client.Send(sendData);
                    insert_Log(packetData[0], "PC모니터링", "NEEDLESS_CLIENT_SOFTWARERESULT", "불필요 소프트웨어 List 검색 결과 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("NEEDLESS_CLIENT_SOFTWARERESULT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 불필요 소프트웨어 삭제
            if (packetType[0] == "CLIENT_SOFTWARE_DEL")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += packetData[0] + "+";
                    sendData += packetData[2] + "#"; //ip
                    sendData += packetData[3]; //소프트웨어

                    byte[] packet = Encoding.Default.GetBytes(sendData);
                    server.SendData(packetData[2], packet);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_SOFTWARE_DEL", "불필요 소프트웨어 List 삭제 성공");
                    Console.WriteLine("CLIENT_SOFTWARE_DEL 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_SOFTWARE_DEL 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion

            #region 불필요 소프트웨어 삭제 결과
            if (packetType[0] == "CLIENT_SOFTWARE_DEL_RESULT")
            {
                try
                {
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "SOFTWARE_LOG+";    //패킷 타입.
                    sendData += packetData[2] + "#";    //IP
                    sendData += packetData[0] + "#";    //파일명
                    sendData += packetData[1];           //TRUE
                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                    insert_Log(packetData[2], "PC모니터링", "CLIENT_SOFTWARE_DEL_RESULT", "불필요 소프트웨어 List 삭제 결과 성공");
                    Console.WriteLine("CLIENT_SOFTWARE_DEL_RESULT 성공");
                }
                catch (Exception e)
                {
                    Console.WriteLine("CLIENT_SOFTWARE_DEL_RESULT 실패 : " + e.Message);
                    //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                }
            }
            #endregion
            #endregion

            #region 제어판 설정 결과
            if (packetType[0] == "CLIENT_CONTROL_PANEL_RESULT")
            {
                try
                {
                    CMyDB db = new CMyDB(ConString);
                    db.ConnectDB();
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "CLIENT_CONTROL_PANEL_RESULT+";    //패킷 타입.
                    sendData += packetData[0] + "#";    //IP
                    sendData += packetData[1] + "#";    //true or false (제어 실행 or 제어 해제)
                    sendData += "&";    //마지막 
                    db.Update_Panel_Pc(packetData[0], packetData[1]);
                    client.Send(sendData);
                    insert_Log(packetData[0], "PC제어", "CLIENT_SOFTWARE_DEL_RESULT", "제어판 제어 실행 or 해제 결과");
                    Console.WriteLine("CLIENT_CONTROL_PANEL_RESULT 성공");
                    db.CloseDB();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CLIENT_CONTROL_PANEL_RESULT 실패 : " + ex.Message);
                }
            }
            #endregion

        }

        #region  ADMIN 업데이트(최신화)
        static void admin_Upadate()
        {
            try
            {
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "UPDATE" + "+";
                sendData += "&";    //마지막 .

                client.Send(sendData);
                insert_Log("관리자", "PC모니터링", "UPDATE", "관리자 업데이트 성공");
            }
            catch(Exception ex)
            {
                Console.WriteLine("ADMIN 업데이트(최신화)" + ex.Message);
            }
        }
        #endregion

        #region 층 정보
        static private void admin_Floors_Get(Socket admin)
        {
            try
            {
                //전송 데이터
                List<CFloor> floors = null;

                //DB
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                floors = db.SelectFloor();
                db.CloseDB();

                //패킷 전송.
                int floorTypeLen = 3;

                string sendData = string.Empty;
                sendData += "TOADMIN+";
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

                client.Send(sendData);

                Console.WriteLine("층 정보 얻기 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 정보 얻기 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 층 삭제
        static void admin_Floor_Min(string floor_num)
        {
            try
            {
                int floorNum = int.Parse(floor_num);

                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                List<CRoom> rooms = db.SelectRoom_For_FloorNum(floorNum);


                for (int i = 0; i < rooms.Count; i++)//PC테이블 삭제
                    db.DeletePc_For_RoomNum(rooms[i].Room_num);


                db.DeleteRoom_For_FloorNum(floor_num);//강의실 삭제.
                db.Deletefloor_For_FloorNum(floor_num);
                db.CloseDB();

                //패킷 전송.
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_FLOOR_MIN+";    //패킷 타입.
                sendData += floor_num;

                sendData += "&";    //마지막 .
                client.Send(sendData);
                Console.WriteLine("층 삭제 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 삭제 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 층 서버 로그인
        static private void floor_LogIn(string client_IP)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                //DB에 저장된 IP가 맞는지...
                List<CFloor> floors = db.SelectFloor(client_IP);
                if (floors.Count != 0)
                {
                    //업데이트후
                    db.update_Floor_Power(client_IP, 1);

                    //LOG 정보 남기고
                    //ADMIN에게 LOG 정보 넘기기

                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "ADMIN_FLOOR_LOGIN+";    //패킷 타입.
                    sendData += client_IP; //ip
                    sendData += "&";    //마지막 .
                    insert_Log(client_IP, "PC모니터링", "ADMIN_FLOOR_LOGIN", client_IP + "가 접속 성공");
                    client.Send(sendData);
                }
                else//아니라면
                {
                    insert_Log(client_IP, "PC모니터링", "ADMIN_FLOOR_LOGIN", client_IP + " 가 접속 실패(DB에 플로어 존재 X)");
                    Console.WriteLine("------------아직 db에 플로어가 없다.-------------");
                }

                db.CloseDB();
                Console.WriteLine("층 서버 로그인 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 서버 로그인 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 층 서버 로그아웃
        static void floor_LogOut(string client_IP)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                db.update_Floor_Power(client_IP, 0);
                List<CFloor> fr = db.SelectFloor(client_IP);

                if (fr.Count == 0)
                    return;
                List<CRoom> room = db.SelectRoom_For_FloorNum(fr[0].Floor_NUM);
                for (int i = 0; i < room.Count; i++)
                {
                    List<CComputer> com = db.SelectCPc(room[i].Room_num);

                    for (int k = 0; k < com.Count; k++)
                    {
                        db.Update_Power_Pc(com[k].pc_ip, 0);
                        db.Update_PC_Percent(com[k].pc_ip, "0");
                        db.update_Connect(com[k].pc_ip, "0");
                    }
                }
                db.CloseDB();

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_FLOOR_LOGOUT+";    //패킷 타입.
                sendData += client_IP; //ip
                sendData += "&";    //마지막 .   
                insert_Log(client_IP, "PC모니터링", "ADMIN_FLOOR_LOGOUT", client_IP + " 가 종료 성공");
                client.Send(sendData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("층 서버 로그아웃" + ex.Message);
            }
        }
        #endregion

        #region 강의실 정보
        static void admin_Rooms_Get(string floor_Name, int floor_Code)
        {
            try
            {
                //DB
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                List<CRoom> rooms = db.SelectRoom_For_FloorNum(floor_Code);

                //패킷 전송.
                int typeLen = 3;

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_ROOMS_GET+";    //패킷 타입.
                sendData += floor_Code.ToString() + "#";
                sendData += (rooms.Count * typeLen).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                sendData += MyIp + "#";
                for (int i = 0; i < rooms.Count; i++)
                {
                    sendData += rooms[i].Floor_num.ToString() + "#";     //정보.
                    sendData += rooms[i].Room_num.ToString() + "#";     //정보.    
                    sendData += rooms[i].Room_name;
                    if (i + 1 != rooms.Count)
                        sendData += "#";
                }

                db.CloseDB();
                //byte 변경

                sendData += "&";    //마지막 .

                client.Send(sendData);

                Console.WriteLine("강의실 정보 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 정보 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 강의실 추가
        static void admin_Room_Add(string floor_Num, string room_Num, string room_Name)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                db.InsertRoom(int.Parse(floor_Num), int.Parse(room_Num), room_Name);

                List<CRoom> room = db.SelectRoom(room_Name, room_Num);

                db.CloseDB();

                //패킷 보내기.
                int typeLen = 2;

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_ROOM_ADD+";    //패킷 타입.
                sendData += floor_Num + "#";
                sendData += (room.Count * typeLen).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                for (int i = 0; i < room.Count; i++)
                {
                    sendData += room[i].Room_name + "#";     //정보.
                    sendData += room[i].Room_num.ToString();      //정보.
                    if (i + 1 != room.Count)
                        sendData += "#";
                }

                //byte 변경

                sendData += "&";    //마지막 .
                client.Send(sendData);

                Console.WriteLine("강의실 추가 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 추가 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 강의실 삭제
        static void admin_Room_Min(string floor_Num, string room_Name, string room_Num)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                //강의실 정보 
                List<CRoom> room = db.SelectRoom(room_Name, room_Num);

                if (room.Count == 0)
                    return;

                //PC 정보 삭제.
                db.DeletePc_For_RoomNum(room[0].Room_num);

                //강의실 삭제.
                db.DeleteRoom(room_Num, room_Name);

                db.CloseDB();

                //패킷 보내기.
                int typeLen = 2;

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_ROOM_MIN+";    //패킷 타입.
                sendData += floor_Num + "#";
                sendData += (room.Count * typeLen).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                for (int i = 0; i < room.Count; i++)
                {
                    sendData += room[i].Room_num.ToString() + "#";     //정보.
                    sendData += room[i].Room_name + "#";     //정보.
                    if (i + 1 != room.Count)
                        sendData += "#";
                }

                //byte 변경

                sendData += "&";    //마지막 .

                client.Send(sendData);

                Console.WriteLine(" 강의실 삭제 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 강의실 삭제 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region PC 정보
        static void admin_PCs_Get(Socket admin, string room_Num, string room_Name)
        {
            try
            {
                //DB
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                List<CRoom> room = db.SelectRoom(room_Name, room_Num);

                List<CComputer> pcs = db.SelectCPc(room[0].Room_num);

                db.CloseDB();

                //패킷 전송.
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_PCS_GET+";    //패킷 타입.
                sendData += room_Num + "#";
                sendData += (pcs.Count * 13).ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                for (int i = 0; i < pcs.Count; i++)
                {
                    sendData += pcs[i].room_num.ToString() + "#";     //정보.
                    sendData += pcs[i].pc_type_c + "#";     //정보.
                    sendData += pcs[i].pc_ip.ToString() + "#";     //정보.
                    sendData += pcs[i].mac + "#";     //정보.
                    sendData += pcs[i].power.ToString() + "#";     //정보.
                    sendData += pcs[i].coordinate.ToString() + "#";     //정보.     
                    sendData += pcs[i].software.ToString() + "#";     //정보.
                    sendData += pcs[i].format_file_none.ToString() + "#";     //정보.
                    sendData += pcs[i].format_file_downloding.ToString() + "#";     //정보.
                    sendData += pcs[i].last_format.ToString() + "#";     //정보.
                    sendData += pcs[i].windowaccount.ToString() + "#";     //정보.
                    sendData += pcs[i].on_time.ToString() + "#";     //정보.
                    sendData += pcs[i].off_time.ToString() + "#";     //정보.
                    sendData += pcs[i].panel.ToString() + "#"; ;     //정보.
                    sendData += pcs[i].pointer_name + "#";
                    sendData += pcs[i].pc_state;

                    if (i + 1 != pcs.Count)
                        sendData += "#";
                }

                sendData += "&";    //마지막 .
                client.Send(sendData);


                Console.WriteLine("PC 정보 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 정보 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region PC 생성 01
        static void admin_PC_Add(string pc_IP, string room_Num, string room_Name, string pc_type)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                List<CRoom> room = db.SelectRoom(room_Name, room_Num);

                if (room.Count == 0)
                    return;

                CComputer pc = new CComputer();
                pc.room_num = room[0].Room_num;
                pc.pc_type_c = "0";
                pc.pc_ip = pc_IP;
                pc.mac = "0";
                pc.power = 0;
                pc.coordinate = "10:10";
                pc.software = "";
                pc.format_file_none = 0;
                pc.format_file_downloding = 0;
                pc.last_format = "00-00-00";
                pc.windowaccount = 0;
                pc.on_time = 0;
                pc.off_time = 0;
                pc.panel = "";
                pc.pointer_name = "설정X";
                pc.pc_state = "0";
				pc.format_state = "0";
                db.InsertPc(pc);

                //db.insert_image_file(pc_IP);

                db.CloseDB();

                //패킷 전송.
                int typeLen = 16;
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_PC_ADD+";    //패킷 타입.
                sendData += room_Num + "#";
                sendData += typeLen.ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                sendData += pc.room_num.ToString() + "#";       //정보.
                sendData += pc.pc_type_c + "#";                 //정보.
                sendData += pc.pc_ip.ToString() + "#";          //정보.
                sendData += pc.mac + "#";                       //정보.
                sendData += pc.power.ToString() + "#";          //정보.
                sendData += pc.coordinate.ToString() + "#";     //정보.           
                sendData += pc.software.ToString() + "#";       //정보.
                sendData += pc.format_file_none.ToString() + "#";  //정보.
                sendData += pc.format_file_downloding.ToString() + "#";  //정보.
                sendData += pc.last_format.ToString() + "#";          //정보.
                sendData += pc.windowaccount.ToString() + "#";
                sendData += pc.on_time.ToString() + "#";
                sendData += pc.off_time.ToString() + "#";
                sendData += pc.panel.ToString() + "#";
                sendData += pc.pointer_name + "#";
                sendData += pc.pc_state.ToString();

                sendData += "&";                                //마지막 .

                client.Send(sendData);
                Console.WriteLine("PC 생성 01 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 생성 01 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region PC 생성 02
        static void admin_PC_Add(string pc_IP, string room_Num, string room_Name, int x, int y, string pc_type)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                List<CRoom> room = db.SelectRoom(room_Name, room_Num);

                if (room.Count == 0)
                    return;

                CComputer pc = new CComputer();
                pc.room_num = room[0].Room_num;
                pc.pc_type_c = pc_type;
                pc.pc_ip = pc_IP;
                pc.mac = "0";
                pc.power = 0;
                pc.coordinate = x.ToString() + ":" + y.ToString();
                pc.software = "";
                pc.format_file_none = 0;
                pc.format_file_downloding = 0;
                pc.last_format = "00-00-00";
                pc.windowaccount = 0;
                pc.on_time = 0;
                pc.off_time = 0;
                pc.panel = "";
                pc.pointer_name = "";
                pc.pc_state = "0";
				pc.format_state = "0";
                db.InsertPc(pc);

                //db.insert_image_file(pc_IP);

                db.CloseDB();

                //패킷 전송.
                int typeLen = 16;
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_PC_ADD+";    //패킷 타입.
                sendData += room_Num + "#";
                sendData += typeLen.ToString() + "#";  //(리스트의 총 길이 * 데이터 개수)
                sendData += pc.room_num.ToString() + "#";       //정보.
                sendData += pc.pc_type_c + "#";                 //정보.
                sendData += pc.pc_ip.ToString() + "#";          //정보.
                sendData += pc.mac + "#";                       //정보.
                sendData += pc.power.ToString() + "#";          //정보.
                sendData += pc.coordinate.ToString() + "#";     //정보.           
                sendData += pc.software.ToString() + "#";       //정보.
                sendData += pc.format_file_none.ToString() + "#";  //정보.
                sendData += pc.format_file_downloding.ToString() + "#";  //정보.
                sendData += pc.last_format.ToString() + "#";          //정보.
                sendData += pc.windowaccount.ToString() + "#";
                sendData += pc.on_time.ToString() + "#";
                sendData += pc.off_time.ToString() + "#";
                sendData += pc.panel.ToString() + "#";
                sendData += pc.pointer_name + "#";
                sendData += pc.pc_state.ToString();

                sendData += "&";                                //마지막 .

                client.Send(sendData);
                Console.WriteLine("PC 생성 02 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 생성 02 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region PC 삭제
        static void admin_PC_Min(string room_Num, int maxCount, List<string> del_IPs)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                for (int i = 0; i < del_IPs.Count; i++)
                {
                    string pc_IP = del_IPs[i];
                    db.deletePc(pc_IP);
                    string sendData = string.Empty;
                    sendData += "TOADMIN+";
                    sendData += "ADMIN_PC_MIN+";    //패킷 타입.
                    sendData += room_Num + "#";
                    sendData += pc_IP;

                    sendData += "&";    //마지막 .

                    client.Send(sendData);
                }
                db.CloseDB();
                Console.WriteLine("PC 삭제 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 삭제 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 소프트웨어 사용 시간
        static private void use_software(string ip, string soft_name, string use_time)
        {
            try
            {
                string sendData = string.Empty;
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                string soft_use = db.select_software_use(ip, soft_name);

                if (soft_use == "")
                {
                    db.update_use_software(ip, soft_name, use_time);
                }
                else
                {
                    string[] SOFT_USE = soft_use.Split(':');
                    string[] USE_TIME = use_time.Split(':');

                    int frist_time = int.Parse(SOFT_USE[0]) * 3600 + int.Parse(SOFT_USE[1]) * 60 + int.Parse(SOFT_USE[2]);
                    int second_time = int.Parse(USE_TIME[0]) * 3600 + int.Parse(USE_TIME[1]) * 60 + int.Parse(USE_TIME[2]);

                    int hap = frist_time + second_time;

                    int hours = hap / 3600;
                    int minute = hap % 3600 / 60;
                    int second = hap % 3600 % 60;
                    string time = hours.ToString() + " : " + minute.ToString() + " : " + second.ToString();

                    db.update_use_software(ip, soft_name, time);
                }

                software_use_send(ip);
                db.CloseDB();

                Console.WriteLine("소프트웨어 사용 시간 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용 시간 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 소프트웨어 사용 체크
        static private void software_usecheck(string ip)
        {
            try
            {
                string str1 = string.Empty;
                string sendData = string.Empty;
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                str1 = db.select_software_use_check(ip);

                sendData += "SOFTWARE_USE_CHECK+";
                sendData += str1;

                byte[] packet = Encoding.Default.GetBytes(sendData);
                server.SendData(ip, packet);
                db.CloseDB();

                Console.WriteLine("소프트웨어 사용 체크 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용 체크 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 소프트웨어 사용량 이름 입력
        static private void software_usename_insert(string software_name, string ip)
        {
            try
            {
                string str1 = string.Empty;
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                str1 = db.select_software_use_check(ip);

                string[] msg = str1.Split('#');

                for (int i = 0; i < msg.Length; i++)
                {
                    if (msg[i] == software_name)
                    {
                        return;
                    }
                }
                db.insert_software_use_name(software_name, ip);

                software_use_send(ip);

                software_usecheck(ip);

                db.CloseDB();

                Console.WriteLine("소프트웨어 사용량 이름 입력 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용량 이름 입력 실패 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region 소프트웨어 사용 시간 계산
        static private void software_use_send(string ip)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                string sendData = string.Empty;
                db.ConnectDB();

                List<File_use> soft_check = db.select_software_use_ip(ip);
                List<CComputer> pc_room = db.SelectPC_For_IP(ip);

                double hap = 0;

                for (int i = 0; i < soft_check.Count; i++)
                {
                    if (soft_check[i].file_time == "")
                    {
                        break;
                    }

                    string[] SOFT_USE = soft_check[i].file_time.Split(':');

                    double frist_time = double.Parse(SOFT_USE[0]) * 3600 + double.Parse(SOFT_USE[1]) * 60 + double.Parse(SOFT_USE[2]);

                    hap += frist_time;
                }

                double[] frist_time1 = new double[soft_check.Count];
                persent = new string[soft_check.Count];

                for (int j = 0; j < soft_check.Count; j++)
                {
                    double hap_time1 = 0;
                    double hap_time2 = 0;
                    if (soft_check[j].file_time == "")
                    {
                        break;
                    }

                    string[] SOFT_USE = soft_check[j].file_time.Split(':');

                    frist_time1[j] = double.Parse(SOFT_USE[0]) * 3600 + double.Parse(SOFT_USE[1]) * 60 + double.Parse(SOFT_USE[2]);

                    hap_time1 = (frist_time1[j] / hap) * 100;

                    hap_time2 = Math.Round(hap_time1, 1);

                    persent[j] = hap_time2.ToString() + "%";
                }

                string send = string.Empty;

                int hours;
                int minute;
                int second;
                string time;

                for (int k = 0; k < soft_check.Count; k++)
                {
                    send += soft_check[k].file_name + "#";
                    send += frist_time1[k] + "#";

                    hours = (int)frist_time1[k] / 3600;
                    minute = (int)frist_time1[k] % 3600 / 60;
                    second = (int)frist_time1[k] % 3600 % 60;
                    time = string.Format("{0}:{1}:{2}", hours.ToString(), minute.ToString(), second.ToString());

                    insert_Log2(ip, "소프트웨어 사용시간", soft_check[k].file_name, time);
                }

                sendData += "TOADMIN+";
                sendData += "DBSoftList_use" + "+";
                sendData += hap + "#";
                sendData += pc_room[0].pc_ip + "#";
                sendData += send;
                sendData += "&";    //마지막 .
                db.CloseDB();

                client.Send(sendData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("소프트웨어 사용 시간 계산" + ex.Message);
            }
        }
        #endregion

        #region 클라이언트 로그인

        static void client_Login(Socket floor, string ip, string mac, string pc_name, string type, string panel, string software)
        {
            try
            {
                //MAC 검사 후 접속한 IP와 비교.
                //MAC count 가 없으면 ,                 있을 경우에는 
                //IP 검사후                                  IP 와 비교 후
                //IP count가 있으면                        같으면 로그인
                //로그인.                                      다르면 로그에 남긴 후 전송.
                //없으면 로그에 남긴 후 전송.
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                List<CComputer> select_Mac_Pc = db.SelectComputer_For_Mac(mac);
                List<CComputer> select_IP_Pc = db.SelectPC_For_IP(ip);
                if (select_Mac_Pc.Count != 0) //MAC count 있을 경우에는 
                {
                    if (select_Mac_Pc[0].pc_ip == ip) //접속한 IP와 같으면 로그인.
                    {
                        //TODO : 로그에 로그인 정보 남기기.
                        db.Update_Power_Pc(ip, 1);
                        db.Update_Mac_Pc(ip, mac);
                        db.Update_Type_Pc(ip, type);
                        db.Update_Panel_Pc(ip, panel);
                        db.Update_Software_Pc(ip, software);
                        //==========================================================================
                        //로그인 전송.
                        List<CRoom> room = db.SelectRoom_For_RoomNum(select_Mac_Pc[0].room_num);

                        string sendData = string.Empty;
                        sendData += "TOADMIN+";
                        sendData += "ADMIN_PC_LOGIN+";    //패킷 타입.
                        sendData += room[0].Room_name.ToString() + "#";
                        sendData += room[0].Room_num.ToString() + "#";
                        sendData += select_Mac_Pc[0].pc_ip + "#"; ;
                        sendData += mac;
                        sendData += type;
                        sendData += "&";    //마지막 .
                        client.Send(sendData);
                        insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", ip + " 가 접속 성공");
                    }
                    else//접속한 IP와 다르면 
                    {
                        //TODO : 로그에 정보(IP가 변경되었습니다) 남긴 후 어드민에 전송
                        insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", select_Mac_Pc[0].pc_ip + " 에서, " + ip + " 로  변경.");
                        db.Update_Mac_Pc(select_Mac_Pc[0].pc_ip, "0");//기존 DB에 MAC 주소를 없애고

                        //===============================================================
                        //mac주소를 없애기 위해 admin서버로 보낸다..
                        List<CRoom> room = db.SelectRoom_For_RoomNum(select_Mac_Pc[0].room_num);
                        string sendData = string.Empty;
                        sendData += "TOADMIN+";
                        sendData += "ADMIN_PC_MAC_UPDATE+";    //패킷 타입.
                        sendData += room[0].Room_name.ToString() + "#";
                        sendData += room[0].Room_num.ToString() + "#";
                        sendData += select_Mac_Pc[0].pc_ip + "#"; ;
                        sendData += "0";
                        sendData += type;
                        sendData += "&";    //마지막 .
                        client.Send(sendData);


                        if (select_IP_Pc.Count != 0) //IP 검사후, Count가 있으면
                        {
                            //TODO : 로그에 로그인 정보 남긴 후 어드민에 전송
                            db.Update_Power_Pc(ip, 1);
                            db.Update_Mac_Pc(ip, mac);
                            db.Update_Type_Pc(ip, type);
                            db.Update_Panel_Pc(ip, panel);
                            //===========================================================
                            //로그인 전송
                            sendData = string.Empty;
                            sendData += "TOADMIN+";
                            sendData += "ADMIN_PC_LOGIN+";    //패킷 타입.
                            sendData += room[0].Room_name.ToString() + "#";
                            sendData += room[0].Room_num.ToString() + "#";
                            sendData += select_IP_Pc[0].pc_ip + "#"; ;
                            sendData += mac;
                            sendData += type;
                            sendData += "&";    //마지막 .
                            client.Send(sendData);
                            insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", ip + " 가 접속 성공");
                        }
                        else
                        {
                            //TODO : 없으면 로그에 남긴 후 어드민에 전송.
                            insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", ip + " 가 접속 실패(등록되지 않은 IP)");
                        }
                    }
                }
                else//MAC count 가 없으면
                {
                    if (select_IP_Pc.Count != 0) //IP 검사후, Count가 있으면
                    {
                        //TODO : 로그에 로그인 정보 남긴 후 어드민에 전송
                        db.Update_Power_Pc(ip, 1);
                        db.Update_Mac_Pc(ip, mac);
                        db.Update_Type_Pc(ip, type);
                        db.Update_Panel_Pc(ip, panel);
                        //==========================================================================
                        //로그인 전송.
                        List<CRoom> room = db.SelectRoom_For_RoomNum(select_IP_Pc[0].room_num);

                        string sendData = string.Empty;
                        sendData += "TOADMIN+";
                        sendData += "ADMIN_PC_LOGIN+";    //패킷 타입.
                        sendData += room[0].Room_name.ToString() + "#";
                        sendData += room[0].Room_num.ToString() + "#";
                        sendData += select_IP_Pc[0].pc_ip + "#"; ;
                        sendData += mac;
                        sendData += type;
                        sendData += "&";    //마지막 .

                        client.Send(sendData);
                        insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", ip + " 가 접속 성공");
                    }
                    else
                    {
                        insert_Log(ip, "PC모니터링", "CLIENT_LOGIN", ip + " 가 접속 실패(등록되지 않은 IP)");
                        Console.WriteLine("-------아직 db에 클라이언트 정보가 없음-----");
                    }
                }
                db.CloseDB();
                software_usecheck(ip); // 소프트웨어 사용 시간 체크(===============창근변경==================)                
            }
            catch(Exception ex)
            {
                Console.WriteLine("클라이언트 로그인" + ex.Message);
            }
        }
        #endregion      

        #region 클라이언트 로그아웃
        static void admin_Client_Logout(string ip)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                //==========================================================================
                //로그아웃 전송.
                List<CComputer> select_IP_Pc = db.SelectPC_For_IP(ip);

                if (select_IP_Pc.Count == 0)    //저장된 PC가 아니면.
                    return;

                if (select_IP_Pc[0].power == 3) //포맷 pc인 경우. 리턴
                    return;

                db.Update_Power_Pc(ip, 0);

                db.CloseDB();


                List<CRoom> room = db.SelectRoom_For_RoomNum(select_IP_Pc[0].room_num);

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_PC_LOGOUT+";    //패킷 타입.
                sendData += room[0].Room_name.ToString() + "#";
                sendData += room[0].Room_num.ToString() + "#";
                sendData += select_IP_Pc[0].pc_ip + "#"; ;
                sendData += "&";    //마지막 .

                client.Send(sendData);
                insert_Log(ip, "PC모니터링", "CLIENT_LOGOUT", ip + " 가 종료 성공");
                Console.WriteLine("클라이언트 로그아웃 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("클라이언트 로그아웃 recive(x) : " + e.Message);
            }
        }
        #endregion

		

        #region 클라이언트 재접속 요청
        static void client_ReLogin(string client_ip)
        {
            try
            {
                string sendData = string.Empty;
                sendData += "RELOGIN+";    //패킷 타입.
                sendData += "&";    //마지막 .

                byte[] packet = Encoding.Default.GetBytes(sendData);
                server.SendData(client_ip, packet);
                insert_Log(client_ip, "PC모니터링", "RELOGIN", client_ip + " 재접속 요청");
                Console.WriteLine("클라이언트 재접속 요청 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("클라이언트 재접속 요청 recive(x) : " + e.Message);
            }
        }
        #endregion

        #region connect 상태 업데이트
        static void admin_Connect_Update(string ip, string connect)
        {
            try
            {
                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_CONNECT_UPDATE+";    //패킷 타입.
                sendData += ip + "#";
                sendData += connect;
                sendData += "&";    //마지막 .

                client.Send(sendData);
                Console.WriteLine("connect 상태 업데이트 성공 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("connect 상태 업데이트 실패 recive(x) : " + e.Message);
            }
        }
        #endregion      

        #region Progress 업데이트
        static void admin_File_Progress(string client_ip, string percent, string server_ip, string file_name)
        {
            try
            {
                //전송
                string sendData = string.Empty;
                //sendData += "TOADMIN+";
                sendData += "CLIENT_DOWNLOAD_RESULT+";    //패킷 타입.
                sendData += client_ip + "#";
                sendData += Path.GetFileName(file_name) + "#";
                sendData += percent+"#";

                //sendData += "&";    //마지막 .

                //client.Send(sendData);

                byte[] packet = Encoding.Default.GetBytes(sendData);   //전송

                pc_server.SendData(pc_set_ip, packet);

                Console.WriteLine("파일 전송 퍼센트 변화 전송 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("파일 전송 퍼센트 변화 전송 recive(x) : " + e.Message);
            }
        }
        #endregion       

        #region 파일 전송 스레드
        static void file_Send_Thread(object s)
        {
            dataSendEvent.Reset();
            while (true)
            {
                dataSendEvent.WaitOne();
               
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                if (fpc.Count != 0)
                {
                    #region 창근전송
                    List<CNoneFilePc> none_File_Pcs = db.File_Check_For_Type(pc_filepath);
                    List<CProgram> Get_Program = db.Get_Program_Info3(pc_filepath);
                    for (int i = 0; i < fpc.Count; i++)
                    {
                        string none_Pc_IP = fpc[i].pcs[0].ip;
                        int count = 0;  // 0일때 센터서버가 클라이언트로 파일을 보낸다 아닐땐 클라이언트간 데이터 송수신
                        List<CComputer> pcs = db.SelectPC_For_NONE(1);
                        List<CComputer> com = db.SelectPC_For_IP(none_Pc_IP);
                        if (com[0].power == 0)
                        {
                            none_File_Pcs.Remove(none_File_Pcs[i]);
                            db.delete_nonefilepc(none_File_Pcs[i].pc_Ip);
                            continue;
                        }
                        //==============================================================================

                        #region 클라이언트서버==>클라이언트 파일전송
                        for (int k = 0; k < none_File_Pcs.Count; k++)
                        {
                            //Thread.Sleep(1000);

                            string[] data_Connect = none_File_Pcs[k].data_connect.Split(':');                        

                            if (none_File_Pcs[i].connect == 0 &&
                                    none_File_Pcs[i].complete == 0 &&
                                        pcs[k].power == 1 &&
                                            pcs[k].format_file_downloding == 1 &&
                                                data_Connect[0] == "0")
                            {
                                count++;
                                //클라전송.
                                db.update_Connect(pcs[k].pc_ip, "1:" + none_File_Pcs[i].pc_Ip);  //서버 DB 등록
                                db.update_Connect(none_File_Pcs[i].pc_Ip, "2:" + pcs[k].pc_ip);  //클라 DB 등록

                                //admin 전송
                                //admin_Connect_Update(pcs[k].pc_ip, "1:" + none_File_Pcs[i].pc_Ip);
                                //admin_Connect_Update(none_File_Pcs[i].pc_Ip, "2:" + pcs[k].pc_ip);

                                insert_Log(pcs[k].pc_ip, "PC셋팅", "파일 전송", "Data Server를 실행 (클라이언트 IP : " + pcs[k].pc_ip + ")");
                                insert_Log(none_Pc_IP, "PC셋팅", "파일 전송", "Data Client를 실행 (서버 IP : " + none_File_Pcs[i].pc_Ip + ")");

                                string sendData = string.Empty;
                                sendData += "DATA_SERVER_START+";            //패킷 타입.
                                sendData += none_File_Pcs[i].pc_Ip + "#";       //서버 IP
                                sendData += pcs[k].pc_ip + "#";                       //클라 IP
                                sendData += Get_Program[0].Program_Name;

                                byte[] packet = Encoding.Default.GetBytes(sendData);
                                server.SendData(pcs[0].pc_ip, packet);

                                sendData = string.Empty;
                                sendData += "DATA_CLIENT_START+";    //패킷 타입.                               
                                sendData += pcs[k].pc_ip + "#";       //클라 IP
                                sendData += Get_Program[0].Program_Name;
                                //sendData += none_File_Pcs[i].pc_Ip;       //서버 IP


                                byte[] packet1 = Encoding.Default.GetBytes(sendData);
                                server.SendData(none_File_Pcs[i].pc_Ip, packet1);

                                none_File_Pcs[i].connect = 1;
                                db.update_Connectstate(none_Pc_IP, none_File_Pcs[i].connect);
                                break;
                            }
                        }
                        #endregion

                        #region 센터서버==>클라이언트 파일전송
                        if (count == 0) //서버에서 전송.
                        {
                            if (none_File_Pcs.Count == 0 ||
                                            none_File_Pcs[i].connect == 1 ||
                                                none_File_Pcs[i].complete == 1 ||
                                                    AsynchronousSocketListener.accept_Check == true)
                                continue;

                            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
                            string myip = host.AddressList[0].ToString();

                            db.update_Connect(none_Pc_IP, "3:" + myip);  //클라 DB 등록

                            //admin 등록
                            admin_Connect_Update(none_Pc_IP, "3:" + myip);

                            insert_Log(none_Pc_IP, "PC셋팅", "파일 전송", "Data Client를 실행 (서버 : Center Server)");

                            AsynchronousSocketListener.FileToSend = fpc[i].file_path;
                            AsynchronousSocketListener.acceptIP = none_Pc_IP;
                            AsynchronousSocketListener.accept_Check = true;

                            string sendData = string.Empty;
                            sendData += "DATA_CLIENT_START+";    //패킷 타입.
                            sendData += myip + "#";       //서버 IP
                            sendData += Get_Program[0].Program_Name;

                            byte[] packet = Encoding.Default.GetBytes(sendData);
                            server.SendData(none_Pc_IP, packet);

                            none_File_Pcs[i].connect = 1;
                            db.update_Connectstate(none_Pc_IP, none_File_Pcs[i].connect);


                            break;
                        }
                        #endregion
                    }

                    //완료된것 지우기.
                    List<CNoneFilePc> deleteList1 = new List<CNoneFilePc>();
                    for (int f = 0; f < none_File_Pcs.Count; f++)
                    {
                        if (none_File_Pcs[f].complete == 1)
                        {
                            deleteList1.Add(none_File_Pcs[f]);
                        }
                    }

                    for (int d = 0; d < deleteList1.Count; d++)
                    {
                        none_File_Pcs.Remove(deleteList1[d]);
                        db.delete_nonefilepc(deleteList1[d].pc_Ip);
                        db.Update_pc_exe_check(deleteList1[d].pc_Ip, 0);
                        db.Update_pc_image_check(deleteList1[d].pc_Ip, 0);
                        fpc.Remove(fpc[d]);
                    }
                    deleteList1.Clear();

                    db.CloseDB();
                    if (none_File_Pcs.Count == 0)                    
                        dataSendEvent.Reset();                   
                    else
                        dataSendEvent.Set();

                    #endregion

                    #region 센터서버==>클라이언트 파일전송(청원)
                    //for (int i = 0; i < fpc.Count; i++)
                    //{

                    //    if (AsynchronousSocketListener.accept_Check == true)
                    //        continue;

                    //  //  for (int j = 0; j < fpc[i].pcs.Count; j++)
                    //  //  {
                    //        string none_Pc_IP = fpc[i].pcs[0].ip;

                    //        IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
                    //        string myip = host.AddressList[0].ToString();

                    //        AsynchronousSocketListener.FileToSend = fpc[i].file_path;
                    //        AsynchronousSocketListener.acceptIP = none_Pc_IP;
                    //        AsynchronousSocketListener.accept_Check = true;

                    //        string sendData = string.Empty;
                    //        sendData += "DATA_CLIENT_START+";    //패킷 타입.
                    //        sendData += myip + "#";       //서버 IP
                    //        sendData += fpc[i].file_path;

                    //        byte[] packet = Encoding.Default.GetBytes(sendData);
                    //        server.SendData(none_Pc_IP, packet);
                    //  //  }

                    //    fpc.Remove(fpc[i]);
                    //    break;
                    //}

                    //if (fpc.Count == 0)
                    //    dataSendEvent.Reset();
                    //else
                    //    dataSendEvent.Set();

                    //db.CloseDB();

                    #endregion
                }

            }

        }
        #endregion     

        #region 파일 길이 알아오기.
        static long fileLenCheck(string FileRoute)
        {
            try
            {
                string fileName = FileRoute.Replace("\\", "/");
                long fileLen = 0;
                while (fileName.IndexOf("/") > -1)
                {
                    fileName = fileName.Substring(fileName.IndexOf("/") + 1);
                }
                FileInfo fileInfo = new FileInfo(FileRoute);
                if (fileInfo.Exists == true)//파일이 존재할때.
                    fileLen = fileInfo.Length;
                //else//파일이 존재하지 않을때

                Console.WriteLine("파일 길이 알아오기 성공 recive(x)");
                return fileLen;
            }
            catch (Exception e)
            {
                Console.WriteLine("파일 길이 알아오기 실패 recive(x) : " + e.Message);
                return 0;
            }
        }
        #endregion      

        #region 플로어 종료시 로그아웃처리
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }


        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                default:
                    try
                    {
                        floor_LogOut(MyIp);
                        Thread.Sleep(100);
                        insert_Log(MyIp, "PC모니터링", "Floor 종료", "층(서버) PC가  종료");

                    }
                    catch (Exception)
                    {
                        //insert_Log("관리자", "DB", "DB 입력 형식이 맞지 않습니다.");
                    }
                    return false;
            }
        }
        #endregion

        #region PC ON THREAD
        static void pc_On_Thread(object s)
        {
            try
            {
                // pc_On_Mac가 존재하고 pc_On_Event가 열렸을 때 (pc on 명령)
                // pc_On_Mac에 있는 mac 주소를
                pc_On_Event.Reset();
                while (true)
                {
                    pc_On_Event.WaitOne();

                    if (pc_On_Mac.Count == 0)
                        pc_On_Event.Reset();

                    //브로드 캐스트 소켓을 만들어 각 mac 주소에 Send 한다.
                    for (int i = 0; i < pc_On_Mac.Count; i++)
                        MACAddress.SendWOLPacket(pc_On_Mac[i].mac);

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("PC ON THREAD" + ex.Message);
            }
        }
        #endregion

        #region 로그 입력
        static public void insert_Log(string ip, string type_name, string log_name, string log)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string time = DateTime.Now.ToString("HH:mm:ss");
                int logType = db.GetLogType_For_TypeName(type_name);
                db.InsertLog(ip, logType, log_name, log, today, time);
                db.CloseDB();

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_LOG" + "+";
                sendData += type_name + "#";
                sendData += ip + "#";
                sendData += log_name + "#";
                sendData += log + "#";
                sendData += today + "#";
                sendData += time;

                sendData += "&";    //마지막 .

                client.Send(sendData);

                Console.WriteLine("로그 입력 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("로그 입력 recive(x) : " + e.Message);
            }
        }
        static public void insert_Log2(string ip, string type_name, string log_name, string log)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string time = DateTime.Now.ToString("HH:mm:ss");
                int logType = db.GetLogType_For_TypeName(type_name);
                db.InsertLog2(ip, logType, log, today, time);
                db.CloseDB();

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_LOG2" + "+";
                sendData += ip + "#";
                sendData += type_name + "#";
                sendData += log + "#";
                sendData += log_name + "#";
                sendData += today + "#";
                sendData += time;

                sendData += "&";    //마지막 .

                client.Send(sendData);

                Console.WriteLine("로그 입력 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("로그 입력 recive(x) : " + e.Message);
            }
        }

        static public void insert_Log3(string ip, string type_name, string room_name, string start_time, string time_log)
        {
            try
            {
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string time = DateTime.Now.ToString("HH:mm:ss");
                int logType = db.GetLogType_For_TypeName(type_name);
                db.InsertLog3(ip, logType, room_name, start_time, time_log, today, time);
                db.CloseDB();

                string sendData = string.Empty;
                sendData += "TOADMIN+";
                sendData += "ADMIN_LOG2" + "+";
                sendData += ip + "#";
                sendData += type_name + "#";
                sendData += room_name + "#";
                sendData += start_time + "#";
                sendData += time_log + "#";
                sendData += time + "#";
                sendData += today;
                sendData += "&";    //마지막 .

                client.Send(sendData);

                Console.WriteLine("로그 입력 recive(x)");
            }
            catch (Exception e)
            {
                Console.WriteLine("로그 입력 recive(x) : " + e.Message);
            }
        }
        #endregion        

        #region 알람정보
        static void Information_Alram()
        {
            try
            {
                Days_AlarmConnect = false;
                Date_AlarmConnect = false;

                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                Console.WriteLine("들옴 information");

                //======시스템시간 구하기============================================================
                DateTime dtNow = DateTime.Now;      //시스템 시간
                string NowTime = dtNow.ToLongTimeString();
                string[] NowTime1 = NowTime.Split(' ');
                string sys_am_pm = NowTime1[0];
                string time = NowTime1[1];
                string[] time2 = time.Split(':');
                string time3 = time2[0] + time2[1] + time2[2];
                int sys_nowtime = int.Parse(time3.ToString());

                if (sys_am_pm == "오전")
                {
                    sys_am_pm = "0";
                }
                if (sys_am_pm == "오후")
                {
                    sys_am_pm = "1";
                }

                //====================================================================================

                //======시스템요일 구하기=======================================================
                CultureInfo cultures = CultureInfo.CreateSpecificCulture("ko-KR");
                string Date = DateTime.Now.ToString(string.Format("yyyy-MM-dd@ddd", cultures));
                string[] packet = Date.Split('@');

                string sys_day = packet[1];

                if (sys_day == "월")
                {
                    sys_day = "1";
                }
                if (sys_day == "화")
                {
                    sys_day = "2";
                }
                if (sys_day == "수")
                {
                    sys_day = "3";
                }
                if (sys_day == "목")
                {
                    sys_day = "4";
                }
                if (sys_day == "금")
                {
                    sys_day = "5";
                }
                if (sys_day == "토")
                {
                    sys_day = "6";
                }
                if (sys_day == "일")
                {
                    sys_day = "7";
                }
                //=====================================================================================

                on_Date = db.Select_Ondate();
                off_Date = db.Select_Offdate();
                on_Days = db.Select_Ondays(int.Parse(sys_day), int.Parse(sys_am_pm), sys_nowtime);
                off_Days = db.Select_Offdays(int.Parse(sys_day), int.Parse(sys_am_pm), sys_nowtime);


                string on_alram_day = string.Empty;
                string off_alram_day = string.Empty;
                string on_am_pm = string.Empty;
                string off_am_pm = string.Empty;
                string total = string.Empty;

                int on;
                int off;

                #region //===================================날짜알람==========================================================================================
                if (on_Date.Count != 0 && off_Date.Count != 0)
                {
                    //==============================날짜알람(on/off 날짜+오전오후(0,1)로 차 구하기)================================================================
                    string[] d = on_Date[0].ALARM_On_time.Split(':');
                    string Ondate = d[0] + d[1] + d[2];
                    string[] d2 = off_Date[0].ALARM_Off_time.Split(':');
                    string Offdate = d2[0] + d2[1] + d2[2];

                    off = int.Parse(off_Date[0].ALARM_Off_year) + int.Parse(off_Date[0].ALARM_Off_month) + int.Parse(off_Date[0].ALARM_Off_day) + off_Date[0].ALARM_AM_PM + int.Parse(Offdate);
                    on = int.Parse(on_Date[0].ALARM_On_year) + int.Parse(on_Date[0].ALARM_On_month) + int.Parse(on_Date[0].ALARM_On_day) + on_Date[0].ALARM_AM_PM + int.Parse(Ondate);

                    int On_Off_total = on - off;

                    if (On_Off_total == 0)
                    {
                        Date_ip_On = db.Select_All_On(on_Date[0].ALARM_On_year, on_Date[0].ALARM_On_month, on_Date[0].ALARM_On_day, 0, on_Date[0].ALARM_AM_PM, on_Date[0].ALARM_On_time);
                        Date_ip_Off = db.Select_All_OFF(off_Date[0].ALARM_Off_year, off_Date[0].ALARM_Off_month, off_Date[0].ALARM_Off_day, 0, off_Date[0].ALARM_AM_PM, off_Date[0].ALARM_Off_time);
                        OnOn();
                        OffOff();
                    }
                    else if (On_Off_total < 0)     //on 시간이 먼저
                    {
                        Date_ip_On = db.Select_All_On(on_Date[0].ALARM_On_year, on_Date[0].ALARM_On_month, on_Date[0].ALARM_On_day, 0, on_Date[0].ALARM_AM_PM, on_Date[0].ALARM_On_time);
                        OnOn();
                    }
                    else if (On_Off_total > 0)     //off 시간이 먼저
                    {
                        Date_ip_Off = db.Select_All_OFF(on_Date[0].ALARM_On_year, on_Date[0].ALARM_On_month, on_Date[0].ALARM_On_day, 0, on_Date[0].ALARM_AM_PM, on_Date[0].ALARM_On_time);
                        OffOff();
                    }
                }
                if (on_Date.Count >= 1 && off_Date.Count == 0)
                {
                    Date_ip_On = db.Select_All_On(on_Date[0].ALARM_On_year, on_Date[0].ALARM_On_month, on_Date[0].ALARM_On_day, 0, on_Date[0].ALARM_AM_PM, on_Date[0].ALARM_On_time);
                    OnOn();
                }
                if (off_Date.Count >= 1 && on_Date.Count == 0)
                {
                    Date_ip_Off = db.Select_All_OFF(off_Date[0].ALARM_Off_year, off_Date[0].ALARM_Off_month, off_Date[0].ALARM_Off_day, 0, off_Date[0].ALARM_AM_PM, off_Date[0].ALARM_Off_time);
                    OffOff();
                }

                #endregion//=======================================================================================

                #region//=================================요일알람============================================================================================
                if (on_Days.Count != 0 && off_Days.Count != 0)
                {



                    if (int.Parse(sys_day) == on_Days[0].ALARM_On_days && int.Parse(sys_day) == off_Days[0].ALARM_Off_days)
                    {
                        string[] d = on_Days[0].ALARM_On_time.Split(':');
                        string on_day_time = d[0] + d[1] + d[2];

                        string[] d2 = off_Days[0].ALARM_Off_time.Split(':');
                        string off_day_time = d2[0] + d2[1] + d2[2];
                        int dayon = int.Parse(on_Days[0].ALARM_On_days + on_Days[0].ALARM_AM_PM + on_day_time);
                        int dayoff = int.Parse(off_Days[0].ALARM_Off_days + off_Days[0].ALARM_AM_PM + off_day_time);
                        int daytotal = dayon - dayoff;

                        if (daytotal == 0)
                        {
                            string a = "";
                            Days_ip_On = db.Select_All_On(a, a, a, on_Days[0].ALARM_On_days, on_Days[0].ALARM_AM_PM, on_Days[0].ALARM_On_time);
                            Days_ip_Off = db.Select_All_OFF(a, a, a, off_Days[0].ALARM_Off_days, off_Days[0].ALARM_AM_PM, off_Days[0].ALARM_Off_time);
                            OnOn();
                            OffOff();
                        }
                        else if (daytotal < 0)     //on 시간이 먼저
                        {
                            string a = "";
                            Days_ip_On = db.Select_All_On(a, a, a, on_Days[0].ALARM_On_days, on_Days[0].ALARM_AM_PM, on_Days[0].ALARM_On_time);
                            OnOn();
                        }
                        else if (daytotal > 0)     //off 시간이 먼저
                        {
                            string a = "";
                            Days_ip_Off = db.Select_All_OFF(a, a, a, off_Days[0].ALARM_Off_days, off_Days[0].ALARM_AM_PM, off_Days[0].ALARM_Off_time);
                            OffOff();
                        }
                    }
                    else if (int.Parse(sys_day) == on_Days[0].ALARM_On_days && int.Parse(sys_day) != off_Days[0].ALARM_Off_days)
                    {
                        string a = "";
                        Days_ip_On = db.Select_All_On(a, a, a, on_Days[0].ALARM_On_days, on_Days[0].ALARM_AM_PM, on_Days[0].ALARM_On_time);
                        OnOn();
                    }
                    else if (int.Parse(sys_day) == off_Days[0].ALARM_Off_days && int.Parse(sys_day) != on_Days[0].ALARM_On_days)
                    {
                        string a = "";
                        Days_ip_Off = db.Select_All_OFF(a, a, a, off_Days[0].ALARM_Off_days, off_Days[0].ALARM_AM_PM, off_Days[0].ALARM_Off_time);
                        OffOff();
                    }



                }
                if (on_Days.Count >= 1 && off_Days.Count == 0)
                {
                    string a = "";
                    Days_ip_On = db.Select_All_On(a, a, a, on_Days[0].ALARM_On_days, on_Days[0].ALARM_AM_PM, on_Days[0].ALARM_On_time);
                    OnOn();
                }
                if (off_Days.Count >= 1 && on_Days.Count == 0)
                {
                    string a = "";
                    Days_ip_Off = db.Select_All_OFF(a, a, a, off_Days[0].ALARM_Off_days, off_Days[0].ALARM_AM_PM, off_Days[0].ALARM_Off_time);
                    OffOff();
                }

                //========================================================================================================================================
                #endregion
                db.CloseDB();
            }
            catch(Exception ex)
            {
                Console.WriteLine("알람정보" + ex.Message);
            }
        }
        #endregion

        #region OffOff      선미수정
        static void OffOff()
        {
            try
            {
                Days_AlarmConnect = true;
                Date_AlarmConnect = true;

                if (off_Days.Count >= 1)
                {
                    string time = off_Days[0].ALARM_Off_time + "+off";
                    onoffthread = new Thread(new ParameterizedThreadStart(DaysOnOffThread));
                    onoffthread.Start(time);
                    onoffthread.IsBackground = true;
                }
                if (off_Date.Count >= 1)
                {
                    string time = off_Date[0].ALARM_Off_time + "+off";
                    onoffthread = new Thread(new ParameterizedThreadStart(DateOnOffThread));
                    onoffthread.Start(time);
                    onoffthread.IsBackground = true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("OffOff" + ex.Message);
            }
        }
        #endregion

        #region OnOn 선미수정
        static void OnOn()
        {
            try
            {
                Days_AlarmConnect = true;
                Date_AlarmConnect = true;

                if (on_Days.Count >= 1)
                {
                    string time = on_Days[0].ALARM_On_time + "+on";
                    onoffthread = new Thread(new ParameterizedThreadStart(DaysOnOffThread));
                    onoffthread.Start(time);
                    onoffthread.IsBackground = true;
                }
                if (on_Date.Count >= 1)
                {
                    string time = on_Date[0].ALARM_On_time + "+on";
                    onoffthread = new Thread(new ParameterizedThreadStart(DateOnOffThread));
                    onoffthread.Start(time);
                    onoffthread.IsBackground = true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("OnOn" + ex.Message);
            }
        }
        #endregion

        #region 선미
        private static void DateOnOffThread(object o)         //날짜스레드 선미수정
        {
            try
            {
                string str = (string)o;
                string[] str2 = str.Split('+');           //str2[0]은 디비저장된 알람시간 str2[1]은 on인지 off인지 비교하기위함
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();
                List<CComputer> com = new List<CComputer>();
                string ip = string.Empty;
                string mac = string.Empty;
                int off_time_count = 0;

                while (true)
                {
                    if (Date_AlarmConnect == true)
                    {
                        //Console.WriteLine("시간체크중..................");

                        DateTime dtNow = DateTime.Now;      //시스템 시간
                        string NowTime = dtNow.ToLongTimeString();
                        string[] NowTime1 = NowTime.Split(' ');
                        string sys_am_pm = NowTime1[0];
                        string sys_nowtime = NowTime1[1];

                        DateTime t1 = DateTime.Parse(sys_nowtime);
                        DateTime t2 = DateTime.Parse(str2[0]); //알람시간
                        TimeSpan t3 = t2.Subtract(t1);
                        int t4 = (int)t3.TotalSeconds;


                        if (t4 <= 50 && t4 >= 0)
                        {
                            Console.WriteLine("시간맞음!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            //현재 스레드개수


                            if (str2[1] == "on")        //켜기알람일때
                            {
                                //날짜온 리스트 저장해논거  아이피 불러와서 패킷보내면 끝

                                for (int i = 0; i < Date_ip_On.Count; i++)
                                {
                                    ip = Date_ip_On[i].PC_IP;
                                    mac = db.GetMac_For_IP(ip);
                                    if (mac == "0")
                                    {
                                        continue;
                                    }
                                    com = db.SelectPC_For_IP(ip);
                                    if (com.Count == 0)
                                    {
                                        continue;
                                    }
                                    if (com[0].power == 1)
                                    {
                                        continue;
                                    }

                                    //PC켜기
                                    int count2 = 0;

                                    for (int k = 0; k < pc_On_Mac.Count; k++)
                                    {
                                        if (pc_On_Mac[k].ip == ip)
                                            count2++;
                                    }


                                    if (count2 == 0)
                                    {
                                        CPcOn pcOn = new CPcOn();


                                        pcOn.ip = ip;
                                        pcOn.mac = mac;
                                        pc_On_Mac.Add(pcOn);
                                        Console.WriteLine("켜져라");
                                        pc_On_Event.Set();
                                        Date_AlarmConnect = false;

                                        if (on_Date[0].ALARM_On_year != "")     //날짜알람은 삭제
                                        {
                                            db.delete_Pc_On_date(ip, on_Date[i].ALARM_On_year, on_Date[i].ALARM_On_month, on_Date[i].ALARM_On_day, on_Date[i].ALARM_AM_PM, on_Date[i].ALARM_On_time);

                                            if (db.Select_Ondate_delete(ip) == "" && db.Select_Ondays_delete(ip) == "")
                                            {
                                                db.Update_pc_ON_TIME_check(ip, off_time_count);

                                                string sendData = string.Empty;
                                                sendData += "TOADMIN+";
                                                sendData += "DB_Pctimer_Delete_complete+";
                                                sendData += "ON" + "#";
                                                sendData += ip + "#";
                                                sendData += "&";    //마지막 .
                                                client.Send(sendData);
                                            }

                                        }


                                    }

                                }
                                db.CloseDB();
                                //    Thread.Sleep(59900);
                                Information_Alram();

                            }
                            if (str2[1] == "off")
                            {

                                for (int i = 0; i < Date_ip_Off.Count; i++)
                                {
                                    ip = Date_ip_Off[i].PC_IP;
                                    string sendData = string.Empty;
                                    sendData += "SHUTDOWN+";    //패킷 타입.
                                    byte[] packet1 = Encoding.Default.GetBytes(sendData);   //전송
                                    server.SendData(ip, packet1);
                                    Date_AlarmConnect = false;

                                    string time = string.Empty;

                                    if (off_Date[i].ALARM_Off_year != "")     //날짜알람 삭제
                                    {
                                        db.delete_Pc_Off_date(ip, off_Date[i].ALARM_Off_year, off_Date[i].ALARM_Off_month, off_Date[i].ALARM_Off_day, off_Date[i].ALARM_AM_PM, off_Date[i].ALARM_Off_time);

                                        if (db.Select_Offdate_delete(ip) == "" && db.Select_Offdays_delete(ip) == "")
                                        {
                                            db.Update_pc_OFF_TIME_check(ip, off_time_count);

                                            string sendData1 = string.Empty;
                                            sendData1 += "TOADMIN+";
                                            sendData1 += "DB_Pctimer_Delete_complete+";
                                            sendData1 += "OFF" + "#";
                                            sendData1 += ip + "#";
                                            sendData1 += "&";    //마지막 .
                                            client.Send(sendData1);
                                        }
                                    }
                                }
                                db.CloseDB();
                                // Thread.Sleep(59900);
                                Information_Alram();
                            }
                            return;
                        }
                    }
                    else
                        return;

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("DateOnOffThread" + ex.Message);
            }
        }

        private static void DaysOnOffThread(object o)         //요일스레드 선미수정
        {
            try
            {
                string str = (string)o;
                string[] str2 = str.Split('+');           //str2[0]은 디비저장된 알람시간 str2[1]은 on인지 off인지 비교하기위함
                CMyDB db = new CMyDB(ConString);
                db.ConnectDB();

                string ip = string.Empty;
                string mac = string.Empty;

                while (true)
                {

                    if (Days_AlarmConnect == true)
                    {
                        DateTime dtNow = DateTime.Now;      //시스템 시간
                        string NowTime = dtNow.ToLongTimeString();
                        string[] NowTime1 = NowTime.Split(' ');
                        string sys_am_pm = NowTime1[0];
                        string sys_nowtime = NowTime1[1];

                        DateTime t1 = DateTime.Parse(sys_nowtime);
                        DateTime t2 = DateTime.Parse(str2[0]); //알람시간
                        TimeSpan t3 = t2.Subtract(t1);
                        int t4 = (int)t3.TotalSeconds;

                        if (t4 <= 50 && t4 >= 0)
                        {
                            //Console.WriteLine("시간맞음!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            //현재 스레드개수

                            if (str2[1] == "on")        //켜기알람일때
                            {
                                //날짜온 리스트 저장해논거  아이피 불러와서 패킷보내면 끝

                                for (int i = 0; i < Days_ip_On.Count; i++)
                                {
                                    ip = Days_ip_On[i].PC_IP;
                                    mac = db.GetMac_For_IP(ip);
                                    if (mac == "0")
                                    {
                                        continue;
                                    }
                                    List<CComputer> com = db.SelectPC_For_IP(ip);
                                    if (com.Count == 0)
                                    {
                                        continue;
                                    }
                                    if (com[0].power == 1)
                                    {
                                        continue;
                                    }

                                    //PC켜기
                                    int count2 = 0;

                                    for (int k = 0; k < pc_On_Mac.Count; k++)
                                    {
                                        if (pc_On_Mac[k].ip == ip)
                                            count2++;
                                    }


                                    if (count2 == 0)
                                    {
                                        CPcOn pcOn = new CPcOn();


                                        pcOn.ip = ip;
                                        mac = db.GetMac_For_IP(ip);
                                        pcOn.mac = mac;
                                        pc_On_Mac.Add(pcOn);
                                        Console.WriteLine("켜져라");
                                        pc_On_Event.Set();
                                        Days_AlarmConnect = false;
                                    }
                                }
                                db.CloseDB();
                                Thread.Sleep(59900);
                                Information_Alram();
                            }
                            if (str2[1] == "off")
                            {

                                for (int i = 0; i < Days_ip_Off.Count; i++)
                                {
                                    ip = Days_ip_Off[i].PC_IP;
                                    string sendData = string.Empty;
                                    sendData += "SHUTDOWN+";    //패킷 타입.
                                    byte[] packet1 = Encoding.Default.GetBytes(sendData);   //전송
                                    server.SendData(ip, packet1);
                                    Days_AlarmConnect = false;
                                  
                                }
                                db.CloseDB();
                                Thread.Sleep(59900);
                                Information_Alram();
                            }
                            return;
                        }
                    }
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DaysOnOffThread" + ex.Message);
            }

        }
        #endregion

        static void thread_alram(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (System.DateTime.Now.Date != startDate)
                {
                    startDate = System.DateTime.Now.Date;
                    Information_Alram();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("thread_alram" + ex.Message);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("====플로어서버 시작====");

            //파일 전송 스레드.
            Thread thread = new Thread(new ParameterizedThreadStart(file_Send_Thread));
            thread.IsBackground = true;
            thread.Start();

            // PC SET 서버 열기
            if (pc_server == null) pc_server = new PcSetServer();
            pc_server.Start(12347, receive);
           

            //파일전송서버
            DataSend.AsynchronousSocketListener.port = 8500;
            DataSend.AsynchronousSocketListener.acceptIP = "0";
            Thread listener = new Thread(new ThreadStart(DataSend.AsynchronousSocketListener.StartListening));
            listener.IsBackground = true;
            listener.Start();

            Thread thread1 = new Thread(new ParameterizedThreadStart(pc_On_Thread));
            thread1.IsBackground = true;
            thread1.Start();

            //클라이언트들과 연결
            server = new CServer();
            server.Start(8888, receive);

            //모듈과 연결
            client = new CClient("61.81.99.86", 10000, receive);
            Thread workerThread1 = new Thread(client.StartClient);
            workerThread1.IsBackground = true;
            workerThread1.Start();

            //콘솔종료이벤트추가
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            //=====================DB찾은 후 플로어 로그인============
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            MyIp = IPHost.AddressList[0].ToString(); // 내 아이피 가져오기

            Thread.Sleep(50);
            CMyDB cdb = new CMyDB();
            ConString = cdb.SearchDB(MyIp);


            if (ConString == null)
            {
                Console.WriteLine("프로그램 종료 =====>DB가 없습니다");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            else
            {
                floor_LogIn(MyIp);
            }
            //======================================================
            Information_Alram();
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(thread_alram);
            timer.Interval = 1000;
            timer.Start();
            startDate = System.DateTime.Now.Date;
            while (true)
            {

            }
        }
    }
}
            