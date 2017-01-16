using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floor_Server.DB
{
    public class On_Date
    {
        public string PC_IP;
        public string ALARM_On_year;
        public string ALARM_On_month;
        public string ALARM_On_day;
        public int ALARM_AM_PM;
        public string ALARM_On_time;
    }

    public class Off_Date
    {
        public string PC_IP;
        public string ALARM_Off_year;
        public string ALARM_Off_month;
        public string ALARM_Off_day;
        public int ALARM_AM_PM;
        public string ALARM_Off_time;
    }

    public class On_Days
    {
        public string PC_IP;
        public int ALARM_On_days;
        public int ALARM_AM_PM;
        public string ALARM_On_time;
    }

    public class Off_Days
    {
        public string PC_IP;
        public int ALARM_Off_days;
        public int ALARM_AM_PM;
        public string ALARM_Off_time;
    }

    public class Ip_on
    {
        public string PC_IP;
    }
    public class Ip_off
    {
        public string PC_IP;
    }
        //시스템지점
    public class CProgram     
    {
        public string Program_Name; 
        public string Program_Path; 
        public string Program_Length; 
        public string Key_State; 
        public string Key_File_Name;
        public string Key_File_Path;
        public string Pointer_Name;
    }

    public class CPoint
    {
        public string Pointer_Name;
        public string Floor_Num;
    }

    public class CFloor         // 층 정보
    {
        public string Floor_IP;   // 층 아이피
        public int Floor_NUM;     // 층 (1층,2층....)
        public string Floor_NAME;     // 층 (1층,2층....)
        public int Floor_Power;   //전원
    }

    public class CRoom          // 강의실 정보
    {
        public int Floor_num;
        public int Room_num;
        public string Room_name;
    }
    public class CComputer
    {
        public int room_num;
        public string pc_type_c;
        public string pc_ip;
        public string mac;
        public int power;
        public string coordinate;
        public string software;
        public int format_file_none;
        public int format_file_downloding;
        public string last_format;
        public int windowaccount;
        public int on_time;
        public int off_time;
        public string panel;
        public string pointer_name;
        public string pc_state;
		public string format_state;
		public string key_state;
    }

    public class CNoneFilePc
    {
        public int Room_Num;
        public string type;
        public string pc_Ip;
        public string persent;
        public int complete;
        public int connect;
        public string data_connect;
    }

    public class SOFTWARE
    {
        public int SOFTWARENUM { get; set; }
        public string SOFTWARENAME { get; set; }
    }

    public class HARDWARE
    {
        public int HARDWARENUM { get; set; }
        public string HARDWARENAME { get; set; }
    }

    public class WINDOWACCOUNTS
    {
        public int idx { get; set; }
        public string account { get; set; }
        public string password { get; set; }
    }

    public class CFormat_File_Image_Check_C
    {
        public string pc_ip;
        public string pc_type_c;
        public string format_file_none_name_c;
        public int format_file_none;
    }

    public class CFormat_File_Exe_Check_S
    {
        public int format_file_downloding_num_s;
        public string format_file_downloding_type_s;
        public string format_file_downloding_name_s;
    }

    public class CFormat_File_Image_Check_S
    {
        public int format_file_none_num_s;
        public string format_file_none_type_s;
        public string format_file_none_name_s;
    }

    public class CLog
    {
        public string pc_ip;
        public int log_num;
        public string log_name;
        public string log_log;
        public string log_detection_day;
        public string log_detection_time;
    }

    public class File_use
    {
        public string file_name;
        public string file_time;
    }

    class CMyDB
    {
        #region 멤버
        //Database 접속 경로
        List<SOFTWARE> user;
        List<WINDOWACCOUNTS> winduser;
        public static string constring1;
        SqlConnection conn = new SqlConnection();
        private string sConnString = "";
        public static int dbcount;
        public static string[] db_name;
        #endregion

        #region 생성자
        public CMyDB(string constring)
        {
            constring1 = constring;
        }

        public CMyDB()
        {

        }
        #endregion

        #region db찾기

        public void SearchCount()
        {
            //======================DB카운트 받아오기=================================
            string sqlDBQuery = "SELECT count(*) as DatabaseCount FROM master.sys.databases";
            conn = new SqlConnection();
            conn.ConnectionString = "Server=61.81.99.86;database=master;uid=Dots;pwd=1234";
            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
            conn.Open();
            dbcount = (int)myCommand.ExecuteScalar() - 4; //자동생성된 시스템데이터베이스가 4개라 빼야한다
            if (dbcount == 0)
            {
                Console.WriteLine("프로그램 종료 ==>현재 DB가 없습니다 DB를 생성하시오 ");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            sqlDBQuery = string.Empty;
            conn.Close();
            //==========================================================================

            db_name = new string[dbcount + 4];

            //=======================DB이름 받아오기====================================
            sqlDBQuery = "SELECT name FROM master.sys.databases";
            myCommand = new SqlCommand(sqlDBQuery, conn);

            conn.Open();
            SqlDataReader rd = myCommand.ExecuteReader();

            int count = 0;
            while (rd.Read())
            {
                db_name[count] = rd["name"].ToString();
                count++;
            }
            conn.Close();
            //==========================================================================
        }

        public string SearchDB(string _MyIp)
        {
            string MyIp = _MyIp;
            string ip = string.Empty;

            while (true)
            {
                try
                {
                    SearchCount();

                    for (int i = 1; i <= dbcount; i++)
                    {
                        string dbname = db_name[3 + i];                     ///////////////////db_name[4]부터 생성한 디비이름
                        conn = new SqlConnection();
                        conn.ConnectionString = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);
                        string sqlDBQuery = "SELECT FLOOR_IP From FLOOR_SERVER where FLOOR_IP = '" + MyIp + "'";

                        SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
                        conn.Open();

                        ip = (string)myCommand.ExecuteScalar(); //자동생성된 시스템데이터베이스가 4개라 빼야한다
                        sqlDBQuery = string.Empty;
                        conn.Close();

                        if (ip == MyIp)
                        {
                            //이 부분에서 내 아이피에 맞는 db를 찾게 된다.
                            constring1 = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);
                            Console.WriteLine("내DB이름 : " + dbname);
                            //어드민으로 보내는 메세지
                            return constring1;
                        }
                        if (i == dbcount)
                        {
                            SearchCount();
                            i = 1;
                            Thread.Sleep(1000);
                            Console.WriteLine("DB찾는중.....................");
                        }

                    }
                }
                catch
                {
                    Console.Write("테이블 갯수 받아오기 실패");
                    return null;
                }
            }

        }
        #endregion

        #region DB 연결/ 해제
        //DB접속하기
        public void ConnectDB()
        {
            try
            {
                sConnString = constring1;
            }
            catch
            {
            }
            if (conn.State.ToString().Equals("Closed"))
            {
                conn.ConnectionString = sConnString;
            }
        }

        //DB 접속 끊기
        public void CloseDB()
        {
            if (conn != null)
            {
                conn.Close();
            }
        }
        #endregion

        //insert문========================================================================

        #region 강의실 추가
        public void InsertRoom(int floor_Num, int room_Num, string room_Name)
        {
            try
            {
                if (floor_Num == 0)
                    return;

                string str = string.Format("INSERT into ROOM( FLOOR_NUM, ROOM_NUM, ROOM_NAME) values({0}, {1}, '{2}')", floor_Num, room_Num, room_Name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("강의실 추가 실행");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 추가 오류 : " + e.Message);
            }
        }
        #endregion

        #region PC 생성
        public void InsertPc(CComputer pc)
        {
            try
            {
                string str = string.Format("INSERT into CLIENT( ROOM_NUM, PC_TYPE, PC_IP, MAC, POWER, COORDINATE, SOFTWARE, FORMAT_FILE_IMAGE, FORMAT_FILE_EXE, LAST_FORMAT, WINDOWACCOUNT, ON_TIME, OFF_TIME, PC_PANEL, POINTER_NAME, PC_STATE, FORMAT_STATE) values({0}, '{1}', '{2}', '{3}', {4}, '{5}', '{6}', {7}, {8}, {9}, '{10}', {11}, {12}, '{13}', '{14}', '{15}', {16})",
                    pc.room_num, pc.pc_type_c, pc.pc_ip, pc.mac, pc.power, pc.coordinate, pc.software, pc.format_file_none, pc.format_file_downloding, pc.last_format, pc.windowaccount, pc.on_time, pc.off_time, pc.panel, pc.pointer_name, pc.pc_state, pc.format_state);

                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 생성 실행");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 생성 오류 : " + e.Message);
            }
        }
        #endregion

        #region LOG 추가
        public void InsertLog(string ip, int log_num, string log_name, string log_log, string log_detection_day, string log_detection_time)
        {
            string str = "INSERT into LOG(PC_IP, LOG_NUM, LOG_NAME, LOG_LOG, LOG_DETECTION_DAY, LOG_DETECTION_TIME) values('" + ip + "' , " + log_num + ", '" + log_name + "', '" + log_log + "', '" + log_detection_day + "' , ' " + log_detection_time + "')";
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
                        
        public void InsertLog2(string ip, int log_num, string log_log, string log_detection_day, string log_detection_time)
        {
            string[] SOFT_USE = log_log.Split(':');
            string log = string.Format("{0}시 {1}분 {2}초", SOFT_USE[0], SOFT_USE[1], SOFT_USE[2]);
            string str = "INSERT into LOG2(PC_IP, LOG_NUM, LOG_LOG, LOG_DETECTION_DAY, LOG_DETECTION_TIME) values('" + ip + "' , " + log_num + ", '" + log + "', '" + log_detection_day + "' , ' " + log_detection_time + "')";
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void InsertLog3(string ip, int logType, string room_name, string start_time, string time_log, string log_detection_day, string log_detection_time)
        {
            string[] SOFT_USE = time_log.Split(':');
            string log = string.Format("{0}시 {1}분 {2}초", SOFT_USE[0], SOFT_USE[1], SOFT_USE[2]);
            string str = string.Format("INSERT into LOG3(PC_IP, LOG_NUM, ROOM_NAME,START_TIME,TIME_LOG, LOG_DETECTION_DAY, LOG_DETECTION_TIME) values('{0}',{1},'{2}','{3}','{4}','{5}','{6}')",
                ip, logType, room_name, start_time, time_log, log_detection_day, log_detection_time);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        #endregion

        #region 디바이스 추가
        public void Device_insert(string pip, string pk, string pmon, string pmouse)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = "INSERT into DEVICESTATUS(PC_IP,KEYBOARD,MOUSE,MONITER) values('" + pip + "','" + pk + "','" + pmon + "','" + pmouse + "')";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("디바이스 추가 실행");
            }
            catch (Exception e)
            {
                Console.WriteLine("디바이스 추가 오류 : " + e.Message);
            }
        }
        #endregion

        #region 윈도우 계정 생성
        public void Window_insert(string pip, string paccount, string ppw, string stater)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = "INSERT into CLIENT_WINDOWACCOUNTS_CHECK(PC_IP,ACCOUNT,PW,STATE,TIME,WINDOWACCOUNT) values('" + pip + "','" + paccount + "','" + ppw + "','" + stater + "','" + DateTime.Now.ToString("HH::mm::ss") + "', 1 )";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("강의실 추가 실행");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 추가 오류 : " + e.Message);
            }
        }

        #endregion

        #region NoneFilePc 추가
        public void insert_nonefilepc(int pc_room_num, string file_type, string pc_ip, string pc_persent, int pc_complete, int pc_connect, string pc_data_connect)
        {
            try
            {
                string str = "INSERT INTO NONEFILEPC(ROOM_NUM, TYPE, PC_IP, PERSENT, COMPLETE, CONNECT) VALUES('" + pc_room_num + "','" + file_type + "','" + pc_ip + "','" + pc_persent + "'," + pc_complete + "," + pc_connect + ")";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("NoneFilePc 추가 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("NoneFilePc 추가 실패 : " + e.Message);

            }
        }
        #endregion

        #region 클라이언트 이미지 추가
        //public void insert_image_file(string pc_ip)
        //{
        //    try
        //    {
        //        string str = "INSERT INTO FORMAT_FILE_IMAGE_CHECK_C(PC_IP, FORMAT_FILE_IMAGE) VALUES('" + pc_ip + "' , 0 )";
        //        SqlCommand cmd = new SqlCommand(str, conn);
        //        conn.Open();
        //        cmd.ExecuteNonQuery();
        //        conn.Close();
        //        Console.WriteLine("클라이언트 이미지 추가 성공");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("클라이언트 이미지 추가 실패 : " + e.Message);

        //    }
        //}
        #endregion

        #region 서버 이미지 추가(창근변경2)
        public void insert_image_file_server(string image_name, string image_type)
        {
            try
            {
                string str = "INSERT INTO FORMAT_FILE_IMAGE_CHECK_S(FORMAT_FILE_IMAGE_NAME_S, FORMAT_FILE_IMAGE_TYPE_S) VALUES('" + image_name + "' , '" + image_type + "')";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                //string str1 = "DELETE FROM FORMAT_FILE_IMAGE_CHECK_S WHERE FORMAT_FILE_IMAGE_NAME_S = (SELECT DISTINCT FORMAT_FILE_IMAGE_NAME_S FROM FORMAT_FILE_IMAGE_CHECK_S) ";
                //SqlCommand cmd1 = new SqlCommand(str1, conn);
                //conn.Open();
                //cmd1.ExecuteNonQuery();
                //conn.Close();

                Console.WriteLine("서버 이미지 추가 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("서버 이미지 추가 실패 : " + e.Message);

            }
        }
        #endregion

        #region 서버 고스트파일 추가(창근변경2)
        public void insert_exe_file_server(string image_name)
        {
            try
            {
                string str = "INSERT INTO FORMAT_FILE_EXE_CHECK_S(FORMAT_FILE_EXE_NAME_S) VALUES('" + image_name + "')";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                Console.WriteLine("서버 고스트파일 추가 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("서버 고스트파일 추가 실패 : " + e.Message);

            }
        }
        #endregion

        #region 소프트웨어 사용량 이름 입력(===============창근변경==================)
        public void insert_software_use_name(string soft_name, string ip)
        {
            try
            {
                //TODO :중복된것도 처리해야됨 UPdate개념.....
                string str = "INSERT INTO CLIENT_SOFTWARE_USE(PC_IP, SOFTWARE_NAME) VALUES('" + ip + "' , '" + soft_name + "')";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                Console.WriteLine("소프트웨어 사용량 이름 입력 성공 ");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용량 이름 입력 실패 : " + e.Message);

            }
        }
        #endregion

        #region PC 켜기알람 선미수정
        public void Insert_pc_ON_TIME(string pc_Ip, string year, string month, string day, int days, int am_pm, string time, int test_time)
        {
            try
            {
                string str = "INSERT INTO CLIENT_ON_TIME_ALARM_CHECK(PC_IP, ALARM_ON_YEAR, ALARM_ON_MONTH, ALARM_ON_DAY,ALARM_ON_DAYS,ALARM_AM_PM, ALARM_ON_TIME, ALARM_ON_TEST_TIME) values('" + pc_Ip + "','" + year + "','" + month + "','" + day + "'," + days + "," + am_pm + ",'" + time + "'," + test_time + ")";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("[DB]PC 켜기알람 추가 성공");
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 켜기알람 추가 실패: " + e.Message);
            }
        }
        #endregion

        #region PC 끄기알람  카운트
        public void Insert_pc_OFF_TIME(string pc_Ip, string year, string month, string day, int days, int am_pm, string time, int test_time)
        {
            try
            {
                string str = "INSERT INTO CLIENT_OFF_TIME_ALARM_CHECK(PC_IP, ALARM_OFF_YEAR, ALARM_OFF_MONTH, ALARM_OFF_DAY, ALARM_OFF_DAYS, ALARM_AM_PM, ALARM_OFF_TIME , ALARM_OFF_TEST_TIME) values('" + pc_Ip + "','" + year + "','" + month + "','" + day + "'," + days + "," + am_pm + ",'" + time + "', " + test_time + ")";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                Console.WriteLine("PC 알람끄기 추가 성공");
                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 알람끄기 추가 실패: " + e.Message);
            }
        }
        #endregion

		#region 불필요 소프트웨어 List 추가
		public void Insert_Software_Name_List(string software_name, string pc_ip)
		{
			try
			{
				string str = string.Format("INSERT INTO CLIENT_SOFTWARE_NAME(SOFTWARE_NAME, PC_IP) values('{0}','{1}')", software_name, pc_ip);
				SqlCommand cmd = new SqlCommand(str, conn);
				conn.Open();
				cmd.ExecuteNonQuery();
				Console.WriteLine("불필요 소프트웨어 추가 성공");
				conn.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("불필요 소프트웨어 추가 실패: " + e.Message);
			}
		}
		#endregion

		#region
		public void Add_Process(string ip, string process_name)
		{
			try
			{
				CMyDB db = new CMyDB();
				db.ConnectDB();
				string str = string.Format("INSERT INTO CLIENT_SOFTWARE_NAME(SOFTWARE_NAME, PC_IP) values('{0}','{1}')", process_name, ip);
				SqlCommand cmd = new SqlCommand(str, conn);
				conn.ConnectionString = constring1;
				conn.Open();
				cmd.ExecuteNonQuery();
				conn.Close();
			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.Message);
			}
		}
		#endregion
        //select문========================================================================

        #region 모든 층 얻기
        //모든 층 검색.
        public List<CFloor> SelectFloor()
        {
            try
            {
                List<CFloor> Floors = new List<CFloor>();

                string strSql = "SELECT * From FLOOR_SERVER";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CFloor sqlPosition = new CFloor();

                    sqlPosition.Floor_IP = rd["FLOOR_IP"].ToString();
                    sqlPosition.Floor_NUM = int.Parse(rd["FLOOR_NUM"].ToString());
                    sqlPosition.Floor_NAME = rd["FLOOR_NAME"].ToString();
                    sqlPosition.Floor_Power = int.Parse(rd["FLOOR_POWER"].ToString());

                    Floors.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("층 정보 검색 성공");
                return Floors;
            }
            catch (Exception e)
            {
                Console.WriteLine("층 정보 검색 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 층 IP로 얻기

        //Floor층 IP 검색
        public List<CFloor> SelectFloor(string floor_Ip)
        {
            try
            {
                string str = string.Format("SELECT * From FLOOR_SERVER where FLOOR_IP = '{0}'", floor_Ip);

                SqlCommand cmd = new SqlCommand(str, conn);

                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                List<CFloor> floors = new List<CFloor>();
                while (rd.Read())
                {
                    CFloor floor = new CFloor();

                    floor.Floor_NAME = rd["FLOOR_NAME"].ToString();
                    floor.Floor_IP = rd["FLOOR_IP"].ToString();
                    floor.Floor_NUM = int.Parse(rd["FLOOR_NUM"].ToString());

                    floors.Add(floor);
                }
                rd.Close();
                conn.Close();

                Console.WriteLine(" 층 정보 검색 IP로 얻기. 성공");
                return floors;
            }
            catch (Exception e)
            {
                Console.WriteLine(" 층 정보 검색 IP로 얻기. 실패 : " + e.Message);
                return null;
            }
        }

        #endregion

        #region 강의실 정보 얻기

        public List<CRoom> SelectRoom_For_FloorNum(int floor_num)
        {
            try
            {
                List<CRoom> Rooms = new List<CRoom>();

                string strSql = string.Format("SELECT * From ROOM where FLOOR_NUM = {0}", floor_num.ToString());

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CRoom sqlPosition = new CRoom();

                    sqlPosition.Floor_num = int.Parse(rd["FLOOR_NUM"].ToString());
                    sqlPosition.Room_num = int.Parse(rd["ROOM_NUM"].ToString());
                    sqlPosition.Room_name = rd["ROOM_NAME"].ToString();

                    Rooms.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("강의실 정보 얻기 성공");
                return Rooms;
            }
            catch
            {
                Console.WriteLine("강의실 정보 얻기 실패");
                return null;
            }
        }

        #endregion

        #region 강의실(1개) 얻기 층 이름, 층 NUM로부터.
        public List<CRoom> SelectRoom(string room_Name, string room_Num)
        {
            try
            {
                string strSql = "SELECT * From ROOM where ROOM_NAME = '" + room_Name + "' AND ROOM_NUM = " + room_Num;

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;
                List<CRoom> room = new List<CRoom>();
                while (rd.Read())
                {
                    CRoom sqlPosition = new CRoom();

                    sqlPosition.Floor_num = int.Parse(rd["FLOOR_NUM"].ToString());
                    sqlPosition.Room_num = int.Parse(rd["ROOM_NUM"].ToString());
                    sqlPosition.Room_name = rd["ROOM_NAME"].ToString();

                    room.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();

                Console.WriteLine("강의실(1개) 얻기 강의실 이름, NUM 으로부터. 성공");
                return room;
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실(1개) 얻기 강의실 이름, NUM 으로부터. 실패 : " + e.Message);
                return null;
            }

        }
        #endregion

        #region 강의실 총 PC 얻기.
        public List<CComputer> SelectCPc(int room_Num)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where ROOM_NUM = " + room_Num;

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("강의실 총 PC 얻기. 성공");
                return Pcs;
            }
            catch
            {
                List<CComputer> Pcs = new List<CComputer>();
                return Pcs;
            }
        }
        #endregion

        #region PC 얻기 MAC 으로부터
        public List<CComputer> SelectComputer_For_Mac(string pc_Mac_Address)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where MAC = '" + pc_Mac_Address + "'";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    pc.pointer_name = rd[14].ToString();
                    pc.pc_state = rd[15].ToString();

                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("PC 얻기 MAC 으로부터. 성공");
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 얻기 MAC 으로부터. 실패 : " + e.Message);
                return null;
            }

        }
        #endregion

        #region PC얻기 IP로부터
        public List<CComputer> SelectPC_For_IP(string pc_Ip)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where PC_IP = '" + pc_Ip + "'";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    pc.pointer_name = rd[14].ToString();
                    pc.pc_state = rd[15].ToString();
					pc.format_state = rd[16].ToString();
					pc.key_state = rd[16].ToString();

                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("PC얻기 IP로부터. 성공");
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC얻기 IP로부터. 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region PC얻기 파일전송상태로부터
        public List<CComputer> SelectPC_For_NONE(int pc_none_file)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where FORMAT_FILE_IMAGE = " + pc_none_file;

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    pc.pointer_name = rd[14].ToString();
                    pc.pc_state = rd[15].ToString();

                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("PC얻기 IP로부터. 성공");
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC얻기 IP로부터. 실패 : " + e.Message);
                return null;
            }
        }
        #endregion      

        #region PC얻기 ROOM_NUM로부터(창근변경2)
        public List<CComputer> SelectPC_For_NUM(int room_num)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where ROOM_NUM = " + room_num;

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    pc.pointer_name = rd[14].ToString();
                    pc.pc_state = rd[15].ToString();

                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("PC얻기 IP로부터. 성공");
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC얻기 ROOM_NUM로부터 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 서버에 있는 EXE 파일 이름 얻기
        public List<CFormat_File_Exe_Check_S> Select_Exe_FileName()
        {
            try
            {
                List<CFormat_File_Exe_Check_S> Exe_Check_S = new List<CFormat_File_Exe_Check_S>();

                string strSql = "SELECT * From FORMAT_FILE_EXE_CHECK_S";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CFormat_File_Exe_Check_S exe_check_s = new CFormat_File_Exe_Check_S();

                    exe_check_s.format_file_downloding_name_s = rd["FORMAT_FILE_EXE_NAME_S"].ToString();

                    Exe_Check_S.Add(exe_check_s);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("서버에 있는 EXE 파일 이름 얻기 성공");
                return Exe_Check_S;
            }
            catch (Exception e)
            {
                Console.WriteLine("서버에 있는 EXE 파일 이름 얻기 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 서버에 있는 IMAGE 파일 이름 얻기
        public List<CFormat_File_Image_Check_S> Select_Image_FileName()
        {
            try
            {
                List<CFormat_File_Image_Check_S> Image_Check_S = new List<CFormat_File_Image_Check_S>();

                string strSql = "SELECT * From FORMAT_FILE_IMAGE_CHECK_S";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CFormat_File_Image_Check_S image_check_s = new CFormat_File_Image_Check_S();

                    image_check_s.format_file_none_name_s = rd["FORMAT_FILE_IMAGE_NAME_S"].ToString();

                    Image_Check_S.Add(image_check_s);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("서버에 있는 IMAGE 파일 이름 얻기 성공");
                return Image_Check_S;
            }
            catch (Exception e)
            {
                Console.WriteLine("서버에 있는 IMAGE 파일 이름 얻기 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 서버에 있는 IMAGE 파일 이름 얻기(타입으로)
        public List<CFormat_File_Image_Check_S> Select_Image_FileName_For_Type(string type)
        {
            try
            {
                List<CFormat_File_Image_Check_S> Image_Check_S = new List<CFormat_File_Image_Check_S>();

                string strSql = "SELECT * From FORMAT_FILE_IMAGE_CHECK_S where FORMAT_FILE_IMAGE_TYPE_S = '" + type + "'";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CFormat_File_Image_Check_S image_check_s = new CFormat_File_Image_Check_S();

                    image_check_s.format_file_none_name_s = rd["FORMAT_FILE_IMAGE_NAME_S"].ToString();

                    Image_Check_S.Add(image_check_s);
                }
                rd.Close();
                conn.Close();
                //Console.WriteLine("서버에 있는 IMAGE 파일 이름 얻기(타입으로) 성공");
                return Image_Check_S;
            }
            catch (Exception e)
            {
                Console.WriteLine("서버에 있는 IMAGE 파일 이름 얻기(타입으로) 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 클라이언트 포맷 파일 얻기 IP로부터
        //public List<CFormat_File_Image_Check_C> SelectFormat_From_IP(string pc_Ip)
        //{
            //try
            //{
            //    List<CFormat_File_Image_Check_C> format_c = new List<CFormat_File_Image_Check_C>();

            //    string strSql = "SELECT * From FORMAT_FILE_IMAGE_CHECK_C where PC_IP = '" + pc_Ip + "'";

            //    SqlCommand cmd = new SqlCommand(strSql, conn);
            //    conn.Open();
            //    SqlDataReader rd = cmd.ExecuteReader();

            //    while (rd.Read())
            //    {
            //        CFormat_File_Image_Check_C sqlPosition = new CFormat_File_Image_Check_C();

            //        sqlPosition.pc_ip = rd["PC_IP"].ToString();
            //        sqlPosition.pc_type_c = rd["PC_TYPE"].ToString();
            //        sqlPosition.format_file_none_name_c = rd["FORMAT_FILE_IMAGE_NAME_C"].ToString();
            //        sqlPosition.format_file_none = int.Parse(rd["FORMAT_FILE_IMAGE"].ToString());


            //        format_c.Add(sqlPosition);
            //    }
            //    rd.Close();
            //    conn.Close();
            //    //Console.WriteLine("클라이언트 포맷 파일 얻기 IP로부터. 성공");
            //    return format_c;
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("클라이언트 포맷 파일 얻기 IP로부터. 실패 : " + e.Message);
            //    return null;
            //}
        //}
        #endregion

        #region 룸 정보 얻기 룸 넘버로부터
        public List<CRoom> SelectRoom_For_RoomNum(int room_Num)
        {
            try
            {
                string strSql = "SELECT * From ROOM where ROOM_NUM = " + room_Num.ToString();

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                //string str1 = string.Empty;
                List<CRoom> room = new List<CRoom>();
                while (rd.Read())
                {
                    CRoom sqlPosition = new CRoom();

                    sqlPosition.Floor_num = int.Parse(rd["FLOOR_NUM"].ToString());
                    sqlPosition.Room_name = rd["ROOM_NAME"].ToString();
                    sqlPosition.Room_num = int.Parse(rd["ROOM_NUM"].ToString());

                    room.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("룸 정보 얻기 룸 넘버로부터. 성공");
                return room;
            }
            catch (Exception e)
            {
                Console.WriteLine("룸 정보 얻기 룸 넘버로부터. 성공 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 맥주소 얻기 PC IP로 부터
        public string GetMac_For_IP(string pc_Ip)
        {
            try
            {
                string strSql = "SELECT * From CLIENT where PC_IP = '" + pc_Ip + "'";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                string pc_Mac_Address = string.Empty;

                while (rd.Read())
                    pc_Mac_Address = rd["MAC"].ToString();

                rd.Close();
                conn.Close();
                Console.WriteLine("맥주소 얻기 PC IP로 부터. 성공");
                return pc_Mac_Address;
            }
            catch (Exception e)
            {
                Console.WriteLine("맥주소 얻기 PC IP로 부터. 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region PC얻기 룸넘버로부터
        public List<CComputer> selectPC_For_RoomNum(int Room_Num)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where ROOM_NUM = " + Room_Num.ToString();

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CComputer pc = new CComputer();

                    pc.room_num = int.Parse(rd[0].ToString());
                    pc.pc_type_c = rd[1].ToString();
                    pc.pc_ip = rd[2].ToString();
                    pc.mac = rd[3].ToString();
                    pc.power = int.Parse(rd[4].ToString());
                    pc.coordinate = rd[5].ToString();
                    pc.software = rd[6].ToString();
                    pc.format_file_none = int.Parse(rd[7].ToString());
                    pc.format_file_downloding = int.Parse(rd[8].ToString());
                    pc.last_format = rd[9].ToString();
                    pc.windowaccount = int.Parse(rd[10].ToString());
                    pc.on_time = int.Parse(rd[11].ToString());
                    pc.off_time = int.Parse(rd[12].ToString());
                    pc.panel = rd[13].ToString();
                    pc.pointer_name = rd[14].ToString();
                    pc.pc_state = rd[15].ToString();
                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                // Console.WriteLine("PC얻기 룸넘버로부터. 성공");
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC얻기 룸넘버로부터. 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 층 코드 얻기 룸 코드로부터
        public int GetFloorCode_For_RoomCode(int room_Code)
        {
            string strSql = "SELECT * From ROOM_INFO where ROOM_CODE = " + room_Code.ToString();

            SqlCommand cmd = new SqlCommand(strSql, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            //string str1 = string.Empty;
            int codeNum = 0;
            while (rd.Read())
            {
                codeNum = int.Parse(rd["FLOOR_CODE"].ToString());
            }
            rd.Close();
            conn.Close();
            return codeNum;
        }
        #endregion

        #region 파일 전송 여부 확인
        public List<CNoneFilePc> File_Check_For_Type(string type)
        {
            string strSql = "SELECT * From NONEFILEPC where TYPE = '" + type.ToString() + "'";

            SqlCommand cmd = new SqlCommand(strSql, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            List<CNoneFilePc> nonfile = new List<CNoneFilePc>();

            while (rd.Read())
            {
                CNoneFilePc nonfilecheck = new CNoneFilePc();

                nonfilecheck.Room_Num = int.Parse(rd["ROOM_NUM"].ToString());
                nonfilecheck.pc_Ip = rd["PC_IP"].ToString();
                nonfilecheck.persent = rd["PERSENT"].ToString();
                nonfilecheck.complete = int.Parse(rd["COMPLETE"].ToString());
                nonfilecheck.connect = int.Parse(rd["CONNECT"].ToString());
                nonfilecheck.data_connect = rd["DATA_connect"].ToString();

                nonfile.Add(nonfilecheck);
            }
            rd.Close();
            conn.Close();
            return nonfile;
        }
        #endregion

        #region 각 강의실 총 PC 수 알아보기.
        public int GetPCCount_For_RoomNum(int room_Num)
        {
            try
            {
                int count = 0;

                string strSql = "SELECT * From CLIENT where ROOM_NUM = '" + room_Num + "'";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    count++;
                }

                rd.Close();
                conn.Close();
                Console.WriteLine("각 강의실 총 PC 수 알아보기. 성공");
                return count;
            }
            catch
            {
                Console.WriteLine("각 강의실 총 PC 수 알아보기. 실패");
                return 0;
            }
        }
        #endregion

        #region 각 강의실 총 PC 접속자 수 알아보기.
        public int GetPCAcceptCount_For_RoomNum(int room_Num)
        {
            try
            {
                int count = 0;

                string strSql = "SELECT * From CLIENT where ROOM_NUM = '" + room_Num + "' AND POWER = 1";

                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    count++;
                }

                rd.Close();
                conn.Close();
                Console.WriteLine("각 강의실 총 PC 접속자 수 알아보기. 성공");
                return count;
            }
            catch (Exception e)
            {
                Console.WriteLine("각 강의실 총 PC 접속자 수 알아보기. 실패 : " + e.Message);
                return 0;
            }
        }
        #endregion

        #region LOG_NUM 얻기 LOG이름으로부터
        public int GetLogType_For_TypeName(string log_TypeName)
        {
            string stl = "SELECT * FROM LOG_TYPE WHERE LOG_TYPE_NAME = '" + log_TypeName + "'";
            SqlCommand cmd = new SqlCommand(stl, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            int log_num = 0;
            while (rd.Read())
            {
                log_num = int.Parse(rd["LOG_NUM"].ToString());
            }
            rd.Close();
            conn.Close();
            return log_num;
        }
        #endregion

        #region LOG 얻기 LOG_NUM이름으로부터
        public List<CLog> GetLogType_For_TypeName(int type_num)
        {
            List<CLog> logall = new List<CLog>();
            string stl = "SELECT * FROM LOG WHERE LOG_NUM = " + type_num;
            SqlCommand cmd = new SqlCommand(stl, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                CLog sqlPosition = new CLog();

                sqlPosition.pc_ip = rd["PC_IP"].ToString();
                sqlPosition.log_name = rd["LOG_NAME"].ToString();
                sqlPosition.log_log = rd["LOG_LOG"].ToString();
                sqlPosition.log_detection_day = rd["LOG_DETECTION_DAY"].ToString();
                sqlPosition.log_detection_time = rd["LOG_DETECTION_TIME"].ToString();

                logall.Add(sqlPosition);
            }
            rd.Close();
            conn.Close();
            return logall;
        }
        #endregion

        #region 센터 서버 DB에 저장되어 있는 EXE 파일 얻기(사전에 다른 소스로 등록 되어 있음)
        public List<CFormat_File_Exe_Check_S> Client_EXE_File_Check(string pc_ip)
        {
            try
            {
                List<CFormat_File_Exe_Check_S> exe_check = new List<CFormat_File_Exe_Check_S>();
                string stl = "SELECT * FROM FORMAT_FILE_EXE_CHECK_S";
                SqlCommand cmd = new SqlCommand(stl, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CFormat_File_Exe_Check_S sqlPosition = new CFormat_File_Exe_Check_S();

                    sqlPosition.format_file_downloding_name_s = rd["FORMAT_FILE_EXE_NAME_S"].ToString();

                    exe_check.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("클라이언트 파일 체크 성공");
                return exe_check;
            }
            catch (Exception e)
            {
                Console.WriteLine("클라이언트 파일 체크. 실패 : " + e.Message);
                return null;
            }

        }
        #endregion

        #region 센터 서버 DB에 저장되어 있는 IMAGE 파일 얻기(사전에 다른 소스로 등록 되어 있음)
        public List<CFormat_File_Image_Check_S> Client_IMAGE_File_Check(string pc_ip, string image_type)
        {
            try
            {
                List<CFormat_File_Image_Check_S> exe_check = new List<CFormat_File_Image_Check_S>();
                string stl = "SELECT * FROM FORMAT_FILE_IMAGE_CHECK_S WHERE FORMAT_FILE_IMAGE_TYPE_S = '" + image_type.ToString() + "'";
                SqlCommand cmd = new SqlCommand(stl, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    CFormat_File_Image_Check_S sqlPosition = new CFormat_File_Image_Check_S();

                    sqlPosition.format_file_none_name_s = rd["FORMAT_FILE_IMAGE_NAME_S"].ToString();

                    exe_check.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("클라이언트 파일 체크 성공");
                return exe_check;
            }
            catch (Exception e)
            {
                Console.WriteLine("클라이언트 파일 체크. 실패 : " + e.Message);
                return null;
            }

        }
        #endregion

        #region 소프트웨어 리스트 검색
        public string soft_Init()
        {
            SOFTWARE s = new SOFTWARE();
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand("SELECT * From CLIENT_SOFTWARE_SET", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            List<SOFTWARE> user1 = new List<SOFTWARE>();

            while (rd.Read())
            {
                user1.Clear();
                user1.Add(new SOFTWARE() { SOFTWARENUM = int.Parse(rd["SOFTWARE_NUM"].ToString()), SOFTWARENAME = rd["SOFTWARE_NAME"].ToString() });
                str += user1[0].SOFTWARENUM + "#" + user1[0].SOFTWARENAME + "#";
            }

            rd.Close();
            conn.Close();

            return str;
        }

        public string soft_Init1(string pc_ip)
        {
            SOFTWARE s = new SOFTWARE();
            string query = "SELECT SOFTWARE_NAME From CLIENT_SOFTWARE_NAME WHERE PC_IP = '" + pc_ip + "'";
			string str = string.Empty;

			SqlCommand cmd = new SqlCommand(query, conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                str += rd["SOFTWARE_NAME"].ToString() +"☆";
			}

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion

        #region 소프트웨어 추가 후 리스트 검색
        public string soft_insert(string query)
        {
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.ConnectionString = constring1;
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();


            cmd = new SqlCommand("SELECT * From CLIENT_SOFTWARE_SET", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            user = new List<SOFTWARE>();
            user.Clear();

            while (rd.Read())
            {
                user.Clear();
                user.Add(new SOFTWARE() { SOFTWARENUM = int.Parse(rd["SOFTWARE_NUM"].ToString()), SOFTWARENAME = rd["SOFTWARE_NAME"].ToString() });
                str += user[0].SOFTWARENUM + "#" + user[0].SOFTWARENAME + "#";

            }

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion

        #region 소프트웨어 삭제 후 리스트 검색
        public string soft_Delete(string query)
        {
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.ConnectionString = constring1;
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            cmd = new SqlCommand("SELECT * From CLIENT_SOFTWARE_SET", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            user = new List<SOFTWARE>();
            user.Clear();

            while (rd.Read())
            {
                user.Clear();
                user.Add(new SOFTWARE() { SOFTWARENUM = int.Parse(rd["SOFTWARE_NUM"].ToString()), SOFTWARENAME = rd["SOFTWARE_NAME"].ToString() });
                str += user[0].SOFTWARENUM + "#" + user[0].SOFTWARENAME + "#";

            }

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion

        #region 윈도우 리스트 검색
        public string window_Init()
        {
            WINDOWACCOUNTS s = new WINDOWACCOUNTS();
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand("SELECT * From CLIENT_WINDOWACCOUNTS_NAME", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            winduser = new List<WINDOWACCOUNTS>();
            winduser.Clear();

            while (rd.Read())
            {
                winduser.Clear();
                winduser.Add(new WINDOWACCOUNTS() { idx = int.Parse(rd["idx"].ToString()), account = rd["ACCOUNT"].ToString(), password = rd["PASSWORD"].ToString() });
                str += winduser[0].idx + "#" + winduser[0].account + "#" + winduser[0].password + "#";

            }

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion

        #region 윈도우 추가 후 리스트 검색
        public string window_insert(string query)
        {
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.ConnectionString = constring1;
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();


            cmd = new SqlCommand("SELECT * From CLIENT_WINDOWACCOUNTS_NAME", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            winduser = new List<WINDOWACCOUNTS>();
            winduser.Clear();

            while (rd.Read())
            {
                winduser.Clear();
                winduser.Add(new WINDOWACCOUNTS() { idx = int.Parse(rd["idx"].ToString()), account = rd["ACCOUNT"].ToString(), password = rd["PASSWORD"].ToString() });
                str += winduser[0].idx + "#" + winduser[0].account + "#" + winduser[0].password + "#";

            }

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion

        #region 윈도우 삭제 후 리스트 검색
        public string window_Delete(string query)
        {
            string str = string.Empty;
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.ConnectionString = constring1;
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            cmd = new SqlCommand("SELECT * From CLIENT_WINDOWACCOUNTS_NAME", conn);
            conn.ConnectionString = constring1;
            conn.Open();

            SqlDataReader rd = cmd.ExecuteReader();
            winduser = new List<WINDOWACCOUNTS>();
            winduser.Clear();

            while (rd.Read())
            {
                winduser.Clear();
                winduser.Add(new WINDOWACCOUNTS() { idx = int.Parse(rd["idx"].ToString()), account = rd["ACCOUNT"].ToString(), password = rd["PASSWORD"].ToString() });
                str += winduser[0].idx + "#" + winduser[0].account + "#" + winduser[0].password + "#";

            }

            rd.Close();
            conn.Close();

            return str;
        }
        #endregion
  
        #region 소프트웨어 사용량 불러오기(IP,NAME)(===============창근변경==================)
        public string select_software_use(string ip, string name)
        {
            try
            {
                string stl = "SELECT * FROM CLIENT_SOFTWARE_USE WHERE PC_IP = '" + ip + "' AND SOFTWARE_NAME = '" + name + "'";
                SqlCommand cmd = new SqlCommand(stl, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                string software_use = string.Empty;

                while (rd.Read())
                {
                    software_use = rd["SOFTWARE_USE"].ToString();
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("소프트웨어 사용량 불러오기(IP,NAME) 성공");
                return software_use;
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용량 불러오기(IP,NAME) 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 소프트웨어 사용량 불러오기(IP)(===============창근변경==================)
        public List<File_use> select_software_use_ip(string ip)
        {
            try
            {
                List<File_use> use = new List<File_use>();
                string stl = "SELECT * FROM CLIENT_SOFTWARE_USE WHERE PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(stl, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                string software = string.Empty;

                while (rd.Read())
                {
                    File_use sqlPosition = new File_use();

                    sqlPosition.file_name = rd["SOFTWARE_NAME"].ToString();
                    sqlPosition.file_time = rd["SOFTWARE_USE"].ToString();

                    use.Add(sqlPosition);
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("소프트웨어 사용량 불러오기(IP) 성공");
                return use;
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용량 불러오기(IP) 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 소프트웨어 사용 시간 체크 이름 가져오기(===============창근변경==================)
        public string select_software_use_check(string ip)
        {
            try
            {
                string stl = "SELECT * FROM CLIENT_SOFTWARE_USE WHERE PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(stl, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                string software_name = string.Empty;
                string str = string.Empty;

                while (rd.Read())
                {
                    software_name = rd["SOFTWARE_NAME"].ToString();
                    str += software_name + "#";
                }
                rd.Close();
                conn.Close();
                Console.WriteLine("소프트웨어 사용 시간 체크 이름 가져오기 성공");
                return str;
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용 시간 체크 이름 가져오기 실패 : " + e.Message);
                return null;
            }
        }
        #endregion

        #region 날짜별 알람켜기 전체검색
        public List<On_Date> Select_Ondate()
        {

            try
            {
                List<On_Date> on_Date = new List<On_Date>();
                string str = "SELECT * From CLIENT_ON_TIME_ALARM_CHECK WHERE ALARM_ON_DAYS=0 ORDER BY ALARM_ON_YEAR ASC, ALARM_ON_MONTH ASC,  ALARM_ON_DAY ASC, ALARM_AM_PM ASC, ALARM_ON_TIME ASC";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    On_Date on_date = new On_Date();
                    on_date.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    on_date.ALARM_On_year = rd["ALARM_ON_YEAR"].ToString();
                    on_date.ALARM_On_month = rd["ALARM_ON_MONTH"].ToString();
                    on_date.ALARM_On_day = rd["ALARM_ON_DAY"].ToString();
                    on_date.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    on_date.ALARM_On_time = rd["ALARM_ON_TIME"].ToString();

                    on_Date.Add(on_date);
                }
                rd.Close();
                conn.Close();
                return on_Date;

            }
            catch
            {
                List<On_Date> on_Date2 = new List<On_Date>();
                conn.Close();
                return on_Date2;
            }



        }
        #endregion

        #region 날짜별 알람끄기 전체검색
        public List<Off_Date> Select_Offdate()
        {


            try
            {
                List<Off_Date> Off_Date = new List<Off_Date>();
                string str = "SELECT * From CLIENT_OFF_TIME_ALARM_CHECK WHERE ALARM_OFF_DAYS=0 ORDER BY ALARM_OFF_YEAR ASC, ALARM_OFF_MONTH ASC,  ALARM_OFF_DAY ASC, ALARM_AM_PM ASC, ALARM_OFF_TIME ASC";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    Off_Date off_date = new Off_Date();
                    off_date.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    off_date.ALARM_Off_year = rd["ALARM_OFF_YEAR"].ToString();
                    off_date.ALARM_Off_month = rd["ALARM_OFF_MONTH"].ToString();
                    off_date.ALARM_Off_day = rd["ALARM_OFF_DAY"].ToString();
                    off_date.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    off_date.ALARM_Off_time = rd["ALARM_OFF_TIME"].ToString();


                    Off_Date.Add(off_date);
                }
                rd.Close();
                conn.Close();
                return Off_Date;
            }
            catch
            {
                List<Off_Date> off_Date2 = new List<Off_Date>();
                conn.Close();
                return off_Date2;
            }

        }
        #endregion

        #region 요일별 알람켜기 전체검색(전체삭제할때 사용)
        public List<On_Days> Select_All_Ondays()
        {
            try
            {
                List<On_Days> On_Days = new List<On_Days>();
                string str = string.Format("SELECT* From CLIENT_ON_TIME_ALARM_CHECK where ALARM_ON_DAYS >= 1 ORDER BY ALARM_ON_DAYS ASC, ALARM_AM_PM ASC, ALARM_ON_TIME ASC");
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    On_Days on_days = new On_Days();
                    on_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    on_days.ALARM_On_days = int.Parse(rd["ALARM_ON_DAYS"].ToString());
                    on_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    on_days.ALARM_On_time = rd["ALARM_ON_TIME"].ToString();

                    On_Days.Add(on_days);
                }
                rd.Close();
                conn.Close();
                return On_Days;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                List<On_Days> On_Days2 = new List<On_Days>();
                return On_Days2;
            }
        }
        #endregion

        #region 요일별 알람끄기 전체검색(전체삭제할때 사용)
        public List<Off_Days> Select_All_Offdays()
        {
            try
            {
                List<Off_Days> Off_Days = new List<Off_Days>();
                string str = string.Format("SELECT* From CLIENT_OFF_TIME_ALARM_CHECK where ALARM_OFF_DAYS >= 1 ORDER BY ALARM_OFF_DAYS ASC, ALARM_AM_PM ASC, ALARM_OFF_TIME ASC");
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    Off_Days off_days = new Off_Days();
                    off_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    off_days.ALARM_Off_days = int.Parse(rd["ALARM_OFF_DAYS"].ToString());
                    off_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    off_days.ALARM_Off_time = rd["ALARM_OFF_TIME"].ToString();

                    Off_Days.Add(off_days);
                }
                rd.Close();
                conn.Close();
                return Off_Days;
            }
            catch (Exception e)
            {
                List<Off_Days> Off_Days2 = new List<Off_Days>();
                conn.Close();
                return Off_Days2;
            }

        }
        #endregion

        #region 요일별 알람켜기(현재 요일,시간 또는 이후의 시간만 불러오기)
        public List<On_Days> Select_Ondays(int days, int am_pm, int time)
        {
            try
            {
                List<On_Days> On_Days = new List<On_Days>();
                string str = string.Format("SELECT* From CLIENT_ON_TIME_ALARM_CHECK where ALARM_ON_DAYS={0} AND ALARM_AM_PM >={1} AND ALARM_ON_TEST_TIME >={2}  ORDER BY ALARM_ON_DAYS ASC, ALARM_AM_PM ASC, ALARM_ON_TIME ASC", days, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    On_Days on_days = new On_Days();
                    on_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    on_days.ALARM_On_days = int.Parse(rd["ALARM_ON_DAYS"].ToString());
                    on_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    on_days.ALARM_On_time = rd["ALARM_ON_TIME"].ToString();

                    On_Days.Add(on_days);
                }
                rd.Close();
                conn.Close();
                return On_Days;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                List<On_Days> On_Days2 = new List<On_Days>();
                return On_Days2;
            }
        }
        #endregion

        #region 요일별 끄기알람(현재 요일,시간 또는 이후의 시간만 불러오기)
        public List<Off_Days> Select_Offdays(int days, int am_pm, int time)
        {
            try
            {
                List<Off_Days> Off_Days = new List<Off_Days>();
                string str = string.Format("SELECT * From CLIENT_OFF_TIME_ALARM_CHECK WHERE ALARM_OFF_DAYS={0} AND ALARM_AM_PM >={1} AND ALARM_OFF_TEST_TIME >={2} ORDER BY ALARM_OFF_DAYS ASC, ALARM_AM_PM ASC, ALARM_OFF_TIME ASC", days, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    Off_Days off_days = new Off_Days();
                    off_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                    off_days.ALARM_Off_days = int.Parse(rd["ALARM_OFF_DAYS"].ToString());
                    off_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                    off_days.ALARM_Off_time = rd["ALARM_OFF_TIME"].ToString();

                    Off_Days.Add(off_days);
                }
                rd.Close();
                conn.Close();
                return Off_Days;
            }
            catch (Exception e)
            {
                List<Off_Days> Off_Days2 = new List<Off_Days>();
                conn.Close();
                return Off_Days2;
            }

        }
        #endregion

        #region 켜기알람(같은 조건) 모두 불러오기 //ㅅㅁ(수정)
        public List<Ip_on> Select_All_On(string year, string month, string day, int days, int am_pm, string time)
        {
            List<Ip_on> Ip_On = new List<Ip_on>();
            string str = string.Format("SELECT * From CLIENT_ON_TIME_ALARM_CHECK where ALARM_ON_YEAR='{0}' and  ALARM_ON_MONTH='{1}' and  ALARM_ON_DAY='{2}'and  ALARM_ON_DAYS={3} and ALARM_AM_PM={4} and  ALARM_ON_TIME='{5}'", year, month, day, days, am_pm, time);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            try
            {
                while (rd.Read())
                {
                    Ip_on ip_on = new Ip_on();
                    ip_on.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)

                    Ip_On.Add(ip_on);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            rd.Close();
            conn.Close();
            return Ip_On;
        }
        #endregion

        #region 끄기알람(같은 조건) 모두 불러오기(thread에서 사용)//ㅅㅁ(수정)
        public List<Ip_off> Select_All_OFF(string year, string month, string day, int days, int am_pm, string time)
        {
            List<Ip_off> Ip_Off = new List<Ip_off>();
            string str = string.Format("SELECT PC_IP From CLIENT_OFF_TIME_ALARM_CHECK where ALARM_OFF_YEAR='{0}'and ALARM_OFF_MONTH='{1}'and ALARM_OFF_DAY='{2}'and ALARM_OFF_DAYS={3} and ALARM_AM_PM={4}and ALARM_OFF_TIME='{5}'", year, month, day, days, am_pm, time);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            try
            {
                while (rd.Read())
                {
                    Ip_off ip_off = new Ip_off();
                    ip_off.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)

                    Ip_Off.Add(ip_off);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            rd.Close();
            conn.Close();
            return Ip_Off;
        }
        #endregion

        #region 날짜별 켜기알람 ip로 검색(해당타이머 삭제할때 사용)
        public string Select_Ondate_delete(string ip)
        {

            string msg = string.Empty;
            List<On_Date> on_Date = new List<On_Date>();
            string str = string.Format("SELECT * From CLIENT_ON_TIME_ALARM_CHECK WHERE PC_IP='{0}' and ALARM_ON_DAYS = {1} ", ip, 0);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                On_Date on_date = new On_Date();
                on_date.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                on_date.ALARM_On_year = rd["ALARM_ON_YEAR"].ToString();
                on_date.ALARM_On_month = rd["ALARM_ON_MONTH"].ToString();
                on_date.ALARM_On_day = rd["ALARM_ON_DAY"].ToString();
                on_date.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                on_date.ALARM_On_time = rd["ALARM_ON_TIME"].ToString();

                on_Date.Add(on_date);
            }
            for (int i = 0; i < on_Date.Count; i++)
            {
                msg += "date" + "#";
                msg += on_Date[i].ALARM_On_year + "-" + on_Date[i].ALARM_On_month + "-" + on_Date[i].ALARM_On_day + "#";
                msg += on_Date[i].ALARM_AM_PM + "#";
                msg += on_Date[i].ALARM_On_time + "#";
            }

            rd.Close();
            conn.Close();
            return msg;
        }
        #endregion

        #region 날짜별 끄기알람 ip로 검색(해당타이머 삭제할때 사용)
        public string Select_Offdate_delete(string ip)
        {

            string msg = string.Empty;
            List<Off_Date> off_Date = new List<Off_Date>();
            string str = string.Format("SELECT * From CLIENT_OFF_TIME_ALARM_CHECK WHERE PC_IP='{0}'  and ALARM_OFF_DAYS = 0 ", ip);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                Off_Date off_date = new Off_Date();
                off_date.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                off_date.ALARM_Off_year = rd["ALARM_OFF_YEAR"].ToString();
                off_date.ALARM_Off_month = rd["ALARM_OFF_MONTH"].ToString();
                off_date.ALARM_Off_day = rd["ALARM_OFF_DAY"].ToString();
                off_date.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                off_date.ALARM_Off_time = rd["ALARM_OFF_TIME"].ToString();

                off_Date.Add(off_date);
            }
            for (int i = 0; i < off_Date.Count; i++)
            {
                //   msg += off_Date[i].PC_IP;
                msg += "date" + "#";
                msg += off_Date[i].ALARM_Off_year + "-" + off_Date[i].ALARM_Off_month + "-" + off_Date[i].ALARM_Off_day + "#";
                msg += off_Date[i].ALARM_AM_PM + "#";
                msg += off_Date[i].ALARM_Off_time + "#";
            }
            rd.Close();
            conn.Close();
            return msg;
        }
        #endregion

        #region 요일별 켜기알람 ip로 검색(해당타이머 삭제할때 사용)
        public string Select_Ondays_delete(string ip)
        {

            string msg = string.Empty;
            List<On_Days> on_Days = new List<On_Days>();
            string str = string.Format("SELECT * From CLIENT_ON_TIME_ALARM_CHECK WHERE PC_IP='{0}'  and ALARM_ON_DAYS >= 1  ", ip);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                On_Days on_days = new On_Days();
                on_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                on_days.ALARM_On_days = int.Parse(rd["ALARM_ON_DAYS"].ToString());
                on_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                on_days.ALARM_On_time = rd["ALARM_ON_TIME"].ToString();

                on_Days.Add(on_days);
            }
            for (int i = 0; i < on_Days.Count; i++)
            {
                msg += "days" + "#";
                msg += on_Days[i].ALARM_On_days + "#";
                msg += on_Days[i].ALARM_AM_PM + "#";
                msg += on_Days[i].ALARM_On_time + "#";

            }
            rd.Close();
            conn.Close();
            return msg;

        }
        #endregion

        #region 요일별 끄기알람 ip로 검색(해당타이머 삭제할때 사용)
        public string Select_Offdays_delete(string ip)
        {

            //try
            //{
            string msg = string.Empty;
            List<Off_Days> off_Days = new List<Off_Days>();
            string str = string.Format("SELECT * From CLIENT_OFF_TIME_ALARM_CHECK WHERE PC_IP='{0}' and ALARM_OFF_DAYS >= 1", ip);
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            while (rd.Read())
            {
                Off_Days off_days = new Off_Days();

                off_days.PC_IP = rd["PC_IP"].ToString();   // PC_IP 칼럼은 varchar(50)
                off_days.ALARM_Off_days = int.Parse(rd["ALARM_OFF_DAYS"].ToString());
                off_days.ALARM_AM_PM = int.Parse(rd["ALARM_AM_PM"].ToString());
                off_days.ALARM_Off_time = rd["ALARM_OFF_TIME"].ToString();

                off_Days.Add(off_days);
            }
            for (int i = 0; i < off_Days.Count; i++)
            {
                //  msg += off_Days[i].PC_IP + "#";
                msg += "days" + "#";
                msg += off_Days[i].ALARM_Off_days + "#";
                msg += off_Days[i].ALARM_AM_PM + "#";
                msg += off_Days[i].ALARM_Off_time + "#";
            }
            rd.Close();
            conn.Close();
            return msg;

            //}
            //catch
            //{
            //    List<Off_Days> off_Days2 = new List<Off_Days>();
            //    conn.Close();
            //    return off_Days2;
            //{
        }
        #endregion

		#region PC 포맷 상태값 얻기
		public List<CComputer> Get_Format_State(string pc_ip)
		{
			string msg = string.Empty;
			string query = string.Format("SELECT * FROM CLIENT WHERE PC_IP = '{0}'", pc_ip);
			SqlCommand cmd = new SqlCommand(query, conn);
			conn.Open();
			SqlDataReader rd = cmd.ExecuteReader();

			List<CComputer> com = new List<CComputer>();

			while (rd.Read())
			{
				CComputer cm = new CComputer();

				cm.format_state = rd["FORMAT_STATE"].ToString();

				com.Add(cm);
			}

			rd.Close();
			conn.Close();
			return com;
		}
		#endregion
      
        //update문========================================================================

        #region 층 서버 ON/OFF
        //Floor층 on/off
        public void update_Floor_Power(string floor_Ip, int floor_Power)
        {
            try
            {
                string str = "update FLOOR_SERVER set FLOOR_POWER = " + floor_Power.ToString() + " where FLOOR_IP = '" + floor_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("층 서버 ON/OFF. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 서버 ON/OFF. 실패 : " + e.Message);
            }
        }
        #endregion

        #region 층 서버 이름 변경
        //Floor층 on/off
        public void update_Floor_Name(string floor_num, string after_Name)
        {
            try
            {
                string str = "update FLOOR_SERVER set FLOOR_NAME = '" + after_Name + "' where FLOOR_NUM = " + floor_num;
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("층 서버 이름 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 서버 이름 변경 실패 : " + e.Message);

            }
        }
        #endregion

        #region 강의실 호수 변경
        public void update_Room_Num(string room_num, string after_num)
        {
            try
            {
                string str = "update ROOM set ROOM_NUM = " + after_num + " where ROOM_NUM = " + room_num;
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("강의실 호수 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 호수 변경 실패 : " + e.Message);

            }
        }
        #endregion

        #region 강의실 이름 변경
        public void update_Room_Name(string room_num, string after_Name)
        {
            try
            {
                string str = "update ROOM set ROOM_NAME = '" + after_Name + "' where ROOM_NUM = " + room_num;
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("강의실 이름 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("강의실 이름 변경 실패 : " + e.Message);

            }
        }
        #endregion

        #region PC 수정 POWER ON/OFF
        public void Update_Power_Pc(string pc_Ip, int pc_Power_State)
        {
            try
            {
                string str = "update CLIENT set POWER = " + pc_Power_State + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 POWER ON/OFF 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 POWER ON/OFF 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 MAC 주소 변경
        public void Update_Mac_Pc(string pc_Ip, string pc_Mac)
        {
            try
            {
                string str = "update CLIENT set MAC = '" + pc_Mac + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PC 수정 MAC 주소 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 MAC 주소 변경 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 TYPE 변경
        public void Update_Type_Pc(string pc_Ip, string pc_Type)
        {
            try
            {
                string str = "update CLIENT set PC_TYPE = '" + pc_Type + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 TYPE 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 TYPE 변경 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 PANEL 변경
        public void Update_Panel_Pc(string pc_Ip, string pc_Panel)
        {
            try
            {
                string str = "update CLIENT set PC_PANEL = '" + pc_Panel + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 PANEL 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 PANEL 변경 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 SOFTWARE 변경
        public void Update_Software_Pc(string pc_Ip, string pc_software)
        {
            try
            {
                string str = "update CLIENT set SOFTWARE = '" + pc_software + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 SOFTWARE 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 SOFTWARE 변경 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 ROOM_NUM 변경(창근변경2)
        public void Update_Num_Pc(string pc_Ip, int room_Num)
        {
            try
            {
                string str = "update CLIENT set ROOM_NUM = " + room_Num + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 ROOM_NUM 변경 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 ROOM_NUM 변경 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 SOFTWARE 체크 확인
        public void Update_pc_software_check(string pc_Ip)
        {
            try
            {
                string str = "update CLIENT set SOFTWARE = 1 where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 SOFTWARE 체크 확인 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 SOFTWARE 체크 확인 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 수정 HARDWARE 체크 확인
        public void Update_pc_hardware_check(string pc_Ip)
        {
            try
            {
                string str = "update CLIENT set HARDWARE = 1 where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 수정 HARDWARE 체크 확인 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 수정 HARDWARE 체크 확인 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC EXE 파일 체크 확인
        public void Update_pc_exe_check(string pc_Ip, int image)
        {
            try
            {
                string str = "update CLIENT set FORMAT_FILE_EXE = " + image + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC EXE 파일 체크 확인 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC EXE 파일 체크 확인 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 이미지 파일 체크 확인
        public void Update_pc_image_check(string pc_Ip, int image)
        {
            try
            {
                string str = "update CLIENT set FORMAT_FILE_IMAGE = " + image + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 이미지 파일 체크 확인 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 이미지 파일 체크 확인 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC WINDOWACCOUNT 체크 확인
        public void Update_pc_WINDOWACCOUNT_check(string pc_Ip)
        {
            try
            {
                string str = "update CLIENT set WINDOWACCOUNT = 1 where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC WINDOWACCOUNT 체크 확인 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC WINDOWACCOUNT 체크 확인 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 마지막 백업 시간
        public void Update_pc_LAST_FORMAT(string pc_Ip, string time)
        {
            try
            {
                string str = "update CLIENT set LAST_FORMAT = '" + time + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 마지막 백업 시간 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 마지막 백업 시간 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 파일 유무 수정 이상해
        public void update_File_Pc(string pc_Ip, string pc_Exe_File_name, string pc_Image_File_name)
        {
            try
            {
                string str = "update FORMAT_FILE_CHECK_C set FORMAT_FILE_EXE_NAME_C = " + pc_Exe_File_name + ", " + " FORMAT_FILE_IMAGE_NAME_C = " + pc_Image_File_name + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 파일 유무 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 파일 유무 수정 실패 : " + e.Message);

            }
        }
        #endregion

        #region PC EXE 파일 유무 수정(type)
        public void update_Exe_File_Pc_Type(string pc_type, string pc_Exe_File_name)
        {
            try
            {
                string str = "update FORMAT_FILE_EXE_CHECK_C set FORMAT_FILE_EXE_NAME_C = '" + pc_Exe_File_name + "' where PC_TYPE = '" + pc_type + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC EXE 파일 유무 수정(type) 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC EXE 파일 유무 수정(type) 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC IMAGE 파일 유무 수정(type)
        public void update_Image_File_Pc_Type(string pc_type, string pc_Image_File_name)
        {
            try
            {
                string str = "update FORMAT_FILE_IMAGE_CHECK_C set FORMAT_FILE_IMAGE_NAME_C = '" + pc_Image_File_name + "' where PC_TYPE = '" + pc_type + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC IMAGE 파일 유무 수정(type) 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC IMAGE 파일 유무 수정(type) 수정 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC EXE 파일 유무 수정(ip)
        public void update_ExeFile_Pc(string pc_Ip, string pc_Exe_File_name)
        {
            try
            {
                string str = "update FORMAT_FILE_EXE_CHECK_C set FORMAT_FILE_EXE_NAME_C = '" + pc_Exe_File_name.ToString() + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC EXE 파일 유무 수정(ip) 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC EXE 파일 유무 수정(ip) 실패 : " + e.Message);

            }
        }
        #endregion

        #region PC IMAGE 파일 유무 수정(ip)
        public void update_ImageFile_Pc(string pc_Ip, string pc_Image_File_name)
        {
            try
            {
                string str = "update FORMAT_FILE_IMAGE_CHECK_C set FORMAT_FILE_IMAGE_NAME_C = '" + pc_Image_File_name.ToString() + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC IMAGE 파일 유무 수정(ip) 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC IMAGE 파일 유무 수정 실패(ip) : " + e.Message);

            }
        }
        #endregion

        #region PC 좌표 수정
        public void update_Coordinate_Pc(string pc_Ip, string pc_Coordinate)
        {
            try
            {
                string str = "update CLIENT set COORDINATE = '" + pc_Coordinate + "'" + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PC 좌표 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 좌표 수정 실패 : " + e.Message);
            }
        }
        #endregion

		#region PC 포맷 상태 변경
		public void Update_Format_Pc(int format_state, string pc_ip)
		{
			try
			{
				string str = string.Format("UPDATE CLIENT SET FORMAT_STATE = {0} WHERE PC_IP = '{1}'", format_state, pc_ip);

				SqlCommand cmd = new SqlCommand(str, conn);
				conn.Open();
				cmd.ExecuteNonQuery();
				Console.WriteLine("PC 포맷 상태 변경 성공");

				conn.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("PC 포맷 상태 변경 실패 : " + e.Message);
			}
		}
		#endregion

		#region PC 포맷 키값 상태 변경
		public void Update_Key_Format_Pc(string key_state, string pc_ip)
		{
			try
			{
				string str = string.Format("UPDATE CLIENT SET KEY_STATE = '{0}' WHERE PC_IP = '{1}'", key_state, pc_ip);

				SqlCommand cmd = new SqlCommand(str, conn);
				conn.Open();
				cmd.ExecuteNonQuery();
				conn.Close();
				Console.WriteLine("PC 포맷 키값 상태 변경 성공");
			}
			catch (Exception e)
			{
				Console.WriteLine("PC 포맷 키값 상태 변경 실패 : " + e.Message);
			}
		}
		#endregion

        #region PC 데이터 수정
        //Data_Connect 0이면 쓰지 않음, 1이면 서버, 2면 클라, 3번이면 CenterServer에서 다운. : IP
        public void update_Connect(string pc_Ip, string Data_Connect)
        {
            try
            {
                string str = "update NONEFILEPC set DATA_CONNECT = '" + Data_Connect + "' where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 데이터 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 데이터 수정 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 데이터 연결상태 수정
        //Data_Connect 0이면 쓰지 않음, 1이면 서버, 2면 클라, 3번이면 CenterServer에서 다운. : IP
        public void update_Connectstate(string pc_Ip, int Connect)
        {
            try
            {
                string str = "update NONEFILEPC set CONNECT = " + Connect + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 데이터 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 데이터 수정 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 데이터 전송 완료
        //Data_Connect 0이면 쓰지 않음, 1이면 서버, 2면 클라, 3번이면 CenterServer에서 다운. : IP
        public void update_Complete(string pc_Ip, int complete)
        {
            try
            {
                string str = "update NONEFILEPC set COMPLETE = " + complete + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 데이터 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 데이터 수정 실패 : " + e.Message);
            }
        }
        #endregion

        #region 전송 퍼센트 수정.
        public void Update_PC_Percent(string ip, string downlord_Percent)
        {
            try
            {
                string str = "update NONEFILEPC set PERSENT = '" + downlord_Percent + "' where PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("전송 퍼센트 수정 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("전송 퍼센트 수정 실패 : " + e.Message);

            }
        }
        #endregion

        #region 소프트웨어 점검
        public void clinet_Ipsate(string pfilename, string pstate, string pip)
        {
            int stater = 0;
            CMyDB db = new CMyDB();
            db.ConnectDB();
            stater = (pstate == "True") ? 1 : 0;
            //TODO :중복된것도 처리해야됨 UPdate개념.....
            string str = "UPDATE CLIENT_SOFTWARE_CHECK SET SOFTWARE_NAME = '" + pfilename + "' , SOFTWARE_OFFER = '" + stater + "' where PC_IP = '" + pip + "'";
            SqlCommand cmd = new SqlCommand(str, conn);
            conn.ConnectionString = constring1;
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        #endregion

        #region 소프트웨어 사용 시간 (===============창근변경==================)
        public void update_use_software(string ip, string soft_name, string time)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                //TODO :중복된것도 처리해야됨 UPdate개념.....
                string str = "UPDATE CLIENT_SOFTWARE_USE SET SOFTWARE_USE = '" + time + "' where SOFTWARE_NAME = '" + soft_name + "' AND PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                Console.WriteLine("소프트웨어 사용 시간 성공 ");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용 시간 실패 : " + e.Message);

            }
        }
        #endregion

        #region PC 알람 ON_TIME 카운트//ㅅㅁ(수정)
        public void Update_pc_ON_TIME_check(string pc_Ip, int on_time_count)
        {
            try
            {
                string str = "update CLIENT set ON_TIME = " + on_time_count + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("[DB]PC 알람 ON_TIME 카운트 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 알람 ON_TIME 카운트 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 알람 OFF_TIME 카운트//ㅅㅁ(수정)
        public void Update_pc_OFF_TIME_check(string pc_Ip, int off_time_count)
        {
            try
            {
                string str = "update CLIENT set OFF_TIME = " + off_time_count + " where PC_IP = '" + pc_Ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("PC 알람 OFF_TIME 카운트 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine("PC 알람 OFF_TIME 카운트 실패 : " + e.Message);
            }
        }
        #endregion

        #region 윈도우 계정 업데이트
        public void Window_update(string pip, string paccount, string ppw, string stater)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = "UPDATE CLIENT_WINDOWACCOUNTS_CHECK SET STATE = '" + stater + "', TIME = '" + DateTime.Now.ToString("HH::mm::ss") + "' WHERE PC_IP = '" + pip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("윈도우 계정 업데이트");
            }
            catch (Exception e)
            {
                Console.WriteLine("윈도우 계정 업데이트 오류 : " + e.Message);
            }
        }
        #endregion

		#region PC 포맷 상태 변경
		public void Update_Format_Pc(string format_state, string pc_ip)
		{
			try
			{
				string str = string.Format("UPDATE CLIENT SET FORMAT_STATE = '{0}' WHERE PC_IP = '{1}'", format_state, pc_ip);

				SqlCommand cmd = new SqlCommand(str, conn);
				conn.Open();
				cmd.ExecuteNonQuery();
				conn.Close();
				Console.WriteLine("PC 포맷 상태 변경 성공");
			}
			catch (Exception e)
			{
				Console.WriteLine("PC 포맷 상태 변경 실패 : " + e.Message);
			}
		}
		#endregion

        //delete문========================================================================     

        #region pc 삭제 룸 넘버로 부터
        public void DeletePc_For_RoomNum(int room_Num)
        {
            try
            {
                string str = string.Format("delete from CLIENT where ROOM_NUM = {0}", room_Num);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" pc 삭제 룸 넘버로 부터. 성공");
            }
            catch
            {
                Console.WriteLine(" pc 삭제 룸 넘버로 부터. 실패");
            }
        }
        #endregion

        #region 강의실 삭제 층 넘버로 부터
        //강의실 삭제
        public void DeleteRoom_For_FloorNum(string floor_Num)
        {
            try
            {
                string str = "delete from ROOM where FLOOR_NUM = " + floor_Num;
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 강의실 삭제 층 넘버로 부터. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 강의실 삭제 층 넘버로 부터. 실패 : " + e.Message);
            }
        }
        #endregion

        #region 층 삭제 층 이름으로부터
        public void Deletefloor_For_FloorName(string floor_Name)
        {
            try
            {
                string str = "delete from FLOOR_SERVER where FLOOR_NAME = '" + floor_Name + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 층 삭제 층 이름으로부터. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 층 삭제 층 이름으로부터. 실패 : " + e.Message);
            }
        }
        #endregion

        #region 층 삭제 층 넘버로 부터
        public void Deletefloor_For_FloorNum(string floor_Num)
        {
            try
            {
                string str = "delete from FLOOR_SERVER where FLOOR_NUM = " + floor_Num;
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 층 삭제 층 넘버로 부터. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 층 삭제 층 넘버로 부터. 실패 : " + e.Message);
            }
        }
        #endregion

        #region 강의실 삭제.
        public void DeleteRoom(string room_Num, string room_Name)
        {
            try
            {
                string str = "delete from ROOM where ROOM_NUM = " + room_Num + " AND ROOM_NAME = '" + room_Name + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine("강의실 삭제 성공");
            }
            catch
            {
                Console.WriteLine("강의실 삭제 실패");
            }
        }
        #endregion

        #region PC 삭제
        public void deletePc(string ip)
        {
            try
            {
                string str = "delete from CLIENT where PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PC 삭제. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" PC 삭제. 실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 삭제
        public void delete_nonefilepc(string ip)
        {
            try
            {
                string str = "delete from NONEFILEPC where PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PC 삭제. 성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" PC 삭제. 실패 : " + e.Message);
            }
        }
        #endregion

        #region 소프트웨어 사용량 이름 삭제(===============창근변경==================)
        public void delete_software_use_name(string soft_name, string ip)
        {
            try
            {
                //TODO :중복된것도 처리해야됨 UPdate개념.....
                string str = "DELETE FROM CLIENT_SOFTWARE_USE WHERE SOFTWARE_NAME = '" + soft_name + "' AND PC_IP = '" + ip + "'";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                Console.WriteLine("소프트웨어 사용량 이름 삭제 성공 ");
            }
            catch (Exception e)
            {
                Console.WriteLine("소프트웨어 사용량 이름 삭제 실패 : " + e.Message);

            }
        }
        #endregion

        #region 해당 알람 삭제(ip)
        public void delete_Pc_On_date(string ip, string year, string month, string day, int am_pm, string time)
        {
            try
            {
                string str = string.Format("delete from CLIENT_ON_TIME_ALARM_CHECK where PC_IP = '{0}' and ALARM_ON_YEAR='{1}' and ALARM_ON_MONTH='{2}' and ALARM_ON_DAY='{3}' and ALARM_ON_DAYS = 0 and ALARM_AM_PM={4} and ALARM_ON_TIME='{5}'", ip, year, month, day, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }

        public void delete_Pc_Off_date(string ip, string year, string month, string day, int am_pm, string time)
        {
            try
            {
                string str = string.Format("delete from CLIENT_OFF_TIME_ALARM_CHECK where PC_IP = '{0}' and ALARM_OFF_YEAR='{1}' and ALARM_OFF_MONTH='{2}' and ALARM_OFF_DAY='{3}'and ALARM_OFF_DAYS = 0 and ALARM_AM_PM={4} and ALARM_OFF_TIME='{5}'", ip, year, month, day, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }

        public void delete_Pc_On_days(string ip, int days, int am_pm, string time)
        {
            try
            {
                string str = string.Format("delete from CLIENT_ON_TIME_ALARM_CHECK where PC_IP = '{0}'  and ALARM_ON_DAYS={1} and ALARM_AM_PM={2} and ALARM_ON_TIME='{3}'", ip, days, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }

        public void delete_Pc_Off_days(string ip, int days, int am_pm, string time)
        {
            try
            {
                string str = string.Format("delete from CLIENT_OFF_TIME_ALARM_CHECK where PC_IP = '{0}' and ALARM_OFF_DAYS={1} and ALARM_AM_PM={2} and ALARM_OFF_TIME='{3}'", ip, days, am_pm, time);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }
        #endregion

        #region 전체알람삭제
        public void delete_Pc_Off()
        {
            try
            {
                string str = "delete from CLIENT_OFF_TIME_ALARM_CHECK";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }

        public void delete_Pc_On()
        {
            try
            {
                string str = "delete from CLIENT_ON_TIME_ALARM_CHECK ";
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" 알람 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" 알람 삭제실패 : " + e.Message);
            }
        }
        #endregion

        #region PC 정보 얻기 룸 넘버로부터
        public List<CComputer> ClientList(string room_num)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM CLIENT WHERE ROOM_NUM ={0}", room_num);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CComputer> pls = new List<CComputer>();

              while (rd.Read())
            {
                CComputer pl = new CComputer();

                pl.pointer_name = rd["POINTER_NAME"].ToString();
                pl.pc_state = rd["PC_STATE"].ToString();

                pls.Add(pl);
            }
          
            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 프로그램 정보 얻기 룸 넘버로부터
        public List<CProgram> ProgramList(string room_num)
        {
            string msg = string.Empty;
            string query = string.Format("SELECT * FROM PROGRAMS WHERE ROOM_NUM");
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CProgram> pls = new List<CProgram>();

            while (rd.Read())
            {
                CProgram pl = new CProgram();

                pl.Program_Name = rd["PROGRAM_NAME"].ToString();
                pl.Key_State = rd["KEY_STATE"].ToString();
                pl.Key_File_Name = rd["KEY_FILE_NAME"].ToString();
             
                pls.Add(pl);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 복구지점 정보 얻기 룸 넘버로부터
        public List<CPoint> PointList(string floor_num)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM POINTER WHERE FLOOR_NUM = {0}", floor_num);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CPoint> pls = new List<CPoint>();

            while (rd.Read())
            {
                CPoint pl = new CPoint();

                pl.Pointer_Name = rd["POINTER_NAME"].ToString();
             
                pls.Add(pl);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 층 정보 얻기 층 IP로부터
        public List<CFloor> Get_Floor_Info(string floor_ip)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM FLOOR_SERVER WHERE FLOOR_IP='{0}'", floor_ip);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CFloor> fls = new List<CFloor>();

            while (rd.Read())
            {
                CFloor floor = new CFloor();

                floor.Floor_IP = rd["FLOOR_IP"].ToString();
                floor.Floor_NUM = int.Parse(rd["FLOOR_NUM"].ToString());
                floor.Floor_NAME = rd["FLOOR_NAME"].ToString();
                floor.Floor_Power = int.Parse(rd["FLOOR_POWER"].ToString());

                fls.Add(floor);
            }

            rd.Close();
            conn.Close();
            return fls;
        }
        #endregion

        #region PC 복구지점 얻기 PC IP로부터
        public CComputer Get_PC_POINTER(string pc_ip)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM CLIENT WHERE PC_IP ='{0}'", pc_ip);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            CComputer fls = new CComputer();

            while (rd.Read())
            {
                fls.pointer_name = rd["POINTER_NAME"].ToString();
            }

            rd.Close();
            conn.Close();
            return fls;
        }
        #endregion

        #region 프로그램 추가하기
        public void Program_Insert(CProgram pls)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("INSERT INTO PROGRAMS(PROGRAM_NAME, PROGRAM_PATH, PROGRAM_LENGTH, KEY_STATE, KEY_FILE_NAME, POINTER_NAME) values('{0}','{1}','{2}','{3}','{4}','{5}')", pls.Program_Name, pls.Program_Path, pls.Program_Length, pls.Key_State, pls.Key_File_Name, pls.Pointer_Name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region 지점 추가하기
        public void Point_Insert(CPoint pls)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("INSERT INTO POINTER(POINTER_NAME, FLOOR_NUM) values('{0}','{1}')", pls.Pointer_Name, pls.Floor_Num);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region 키 등록하기(키 상태)
        public void Key_State_Update(CProgram pls)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("UPDATE PROGRAMS SET KEY_STATE='{0}' WHERE PROGRAM_NAME='{1}' AND POINTER_NAME ='{2}'", pls.Key_State,pls.Program_Name, pls.Pointer_Name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region 키 등록하기(키 파일 이름)
        public void Key_File_Name_Update(CProgram pls)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("UPDATE PROGRAMS SET KEY_FILE_NAME='{0}' WHERE PROGRAM_NAME='{1}' AND POINTER_NAME ='{2}'", pls.Key_File_Name, pls.Program_Name, pls.Pointer_Name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region 키 등록하기(키 파일 경로)
        public void Key_File_Path_Update(CProgram pls)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("UPDATE PROGRAMS SET KEY_FILE_PATH='{0}' WHERE PROGRAM_NAME='{1}' AND POINTER_NAME ='{2}'", pls.Key_File_Path, pls.Program_Name, pls.Pointer_Name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region PC 복구 지점 수정
        public void Pc_Pointer_Update(string pc_ip, string pointer)
        {
            try
            {
                CMyDB db = new CMyDB();
                db.ConnectDB();
                string str = string.Format("UPDATE CLIENT SET POINTER_NAME='{0}' WHERE PC_IP='{1}'", pointer, pc_ip);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.ConnectionString = constring1;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        #endregion

		#region PC_STATE 수정
		public void Pc_STATE_Update(string pc_state,string pc_ip)
		{
			try
			{
				CMyDB db = new CMyDB();
				db.ConnectDB();
				string str = string.Format("UPDATE CLIENT SET PC_STATE = '{0}' WHERE PC_IP='{1}'", pc_state, pc_ip);
				SqlCommand cmd = new SqlCommand(str, conn);
				conn.ConnectionString = constring1;
				conn.Open();
				cmd.ExecuteNonQuery();
				conn.Close();
			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.Message);
			}
		}
		#endregion

        #region 룸 정보 얻기 층 넘버로부터
        public List<CRoom> Get_Room_Info(string floor_num)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM ROOM WHERE FLOOR_NUM='{0}'", floor_num);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CRoom> rls = new List<CRoom>();

            while (rd.Read())
            {
                CRoom rl = new CRoom();

                rl.Floor_num = int.Parse(rd["FLOOR_NUM"].ToString());
                rl.Room_num = int.Parse(rd["ROOM_NUM"].ToString());
                rl.Room_name = rd["ROOM_NAME"].ToString();

                rls.Add(rl);
            }

            rd.Close();
            conn.Close();
            return rls;
        }
        #endregion

        #region 프로그램 정보 얻기 지점명으로 부터
        public List<CProgram> Get_Program_Info(string pointer_name)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM PROGRAMS WHERE POINTER_NAME='{0}'", pointer_name);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CProgram> pls = new List<CProgram>();

            while (rd.Read())
            {
                CProgram program = new CProgram();

                program.Program_Name = rd["PROGRAM_NAME"].ToString();
                program.Program_Path= rd["PROGRAM_PATH"].ToString();
                program.Program_Length = rd["PROGRAM_LENGTH"].ToString();
                program.Key_State = rd["KEY_STATE"].ToString();
                program.Key_File_Name = rd["KEY_FILE_NAME"].ToString();
                program.Key_File_Path = rd["KEY_FILE_PATH"].ToString();
                program.Pointer_Name = rd["POINTER_NAME"].ToString();

                pls.Add(program);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 지점 정보 얻기 층 넘버로부터
        public List<CPoint> Get_Pointer_Info(string floor_num)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM POINTER WHERE FLOOR_NUM='{0}'", floor_num);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CPoint> pls = new List<CPoint>();

            while (rd.Read())
            {
                CPoint program = new CPoint();

                program.Pointer_Name = rd["POINTER_NAME"].ToString();
            
                pls.Add(program);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 프로그램 얻기 키 파일로부터
        public List<CProgram> Get_Program_Info2(string key_file_name)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM PROGRAMS WHERE KEY_FILE_NAME='{0}'", key_file_name);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CProgram> pls = new List<CProgram>();

            while (rd.Read())
            {
                CProgram program = new CProgram();

                program.Program_Name = rd["PROGRAM_NAME"].ToString();

                pls.Add(program);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region 프로그램 얻기 파일경로부터
        public List<CProgram> Get_Program_Info3(string file_name_path)
        {
            string msg = string.Empty;
            string quary = string.Format("SELECT * FROM PROGRAMS WHERE PROGRAM_PATH='{0}'", file_name_path);
            SqlCommand cmd = new SqlCommand(quary, conn);
            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();

            List<CProgram> pls = new List<CProgram>();

            while (rd.Read())
            {
                CProgram program = new CProgram();

                program.Program_Name = rd["PROGRAM_NAME"].ToString();

                pls.Add(program);
            }

            rd.Close();
            conn.Close();
            return pls;
        }
        #endregion

        #region PC초기설정 삭제하기
        public void delete_program(string program_name)
        {
            try
            {
                string str = string.Format("delete from PROGRAMS where PROGRAM_NAME='{0}'", program_name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PROGRAM_name 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" PROGRAM_name 삭제실패 : " + e.Message);
            }
        }

        public void delete_program2(string pointer_name)
        {
            try
            {
                string str = string.Format("delete from PROGRAMS where POINTER_NAME='{0}'", pointer_name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" PROGRAM2_name 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" PROGRAM2_name 삭제실패 : " + e.Message);
            }
        }

        public void delete_pointer(string pointer_name)
        {
            try
            {
                string str = string.Format("delete from POINTER where POINTER_NAME='{0}'", pointer_name);
                SqlCommand cmd = new SqlCommand(str, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(" POINTER_name 삭제성공");
            }
            catch (Exception e)
            {
                Console.WriteLine(" POINTER_name 삭제실패 : " + e.Message);
            }
        }

        #endregion
    }

}
