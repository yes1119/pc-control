using Microsoft.Win32;
using Pc_Client.SendData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pc_Client
{
    public delegate void Send(byte[] data);
   public delegate void Remote_Send();
   public delegate void Remote_end();
    public delegate void Event_Check();
    public delegate void Key_Event();

    class CControl
    {
        public Key_Event keyevent_start;

        public CControl(Key_Event ke)
        {
            this.keyevent_start = ke;
        }
        public class Key_List
        {
            public string Key_Name { get; set; }
            public string Key_Value { get; set; }
            public string Key_Sleep { get; set; }
        }

        Funtion fun = new Funtion();
        static string client_ip;
        public static List<Key_List> kls = new List<Key_List>();

        
        public class PROCESS
        {
            public string ProcessName { get; set; }
        }
        #region Member

        public static bool state { get; set; }
        private Socket client;
        public static Send send_Function = null;
        static ProcessCheck procChk = new ProcessCheck(new string[] { "", "", "" }, 1);
        public static ManualResetEvent FinishEvent = new ManualResetEvent(false);
        
        static string start_time = string.Empty;
        static List<PROCESS> prcessName = new List<PROCESS>();
        static string[] Start_Time = null;

        public Remote_Send remote_start = null;
        public Event_Check Check_start = null;          
        public bool ScreenSleep = false;
        //  private Funtion fun;
        private string server_IP = string.Empty;
        public string Remoteip = string.Empty;
        public bool Remote_check = false;
        //private int server_Port = 0;
        System.Threading.ManualResetEvent start = new System.Threading.ManualResetEvent(false);
        #endregion

        #region trayicon


        public void trayIconShow()
        {
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();

            ni.Icon = new System.Drawing.Icon(@"C:\pivotal-tracker-fluid-icon-2013.ico");
            ni.Visible = true;
            ni.DoubleClick += delegate(object sender, EventArgs a)  //더블클릭
            {
                MainWindow m = new MainWindow();
                m.Show();
                m.WindowState = WindowState.Normal;
                // this.Show();
                // this.WindowState = WindowState.Normal;
            };
            ni.Text = "이름입력";   //트레이아이콘 위에 올리면 나오는 텍스트

            taryIcon_Create_ContextMenu(ni);   //메뉴생성
        }

        public void taryIcon_Create_ContextMenu(System.Windows.Forms.NotifyIcon ni)
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem();
            item1.Index = 0;
            item1.Text = "종료하기";
            item1.Click += delegate(object click, EventArgs eClick)
            {
                MessageBox.Show("종료합니다.");
                Environment.Exit(0);
            };
            menu.MenuItems.Add(item1);
            ni.ContextMenu = menu;
        }
        #endregion

        public void control_Receive(Socket client, string packetType, string[] packetData)
        {
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            string myIP = IPHost.AddressList[0].ToString();
            client_ip = myIP;
            #region 서버로부터 재접속 요청  2015 02 02 조수현
            if (packetType == "RELOGIN")
            {
                try
                {
                    Funtion fun = new Funtion();

                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_LOGIN+";
                    sendData1 += myIP + "#";     //서버 주소.
                    sendData1 += fun.GetMacAddress().ToString() + "#";
                    sendData1 += fun.GetPcName().ToString() + "#";
                    sendData1 += Window1.Client_type + "#";
                    sendData1 += Funtion.f_registry() + "#";
                    sendData1 += MainWindow.software;
                    //byte로 변환후 보내기.
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion
           
            #region ShutDown
            
            if (packetType == "SHUTDOWN")
            {
                Process.Start("shutdown.exe", "-s -t 1");//셧다운ㅋ 
            }
            #endregion

            #region 원격제어
            if (packetType == "REMOTE_SEND")
            {
                try
                {
                    //ScreenSleep = true;
                    Remoteip = packetData[0];
                    //Remote_check = true;
                    //remote_sock.Start(control.Remoteip, 8000);
                    remote_start();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 이벤트 체크 
            if (packetType == "EVENTCHECK") 
            {
                Check_start();
            }
            #endregion

            #region 윈도우
            if (packetType == "CLIENT_WINDOWDELETE")
            {
                try
                {
                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_WINDOW_DELTE_RESULT+";
                    sendData1 += myIP + "#";

                    string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    string[] defaultaccount = currentUser.Split('\\');
                    if (defaultaccount[1] == packetData[1])
                    {
                        Console.WriteLine("로그인한 계정을 삭제했습니다.");
                        Console.WriteLine("관리자 계정으로 로그인합니다.");
                        sendData1 += packetData[1] + "#";
                        sendData1 += packetData[2] + "#";
                        sendData1 += "삭제실행";
                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                        //Funtion.CreateUserAccount("Administrator", "dumy", "일단 넣지 않았다.", true);
                        Funtion.remeve(packetData[1]);
                        WriteDefaultLogin("Administrator", "");
                        Thread.Sleep(2000);
                        Process.Start("shutdown.exe", "-s -t 1");
                    }
                    else
                    {
                        bool check = iswindowcheck(packetData[0]);
                        if (check == true)
                        {
                            sendData1 += packetData[1] + "#";
                            sendData1 += packetData[2] + "#";
                            sendData1 += "삭제실행";
                            byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                            send_Function(packet1);
                            Funtion.remeve(packetData[1]);
                            Console.WriteLine(packetData[1] + "를 삭제 했습니다.!!!");
                        }
                        else
                        {
                            sendData1 += packetData[1] + "#";
                            sendData1 += packetData[2] + "#";
                            sendData1 += "해당계정없음";
                            byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                            send_Function(packet1);
                            Console.WriteLine(packetData[0] + "는 존재하지 않습니다.");
                        }

                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 제어판
            if (packetType == "CLIENT_CONTROL_PANEL")
            {
                try
                {
                    Funtion fn = new Funtion();
                    string sendData1 = string.Empty;
                    if (packetData[1] == "false")       //제어 해제
                    {
                        Funtion.f_registry(false);
                        Funtion.Kill();
                        sendData1 += "CLIENT_CONTROL_PANEL_RESULT+";
                        sendData1 += myIP + "#";
                        sendData1 += "false";
                    }
                    if (packetData[1] == "true")        //제어 실행
                    {
                        Funtion.f_registry(true);
                        Funtion.Kill();
                        sendData1 += "CLIENT_CONTROL_PANEL_RESULT+";
                        sendData1 += myIP + "#";
                        sendData1 += "true";
                    }
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);
                    Console.WriteLine("CLIENT_CONTROL_PANEL 성공");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 디바이스
            if (packetType == "CLIENT_DeviceStatus")
            {
                try
                {
                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_DEVICE_RESULT+";
                    sendData1 += myIP + "#";
                    for (int i = 0; i < packetData.Length - 1; i++)
                    {
                        fun.DeviceFind(packetData[i]);
                        sendData1 += fun.Name + "\v" + fun.Status + "\v" + fun.Work + "\v" + "#";
                    }

                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);
                    Console.WriteLine("Success!!...");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            #endregion

            #region 포맷
            if (packetType == "CLIENT_FORMAT")
            {
                try
                {
                    if (packetData[1] == "1")
                    {
                        Funtion fun = new Funtion();
                        string sendData1 = string.Empty;
                        sendData1 += "CLIENT_FORMAT_RESULT+";
                        sendData1 += myIP + "#";
                        sendData1 += fun.GetMacAddress() + "#";
                        sendData1 += Window1.Client_type + "#";
                        sendData1 += "1" + "#";
                        sendData1 += "초기값" + "#";
                        sendData1 += "포맷 알림";

                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                        Console.WriteLine("초기 지점으로 복구 실행");
                    }

                    else if (packetData[1] == "2")
                    {
                        Funtion fun = new Funtion();
                        string sendData1 = string.Empty;
                        sendData1 += "CLIENT_FORMAT_RESULT+";
                        sendData1 += myIP + "#";
                        sendData1 += fun.GetMacAddress() + "#";
                        sendData1 += Window1.Client_type + "#";
                        sendData1 += "0" + "#";
                        sendData1 += packetData[3] + "#";
                        sendData1 += "포맷 알림";

                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                        Console.WriteLine("설정 지점으로 복구 실행");
                    }

                    SystemFormat.Start_Backup(int.Parse(packetData[1]), packetData[2]);

                    Console.WriteLine("포맷 성공");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region Data_Server_Start
            if (packetType == "DATA_SERVER_START")//클라 ip
            {
                try
                {
                    if (myIP != packetData[0])
                    {
                        SendData.AsynchronousSocketListener.FileToSend = "D:\\" + packetData[2];
                        SendData.AsynchronousSocketListener.acceptIP = packetData[0];
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            #endregion

            #region Data_Client_Start
            if (packetType == "DATA_CLIENT_START")//서버 ip
            {
                try
                {
                    if (myIP != packetData[0])
                    {
                        Console.WriteLine(packetData[1] + " : 파일 전송 시작");
                        SendData.AsynchronousClient.FILEname = packetData[1];
                        Thread.Sleep(3000);
                        //파일클라이언트 실행.
                        SendData.AsynchronousClient.IpAddress = IPAddress.Parse(packetData[0]);
                        SendData.AsynchronousClient.Port = 8500;
                        SendData.AsynchronousClient.FileSavePath = "D:\\";

                        Thread threadClient = new Thread(new ThreadStart(SendData.AsynchronousClient.StartClient));
                        threadClient.IsBackground = true;
                        threadClient.Start();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 키 이벤트
            if (packetType == "KEY_SEND_TO_CLIENT")//
            {
                try
                {
                    int length = 2;
                    if (myIP != packetData[0])
                    {
                        Console.WriteLine(packetData[1] + " : 키 이벤트 시작");
                        ShellExecute(0, "open", "D:\\" + packetData[1], "", "", 5);

                        Thread.Sleep(500);

                        for (int i = 2; i < packetData.Length - 1; i += length)
                        {
                            Key_List kl = new Key_List();
                            kl.Key_Name = packetData[1];
                            kl.Key_Sleep = packetData[i];
                            kl.Key_Value = packetData[i + 1];

                            kls.Add(kl);
                        }
                        keyevent_start();
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            #endregion

            #region 복원지점 설정
            if (packetType == "FINISHFILESEND")
            {
                try
                {
                    Console.WriteLine("복원 지점 설정 시작");
                    //   윈8,10은 인자 - 4
                    //   윈7(ULTIMATE7) 인자 - 5
                    //   윈7(HOMEDEITION) 인자 - 6

                    if (Window1.Client_type == "2") SystemFormat.SystemRestore(5);
                    else if (Window1.Client_type == "1") SystemFormat.SystemRestore(6);
                    else { SystemFormat.SystemRestore(4); }

                    FinishEvent.WaitOne();

                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_SETTING_FINISH+";
                    sendData1 += packetData[0] + "#";
                    sendData1 += myIP + "#";

                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region ip변경
            if (packetType == "CLIENT_IPCHANGE")
            {
                try
                {
                    Funtion fun = new Funtion();
                    client.Shutdown(SocketShutdown.Both);
                    string subnetMask = "255.255.255.0";
                    Funtion.setIP(packetData[1], subnetMask);
                    Console.WriteLine("Success!!...");

                    Thread.Sleep(5000);

                    state = fun.IsConnectedToInternet();

                    if (state == true)
                    {
                        FileInfo client_change_ip_txt = new FileInfo(Window1.change_ip);
                        FileStream client_change_ip_txt_stream = client_change_ip_txt.Create();
                        TextWriter tw3 = new StreamWriter(client_change_ip_txt_stream, Encoding.UTF8);
                        tw3.Write(packetData[0]);
                        tw3.Close();
                        //======================================
                        client_change_ip_txt_stream.Close();

                        Window1.Change_ip = packetData[0];
                        //mySocket.server_IP = packetData[0];
                        Console.WriteLine("인터넷 연결 성공!!!");

                        
                        Process.Start("shutdown.exe", "-r -t 1");
                        Console.WriteLine("IP변경으로 인해서 Pc를 재부팅 합니다...잠시만 기달려주세요.");
                    }
                    else
                    {
                        IPHostEntry IPHost1 = Dns.GetHostByName(Dns.GetHostName());
                        string myIP1 = IPHost1.AddressList[0].ToString();

                        Console.WriteLine("IP충돌났습니다.");
                        string subnetMask1 = "255.255.255.0";
                        Funtion.setIP(packetData[0], subnetMask1);
                        Console.WriteLine("다시 원래 ip로!!...");


                        string sendData1 = string.Empty;
                        sendData1 += "CLIENT_IPCHANGESTATE+";
                        sendData1 += myIP1 + "#";
                        sendData1 += packetData[0] + "#";
                        sendData1 += fun.GetMacAddress().ToString() + "#";
                        sendData1 += "FALSE";

                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 소프트웨어 
            if (packetType == "CLIENT_software")
            {
                try
                {
                    Console.WriteLine("찾는 대상 : " + packetData[1]);
                    bool stater = Funtion.installCheak(packetData[1]);
                    Console.WriteLine("설치 여부 결과 값 :" + stater);

                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_SOFTWARERESULT+";
                    sendData1 += packetData[1] + "#";
                    sendData1 += stater + "#";
                    sendData1 += myIP;
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);

                    Console.WriteLine("결과 보냄");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 소프트웨어 제대로 작동     
            if (packetType == "CLIENT_soft_respond")
            {
                try
                {
                    Console.WriteLine("찾는 대상 : " + packetData[1]);
                    bool stater = Funtion.processRespond(packetData[1]);
                    Console.WriteLine("작동 여부 결과 값 :" + stater);

                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_SOFT_RESPOND_RESULT+";
                    sendData1 += packetData[1] + "#";
                    sendData1 += stater + "#";
                    sendData1 += myIP;
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);

                    Console.WriteLine("결과 보냄");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 불필요 소프트웨어 ^▽^
            if (packetType == "NEEDLESS_CLIENT_SOFTWARE")
            {
                try
                {
                    //for(int i=0; i<packetData.Length-1 ; i++)
                    //    MessageBox.Show(packetData[i]); //[0] 61.81.99.83
                    Funtion.NeedlessSoftCheck(packetData);
                    string sendData = string.Empty;
                    sendData += "NEEDLESS_CLIENT_SOFTWARERESULT+★";
                    sendData += myIP + "☆";
                    sendData += Funtion.Compare();
                    byte[] packet1 = Encoding.Default.GetBytes(sendData);
                    send_Function(packet1);

                    Console.WriteLine("결과 보냄");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            #region 프로세스 목록 얻기
            if (packetType == "GETPROCESS")
            {
                Funtion.AppList();

                string sendData1 = string.Empty;
                sendData1 += "GETPROCESS_RESULT+★";
                sendData1 += myIP+"☆";

                for (int i = 0; i < Funtion.list2.Count; i++)
                {
                    sendData1 += Funtion.list2[i] + "☆";
                }
                byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                send_Function(packet1);
            }
            #endregion

            if (packetType == "CLIENT_SOFTWARE_DEL")
            {
                try
                {
                    bool result = Funtion.ForcedDel(packetData[1]);

                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_SOFTWARE_DEL_RESULT+★";
                    sendData1 += packetData[1] + "☆";
                    sendData1 += (result ? "삭제 성공" : "삭제 실패") + "☆";
                    sendData1 += myIP;
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    send_Function(packet1);
                    if (result)
                    {
                        listviewupdate(packetData[1], myIP);
                        Console.WriteLine("삭제 성공");
                    }
                    else
                    {
                        Console.WriteLine("삭제 실패");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region 윈도우로그인설정
            if (packetType == "CLIENT_WindowLogin")
            {
                try
                {
                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_WINDOW_RESULT+";
                    sendData1 += myIP + "#";

                    Console.WriteLine(packetData[1].ToString());//계정
                    Console.WriteLine(packetData[2].ToString());//패스워드
                    bool check = iswindowcheck(packetData[1]);
                    if (check == true)
                    {
                        sendData1 += packetData[1] + "#";
                        sendData1 += packetData[2] + "#";
                        sendData1 += "반복설정";
                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                        WriteDefaultLogin(packetData[1], packetData[2]);
                        Console.WriteLine("설정으로 인한 재부팅을 합니다.");
                        Thread.Sleep(2000);
                        Process.Start("shutdown", "/r /t 0");
                    }
                    else
                    {
                        sendData1 += packetData[1] + "#";
                        sendData1 += packetData[2] + "#";
                        sendData1 += "신규설정";
                        byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                        send_Function(packet1);
                        Console.WriteLine("계정이 없습니다....계정을 생성 합니다.");
                        Funtion.CreateUserAccount(packetData[1], packetData[2], "일단 넣지 않았다.", true);
                        Console.WriteLine("윈도우 계정이름 : " + packetData[1] + "생성!!!");
                        WriteDefaultLogin(packetData[1], packetData[2]);
                        Console.WriteLine("설정으로 인한 재부팅을 합니다.");
                        Thread.Sleep(2000);
                        Process.Start("shutdown", "/r /t 0");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion

            #region SOFTWARE_USE_CHECK
            if (packetType == "SOFTWARE_USE_CHECK")
            {
                try
                {
                    string[] software_name = new string[packetData.Length];
                    prcessName.Clear();
                    for (int i = 0; i < packetData.Length; i++)
                    {
                        PROCESS pr = new PROCESS();
                        software_name[i] = packetData[i].ToString();
                        Console.WriteLine("들어가는 이름 : " + software_name[i]);
                        pr.ProcessName = packetData[i].ToString();
                        prcessName.Add(pr);
                        Console.WriteLine(prcessName.Count());
                    }

                    procChk.ClearCheckList();

                    procChk = new ProcessCheck(software_name, 1);

                    procChk.ProcessChecked += ProcChk_ProcessChecked;

                    if (procChk.StartWatcher())
                        Console.WriteLine("Waiting...");
                    else
                        Console.WriteLine("ProcessChecked is null Or Checklist's count is Zero");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion
            //======================================================================================
        }

        private void listviewupdate(string filename, string ip)
        {
            string list = Funtion.Compare2(filename);
            string sendData = string.Empty;

            sendData += "NEEDLESS_CLIENT_SOFTWARERESULT+★";
            sendData += ip + "☆";
            sendData += list;
            byte[] packet1 = Encoding.Default.GetBytes(sendData);
            send_Function(packet1);
            Console.WriteLine("listviewupdate");

        }

        #region 파일사용량
        private void ProcChk_ProcessChecked(object sender, ProcessCheckedEventArgs e)
        {

            string[] End_Time = null;

            if (e.Status.ToString() == "Created")
            {
                for (int i = 0; i < prcessName.Count; i++)
                {
                    if (prcessName[i].ProcessName == e.RegisterdProcessName)
                    {
                        start_time = DateTime.Now.TimeOfDay.ToString();

                        Start_Time = start_time.Split('.');
                    }
                }
            }
            if (e.Status.ToString() == "Deleted")
            {
                for (int i = 0; i < prcessName.Count; i++)
                {
                    if (ProcessCheck.delete_count[i] == 0)
                        continue;

                    if (ProcessCheck.delete_count[i] == ProcessCheck.create_count[i])
                    {
                        if (prcessName[i].ProcessName == e.RegisterdProcessName)
                        {
                            string end_time = DateTime.Now.TimeOfDay.ToString();

                            End_Time = end_time.Split('.');

                            DateTime Start = Convert.ToDateTime(Start_Time[0].ToString());
                            DateTime End = Convert.ToDateTime(End_Time[0]);

                            TimeSpan dateDiff = End - Start;

                            string msg = dateDiff.ToString();

                            string sendData = string.Empty;
                            sendData += "CLIENT_SOFTWARE_USE_CHECK_RESULT+";
                            sendData += client_ip + "#";
                            sendData += prcessName[i].ProcessName + "#";
                            sendData += msg;

                            byte[] packeter = Encoding.Default.GetBytes(sendData);
                            send_Function(packeter);

                            ProcessCheck.create_count[i] = 0;
                            ProcessCheck.delete_count[i] = 0;

                        }
                    }
                }
            }
        }
        #endregion

        public void WriteDefaultLogin(string usr, string pwd)
        {
            //creates or opens the key provided.Be very careful while playing with 
            //windows registry.
            RegistryKey rekey = Registry.LocalMachine.CreateSubKey
                ("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon");

            if (rekey == null)
                System.Windows.Forms.MessageBox.Show
                ("There has been an error while trying to write to windows registry");
            else
            {
                //these are our hero like values here
                //simply use your RegistryKey objects SetValue method to set these keys
                rekey.SetValue("AutoAdminLogon", "1");
                rekey.SetValue("DefaultUserName", usr);
                rekey.SetValue("DefaultPassword", pwd);
            }
            //close the RegistryKey object
            rekey.Close();
        }
        public bool iswindowcheck(string packData)
        {
            bool i = false;
            SecurityIdentifier builtinAdminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);   //게스트 계정 BuiltinGuestsSid   //관리자 계정 BuiltinAdministratorsSid

            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);

            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, builtinAdminSid.Value);

            foreach (Principal p in group.Members)
            {
                if (p.Name == packData)
                {
                    Console.WriteLine("발견");
                    i = true;
                    break;
                }
                else
                {
                    i=false;
                }
            }
            return i;
        }
    
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);
    }
}
