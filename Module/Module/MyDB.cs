using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Threading;

namespace Module
{
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
        //public int hardware;
        public int format_file_image;
        public string last_format;
        public int windowaccount;
        public int on_time;
        public int off_time;
        public string pc_panel;
    }

    class MyDB
    {
        #region 맴버
        SqlConnection conn = null;
        SqlConnection conn1 = null;
        static string dbname = string.Empty;
        private string Floor_IP = string.Empty;
        private string Floor_NAME = string.Empty;
        private int dbCount;
        private string[] db_name;
        public static int count = 0; 
        #endregion

        #region 생성자
        public MyDB()
        {
 
        }

        public MyDB(string f_ip)
        {
            Floor_IP =f_ip;
        }

        public MyDB(string f_name,string f_ip)
        {
            Floor_NAME = f_name;
            Floor_IP = f_ip;
        }
        #endregion
      
        //DB카운트 받아오기
        public void SearchDB()
        {
            try
            {
                //================================DB카운트 받아오기=============================
                string sqlDBQuery = "SELECT count(*) as DatabaseCount FROM master.sys.databases";
                conn = new SqlConnection();
                conn.ConnectionString = "Server=61.81.99.86;database=master;uid=Dots;pwd=1234";
                SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
                conn.Open();
                dbCount = (int)myCommand.ExecuteScalar() - 4; //실제 데이터베이스(Dots) 개수          
                sqlDBQuery = string.Empty;               
                conn.Close();
                //===========================================================================
                db_name = new string[dbCount + 4];

                //=======================DB이름 받아오기====================================
                sqlDBQuery = "SELECT name FROM master.sys.databases order by name asc";
                myCommand = new SqlCommand(sqlDBQuery, conn);

                conn.Open();
                SqlDataReader rd = myCommand.ExecuteReader();

                int count = 0;
                while (rd.Read())
                {
                    db_name[count] = rd["name"].ToString();
                    count++;
                }
                if (count == 4) //db가 아무것도 없을때
                {
                    dbname = "Dots1000";//  생성될db이름
                }               
                else //db가 하나라도 있을때
                {
                    string[] packet = db_name[dbCount-1].Split('s');
                    int dbnamecount = int.Parse(packet[1]) + 1;     
                    dbname = "Dots" + dbnamecount; //  생성될db이름
                }

                for (int i = 0; i < dbCount; i++) //중복이름 확인하기
                {
                    if (dbname == db_name[i])
                    {
                        string[] packet = dbname.Split('s');
                        int dbnamecount = int.Parse(packet[1]) + 1;
                        dbname = "Dots" + dbnamecount; //  생성될db이름
                    }
                }
                //==========================================================================
            
            }
            catch
            {
                Console.WriteLine("카운트찾아오기실패");
            }
            finally
            {
                conn.Close();
            }

        }

        //층정보 모두
        public List<CFloor> FloorsGet()
        {
            List<CFloor> Floors = new List<CFloor>();
            for (int j = 0; j < dbCount ; j++)
            {
                try
                {
                    string sqlDBQuery = "SELECT * From FLOOR_SERVER";
                    string dbname = db_name[j];
                    conn = new SqlConnection();
                    conn.ConnectionString = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);

                    SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
                    conn.Open();

                    SqlDataReader rd = myCommand.ExecuteReader();

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
               
                }
                catch
                {
                    Console.WriteLine("층 정보 검색 실패");
                    return null;

                }
                finally
                {
                    
                    conn.Close();
                }
            }
            return Floors;
        }

        //층 정보 
        public List<CFloor> SelectFloor(string ip)
        {
            try
            {
                string str = string.Format("SELECT * From FLOOR_SERVER where FLOOR_IP = '{0}'", ip);

                SqlCommand cmd = new SqlCommand(str, conn1);

                conn1.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                List<CFloor> floors = new List<CFloor>();
                while (rd.Read())
                {
                    CFloor floor = new CFloor();

                    floor.Floor_NAME = rd["FLOOR_NAME"].ToString();
                    floor.Floor_IP = rd["FLOOR_IP"].ToString();
                    floor.Floor_NUM = int.Parse(rd["FLOOR_NUM"].ToString());
                    floor.Floor_Power = int.Parse(rd["FLOOR_POWER"].ToString());
                    floors.Add(floor);
                }
                rd.Close();
                conn1.Close();

                return floors;
            }
            catch (Exception)
            {
                return null;
            }
 
        }

        #region 어드민 업데이트 하기
          
        public string admin_Upadate()
        {
            try
            {
                List<CFloor> floors = FloorsGet();

                string sendData = string.Empty;
                sendData += "UPDATE" + "+";

                for (int i = 0; i < floors.Count; i++)
                {
                    string dbname = db_name[i];
                    string constring = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);

                    sendData += floors[i].Floor_NAME + "@";     //층 이름 
                    sendData += floors[i].Floor_IP + "@";       //층 IP 

                    int floors_Count = 0;
                    int floors_Accept_Count = 0;

                    List<CRoom> rooms = SelectRoom_For_FloorNum(floors[i].Floor_NUM,constring);  //층의 룸코드.
                    for (int z = 0; z < rooms.Count; z++)
                    {
                        floors_Count += GetPCCount_For_RoomNum(rooms[z].Room_num, constring);                 //강의실 pc 수   
                        floors_Accept_Count += GetPCAcceptCount_For_RoomNum(rooms[z].Room_num, constring);    //강의실 pc 접속자 수 
                    }
                    sendData += floors_Count + "@";                  //층 총 PC
                    sendData += floors_Accept_Count + "@";             //접속한 총 PC

                    sendData += rooms.Count.ToString() + "@";                  //층 소속된 강의실 수
                    for (int k = 0; k < rooms.Count; k++)
                    {
                        List<CComputer> coms = selectPC_For_RoomNum(rooms[k].Room_num, constring);

                        sendData += rooms[k].Room_num.ToString() + "@";  //층의 소속된 강의실 호수.
                        sendData += coms.Count.ToString() + "@";          //층의 총 PC 수

                        int rooms_Accept_Count = 0;
                        for (int f = 0; f < coms.Count; f++)
                        {
                            if (coms[f].power == 1)
                                rooms_Accept_Count++;
                        }
                        sendData += rooms_Accept_Count.ToString();    //층의 접속자수
                        if (k + 1 != rooms.Count)
                            sendData += "@";    //층의 접속자수
                    }

                    sendData += "#";    //한 층 정보 끝.
                }

                sendData += "&";    //마지막 .

                return sendData;
                
            }
            catch (Exception e)
            {
                Console.WriteLine("ADMIN 업데이트(최신화) 실패 recive(x) : " + e.Message);
                return null;
            }
        }

        //==============admin_Upadate()안에서 처리===============
        public List<CRoom> SelectRoom_For_FloorNum(int floor_num,string constring)
        {
            try
            {

                List<CRoom> Rooms = new List<CRoom>();

                string strSql = string.Format("SELECT * From ROOM where FLOOR_NUM = {0}", floor_num.ToString());
                conn = new SqlConnection();
                conn.ConnectionString = constring;
                
                SqlCommand cmd = new SqlCommand(strSql, conn);
                conn.Open();
                SqlDataReader rd = cmd.ExecuteReader();

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

        public int GetPCCount_For_RoomNum(int room_Num, string constring)
        {
            try
            {
                int count = 0;
                string strSql = "SELECT * From CLIENT where ROOM_NUM = '" + room_Num + "'";
                conn = new SqlConnection();
                conn.ConnectionString = constring;

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

        public int GetPCAcceptCount_For_RoomNum(int room_Num, string constring)
        {
            try
            {
                int count = 0;

                string strSql = "SELECT * From CLIENT where ROOM_NUM = '" + room_Num + "' AND POWER = 1";
                conn = new SqlConnection();
                conn.ConnectionString = constring;
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

        public List<CComputer> selectPC_For_RoomNum(int Room_Num, string constring)
        {
            try
            {
                List<CComputer> Pcs = new List<CComputer>();

                string strSql = "SELECT * From CLIENT where ROOM_NUM = " + Room_Num.ToString();
                conn = new SqlConnection();
                conn.ConnectionString = constring;
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
                    pc.format_file_image = int.Parse(rd[7].ToString());
                    pc.last_format = rd[8].ToString();
                    pc.windowaccount = int.Parse(rd[9].ToString());
                    pc.on_time = int.Parse(rd[10].ToString());
                    pc.off_time = int.Parse(rd[11].ToString());
                    pc.pc_panel = rd[12].ToString();
                    

                    Pcs.Add(pc);
                }
                rd.Close();
                conn.Close();
                
                return Pcs;
            }
            catch (Exception e)
            {
                Console.WriteLine("PC얻기 룸넘버로부터. 실패 : " + e.Message);
                return null;
            }
        }
        //========================================================
        #endregion



        // 생성한 DB에 연결       
        public void ConnectDB()
        {
            string constring = string.Empty;
            constring = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);
            conn1 = new SqlConnection();
            conn1.ConnectionString = constring;
            conn1.Open();
        }
             
        #region 데이터베이스 생성/삭제
        // DB생성
        public void CreateDatabase()
        {
            count++;
            string sqlDBQuery;
            conn = new SqlConnection();
            conn.ConnectionString = "Server=61.81.99.86;database=master;uid=Dots;pwd=1234";
            
            sqlDBQuery = "CREATE DATABASE " + dbname;

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
            try
            {
                conn.Open();
                myCommand.ExecuteNonQuery();

                #region 테이블생성호출
                FloorServer();
                Thread.Sleep(1);
                InsertFloor(Floor_NAME, Floor_IP, count);
                Thread.Sleep(1);
                Room();               
                Thread.Sleep(1);
                Client();
                Thread.Sleep(1);
                NoneFilePc();
                Thread.Sleep(1);
                ClientSoftwareName();
                Thread.Sleep(1);
                ClientSoftwareSet();
                Thread.Sleep(1);
                ClientSoftwareCheck();
                Thread.Sleep(1);
                ClientDeviceStatus();
                Thread.Sleep(1);
                ClientOnTimeAlarmCheck();
                Thread.Sleep(1);
                ClientOffTimeAlarmCheck();
                Thread.Sleep(1);
                FormatFileExeCheckS();
                Thread.Sleep(1);
                FormatFileImageCheckS();
                Thread.Sleep(1);
                LogType();
                Thread.Sleep(1);
                Log();
                Thread.Sleep(1);
                Log2();
                Thread.Sleep(1);
                Log3();
                Thread.Sleep(1);
                ClientWindowAccountsName();
                Thread.Sleep(1);
                ClientWindowAccountsCheck();
                Thread.Sleep(1);
                ClientSoftwareUse();
                Thread.Sleep(1);
                InsertLogType();
                Thread.Sleep(1);
                POINTER();
                Thread.Sleep(1);
                PROGRAMS();
                #endregion
                
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return;
        }

        // DB삭제
        public void DeleteDatabase()
        {
            string ip = string.Empty;
            string sqlDBQuery;

            for (int i = 0; i < dbCount; i++)
            {
                try
                {
                    string dbname = db_name[i];
                    conn = new SqlConnection();
                    conn.ConnectionString = string.Format("Server=61.81.99.86;database='{0}';uid=Dots;pwd=1234", dbname);
                    sqlDBQuery = "SELECT FLOOR_IP From FLOOR_SERVER where FLOOR_IP = '" + Floor_IP + "'";

                    SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn);
                    conn.Open();

                    ip = (string)myCommand.ExecuteScalar(); //자동생성된 시스템데이터베이스가 4개라 빼야한다
                    sqlDBQuery = string.Empty;
                    conn.Close();

                    if (Floor_IP == ip) // 내 아이피에 맞는 db를 찾기
                    {
                        conn = new SqlConnection();
                        conn.ConnectionString = "Server=61.81.99.86;database=master;uid=Dots;pwd=1234";

                        string sqlDBQuery1 = "CREATE TABLE #sp_who2_table (SPID INT,Status VARCHAR(255),"
                                    + "Login VARCHAR(255),HostName VARCHAR(255),"
                                    + "BlkBy VARCHAR(255),DBName VARCHAR(255),"
                                    + "Command VARCHAR(255),CPUTime INT,"
                                    + "DiskIO VARCHAR(255),LastBatch VARCHAR(255),"
                                    + "ProgramName VARCHAR(255),SPID2 INT,"
                                    + "REQUESTID INT)";

                        string sqlDBQuery2 = "INSERT INTO #sp_who2_table EXEC sp_who2";
                        string sqlDBQuery3 = string.Format("SELECT SPID FROM #sp_who2_table where DBNAME='{0}'", dbname);
                       
                        SqlCommand myCommand1 = new SqlCommand(sqlDBQuery1, conn);
                        SqlCommand myCommand2 = new SqlCommand(sqlDBQuery2, conn);
                        SqlCommand myCommand3 = new SqlCommand(sqlDBQuery3, conn);

                        try
                        {
                            conn.Open();
                            myCommand1.ExecuteNonQuery();
                            Console.WriteLine("테이블생성 성공");
                            myCommand2.ExecuteNonQuery();
                            Console.WriteLine("인설트성공");
                            SqlDataReader rd = myCommand3.ExecuteReader();
                            string str = string.Empty;
                            while (rd.Read())
                            {
                                str += rd["SPID"].ToString() +"#";
                            }
                            rd.Close();
                            string[] packet = str.Split('#');
                            
                            for (int m = 0; m < packet.Length; m++)
                            {
                                string dbquery = "kill " + packet[m];
                                SqlCommand Command0 = new SqlCommand(dbquery, conn);
                                try
                                {
                                    Command0.ExecuteNonQuery();
                                }
                                catch
                                {
                                    Console.WriteLine("spid kill 실패 spid카운트 : " + packet.Length);
 
                                }                   
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            string sql = "DROP DATABASE " + dbname;
                            SqlCommand Command = new SqlCommand(sql, conn);
                            Command.ExecuteNonQuery();
                            conn.Close();
                            Console.WriteLine("디비삭제완료");
                        }
                        return;
                    }

                }
                catch
                {
                    Console.WriteLine("찾기실패");
                    conn.Close();
                }
            }

            return;

        }
        #endregion

       
        #region 테이블생성 및 초기설정
        public void FloorServer()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE FLOOR_SERVER ( "
                        + "FLOOR_IP varchar(50) null, "
                        + "FLOOR_NUM int PRIMARY KEY, "
                        + "FLOOR_NAME varchar(50) null, "
                        + "FLOOR_POWER int null"
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료1");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void InsertFloor(string floor_Name, string ip, int count)
        {
            try
            {
                string str = string.Format("INSERT into FLOOR_SERVER( FLOOR_IP, FLOOR_NUM, FLOOR_NAME, FLOOR_POWER) values('{0}', {1}, '{2}', {3})", ip, count ,floor_Name, 0);
                SqlCommand cmd = new SqlCommand(str, conn1);
                conn1.Open();
                cmd.ExecuteNonQuery();
                conn1.Close();
                Console.WriteLine("층 추가 실행2");
            }
            catch (Exception e)
            {
                Console.WriteLine("층 추가 오류 : " + e.Message);
            }
            finally
            {
                conn1.Close();
            }
        }

        public void Room()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE ROOM ( "
                        + "FLOOR_NUM int null, "
                        + "ROOM_NUM int PRIMARY KEY, "
                        + "ROOM_NAME varchar(50) null "
			            + "FOREIGN KEY(FLOOR_NUM) REFERENCES FLOOR_SERVER(FLOOR_NUM)"
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료3");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }
   	
        public void Client()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT ( "
                        + "ROOM_NUM int null, "
                        + "PC_TYPE varchar(50) null, "
                        + "PC_IP varchar(50) PRIMARY KEY, "
                        + "MAC varchar(50) null, "
                        + "POWER int null, "
                        + "COORDINATE varchar(50) null, "
                        + "SOFTWARE varchar(50) null, "
                        + "FORMAT_FILE_IMAGE int null, "
                        + "FORMAT_FILE_EXE int null, "
                        + "LAST_FORMAT varchar(50) null, "
                        + "WINDOWACCOUNT int null, "
                        + "ON_TIME int null, "
                        + "OFF_TIME int null, "
                        + "PC_PANEL varchar(50) null, "
                        + "POINTER_NAME varchar(50) null, "
                        + "PC_STATE varchar(50) null, "
                        + "FORMAT_STATE varchar(50) null, "
                        + "KEY_STATE varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료4");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }        

        public void NoneFilePc()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE NONEFILEPC ( "
			            + "NUM int identity(1,1), "
			            + "ROOM_NUM int null, "
			            + "TYPE varchar(50) null, "
                        + "PC_IP varchar(50) null, "
                        + "PERSENT varchar(MAX) null, "
                        + "COMPLETE int null, "
                        + "CONNECT int null, "
			            + "DATA_PERSENT varchar(50) null, "
                        + "DATA_CONNECT varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료5");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

	    public void ClientSoftwareName()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_SOFTWARE_NAME ( "
                        + "SOFTWARE_NUM int identity(1,1) PRIMARY KEY, "
                        + "SOFTWARE_NAME varchar(MAX), "
                        + "PC_IP varchar(50) null "
                        + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP)"
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료6");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

	    public void ClientSoftwareSet()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_SOFTWARE_SET ( "
                        + "SOFTWARE_NUM int identity(1,1) , "
                        + "SOFTWARE_NAME varchar(50) PRIMARY KEY "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료7");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientSoftwareCheck()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_SOFTWARE_CHECK ( "
                        + "PC_IP varchar(50) null, "
                        + "SOFTWARE_NAME varchar(50) null, "
                        + "SOFTWARE_OFFER int null, "
			            + "SOFTWARE_STATE varchar(50) null, "
			            + "SOFTWARE_IDX int identity(1,1) PRIMARY KEY "
			            + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP), "
			            + "FOREIGN KEY(SOFTWARE_NAME) REFERENCES CLIENT_SOFTWARE_NAME(SOFTWARE_NAME), "
			            + "FOREIGN KEY(SOFTWARE_NAME) REFERENCES CLIENT_SOFTWARE_SET(SOFTWARE_NAME)"
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료8");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientDeviceStatus()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE DEVICESTATUS ( "
                      	+ "PC_IP varchar(50) null, "
			            + "KEYBOARD varchar(50) null, "
			            + "MOUSE varchar(50) null, "
			            + "MONITER varchar(50) null "
			            + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료9");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientOnTimeAlarmCheck()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_ON_TIME_ALARM_CHECK  ( "
                        + "ALARM_INDEX int identity(1,1) PRIMARY KEY, "
                        + "PC_IP varchar(50) null, "
                        + "ALARM_ON_YEAR varchar(50) null, "
                        + "ALARM_ON_MONTH varchar(50) null, "
                        + "ALARM_ON_DAY varchar(50) null, "			            
                        + "ALARM_AM_PM int null, "
                        + "ALARM_ON_TIME varchar(50) null, "
                        + "ALARM_ON_DAYS int null, "
                        + "ALARM_ON_TEST_TIME int null "
			            + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료10");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientOffTimeAlarmCheck()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_OFF_TIME_ALARM_CHECK  ( "
                        + "ALARM_INDEX int identity(1,1) PRIMARY KEY, "
                        + "PC_IP varchar(50) null, "
                        + "ALARM_OFF_YEAR varchar(50) null, "
                        + "ALARM_OFF_MONTH varchar(50) null, "
                        + "ALARM_OFF_DAY varchar(50) null, "
                        + "ALARM_AM_PM int null, "
                        + "ALARM_OFF_TIME varchar(50) null, "
                        + "ALARM_OFF_DAYS int null, "
                        + "ALARM_OFF_TEST_TIME int null "
			            + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료11");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }      

        public void FormatFileExeCheckS()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE FORMAT_FILE_EXE_CHECK_S ( "
                        + "FORMAT_FILE_EXE_NUM_S int identity(1,1), "
                        + "FORMAT_FILE_EXE_TYPE_S varchar(50) null, "
                        + "FORMAT_FILE_EXE_NAME_S varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료12");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void FormatFileImageCheckS()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE FORMAT_FILE_IMAGE_CHECK_S ( "
                        + "FORMAT_FILE_IMAGE_NUM_S int identity(1,1), "
                        + "FORMAT_FILE_IMAGE_TYPE_S varchar(50) null, "
                        + "FORMAT_FILE_IMAGE_NAME_S varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료13");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

	    public void LogType()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE LOG_TYPE ( "
                        + "LOG_NUM int identity(1,1) PRIMARY KEY, "
                        + "LOG_TYPE_NAME varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료14");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void Log()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE LOG ( "
                        + "PC_IP varchar(50) null, "
		            	+ "LOG_NUM int null, "
                        + "LOG_NAME varchar(50) null, "
                        + "LOG_LOG varchar(MAX) null, "
                        + "LOG_DETECTION_DAY varchar(50) null, "
                        + "LOG_DETECTION_TIME varchar(50) null "
			            + "FOREIGN KEY(LOG_NUM) REFERENCES LOG_TYPE(LOG_NUM) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료15");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void Log2()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE LOG2 ( "
                        + "PC_IP varchar(50) null, "
                        + "LOG_NUM int null, "
                        + "LOG_LOG varchar(MAX) null, "
                        + "LOG_DETECTION_DAY varchar(50) null, "
                        + "LOG_DETECTION_TIME varchar(50) null "
                        + "FOREIGN KEY(LOG_NUM) REFERENCES LOG_TYPE(LOG_NUM) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료16");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void Log3()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE LOG3 ( "
                        + "PC_IP varchar(50) null, "
                        + "LOG_NUM int null, "
                        + "ROOM_NAME varchar(50) null, "
                        + "START_TIME varchar(50) null, "
                        + "TIME_LOG varchar(50) null, "
                        + "LOG_DETECTION_DAY varchar(50) null, "
                        + "LOG_DETECTION_TIME varchar(50) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료17");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientWindowAccountsName()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_WINDOWACCOUNTS_NAME ( "
                        + "idx int identity(1,1) PRIMARY KEY, "
                        + "ACCOUNT varchar(MAX) null, "
                        + "PASSWORD varchar(MAX) null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료18");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void ClientWindowAccountsCheck()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_WINDOWACCOUNTS_CHECK ( "
                        + "PC_IP varchar(50) null, "
                        + "ACCOUNT varchar(50) null, "
                        + "PW varchar(50) null, "
                        + "STATE varchar(50) null, "
                        + "TIME varchar(50) null, "
                        + "WINDOWACCOUNT int PRIMARY KEY "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료19");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

	    public void ClientSoftwareUse()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE CLIENT_SOFTWARE_USE ( "
                        + "IDX int identity(1,1), "
                        + "PC_IP varchar(50) null, "
                        + "SOFTWARE_NAME varchar(50) null, "
                        + "SOFTWARE_USE varchar(50) null "
                        + "FOREIGN KEY(PC_IP) REFERENCES CLIENT(PC_IP) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료20");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

	    public void InsertLogType()
        {
            ConnectDB();

            string sqlDBQuery = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('PC셋팅')";
            string sqlDBQuery1 = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('PC모니터링')";
            string sqlDBQuery2 = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('PC제어')";
            string sqlDBQuery3 = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('PC 사용시간')";
            string sqlDBQuery4 = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('소프트웨어 사용시간')";
            string sqlDBQuery5 = "INSERT into LOG_TYPE( LOG_TYPE_NAME) values('원격제어 사용시간')";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            SqlCommand myCommand1 = new SqlCommand(sqlDBQuery1, conn1);
            SqlCommand myCommand2 = new SqlCommand(sqlDBQuery2, conn1);
            SqlCommand myCommand3 = new SqlCommand(sqlDBQuery3, conn1);
            SqlCommand myCommand4 = new SqlCommand(sqlDBQuery4, conn1);
            SqlCommand myCommand5 = new SqlCommand(sqlDBQuery5, conn1);
           
            try
            {
                Console.WriteLine("인설트로그완료21");

                myCommand.ExecuteNonQuery();
                myCommand1.ExecuteNonQuery();
                myCommand2.ExecuteNonQuery();
                myCommand3.ExecuteNonQuery();
                myCommand4.ExecuteNonQuery();
                myCommand5.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void POINTER()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE POINTER ( "
                        + "POINTER_NAME varchar(50) PRIMARY KEY, "
                        + "FLOOR_NUM int null "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료22");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }

        public void PROGRAMS()
        {
            string sqlDBQuery;
            ConnectDB();

            sqlDBQuery = "CREATE TABLE PROGRAMS ( "
                        + "PROGRAM_NAME varchar(50) PRIMARY KEY, "
                        + "PROGRAM_PATH varchar(MAX), "
                        + "PROGRAM_LENGTH varchar(50), "
                        + "KEY_STATE bit, "
                        + "KEY_FILE_NAME varchar(50) null, "
                        + "KEY_FILE_PATH varchar(MAX) null, "
                        + "POINTER_NAME varchar(50) null "
                        + "FOREIGN KEY(POINTER_NAME) REFERENCES POINTER(POINTER_NAME) "
                        + ")";

            SqlCommand myCommand = new SqlCommand(sqlDBQuery, conn1);
            try
            {
                Console.WriteLine("테이블생성완료23");

                myCommand.ExecuteNonQuery();

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn1.Close();
            }
            return;
        }
	

        #endregion
    }
}

