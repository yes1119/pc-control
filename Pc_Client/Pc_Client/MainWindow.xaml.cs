//============================
//20150202 수정
//2015-02-03    전송 실패시 패킷 전송 수정.
//2015-02-04    서버 전송 실패시 스레드 종료.
//2015-02-17    는 비프음 때문에 +로 변경
using Microsoft.Win32;
using Pc_Client.SendData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Pc_Client
{
    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 멤버

        static int flag = 0;
        private bool Remote_Check = false;
        private bool Event_Checker = false;

        static mySocket sock ;
        static Remote_Client remote_sock;
        public static string software;

        static CControl control;


        string myip;
       
        public DispatcherTimer ScreenSendThread;    //화면 전송에 쓰일 타이머
        public DispatcherTimer EventCheckThread;    //화면 전송에 쓰일 타이머

        public delegate void remote_Control(byte[] data);
        public delegate void key_Control();
        public delegate void remote_Start();
        public delegate void checker_Start();           

        private int hooking_count = 0;
        private int compare_count = 600000;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
          
            #region 관리자 권한 실행 여부 확인 후 레지스트리 경로 추가
            if (IsAdministrator() == false)
            {
                //관리자 권한이 아닐 시에 실행했던 프로세스를 관리자로 재 실행
                try
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo();
                    // 프로세스를 시작할 때 운영체제 셀을 사용할지 여부
                    procInfo.UseShellExecute = true;
                    // 실행 할 파일명 
                    procInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
                    // 시작할 프로세스가 포함된 디렉터리를 가져오거나 설정합니다.
                    procInfo.WorkingDirectory = Environment.CurrentDirectory;
                    // FileName 속성이 지정한 응용 프로그램이나 문서를 열 때 사용할 동사를 가져오거나 설정합니다.
                    // runas = 관리자 권한
                    procInfo.Verb = "runas";
                    //관리자 권한이 아닐 시 프로세스 재 시작.
                    Process.Start(procInfo);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                }
                this.Close();
            }
           
            if (IsAdministrator() == true)
            {
                try
                {
                    flag = 1;
                    //레지스트리 경로 얻기.
                    Microsoft.Win32.RegistryKey key1 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE");
                    Microsoft.Win32.RegistryKey key2 = key1.OpenSubKey(@"Microsoft");
                    Microsoft.Win32.RegistryKey key3 = key2.OpenSubKey(@"Windows");
                    Microsoft.Win32.RegistryKey key4 = key3.OpenSubKey(@"CurrentVersion");
                    Microsoft.Win32.RegistryKey key5 = key4.OpenSubKey(@"Run", true);

                    object str = key5.GetValue("MoonSeokJin");
                    if (str == null)
                        Console.WriteLine(" 없음");
                    key5.SetValue("MoonSeokJin", System.Windows.Forms.Application.ExecutablePath.ToString(), RegistryValueKind.String);
                    // MessageBox.Show(key5.GetValue("choiyoungwoo").ToString());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            #endregion
            //방화벽 체크후 해제.
            CFireWall.FireWallCheck();
            //기본셋팅
            Funtion.SetInfo();
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            myip=IPHost.AddressList[0].ToString(); // 내 아이피 가져오기
            // IP 셋팅
            Window1 winMain = new Window1();

            ManagementClass cls = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection instances = cls.GetInstances();

            foreach (ManagementObject instance in instances)
            {
                foreach (PropertyData prop in instance.Properties)
                {
                    if (prop.Name == "Caption")
                    {
                        //Console.WriteLine(string.Format("WINDOW : {0}", prop.Value));
                        software = prop.Value.ToString();
                    }
                }
            }

            #region 시작 프로그램 등록
            #region 시작 프로그램 등록
            ////TODO : 관리자에서 윈도우계정 패킷받아서 경로 지정해줘야됨....
            //string MMPS_Application_FolderPath = Environment.CurrentDirectory + @"/Pc_Client.exe";    // 링크해줄 원본파일 위치
            ////string MMPS_DiskTop_Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);    // 시작프로그램 폴더 위치
            //string MMPS_DiskTop_Path = "C:\\Users\\aa\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";    // 시작프로그램 폴더 위치
            //string Application_ShortCut_Name = "Power_Client";                                             // 시작프로그램에 등록될 이름

            ////[참조] - C:/Windows/System/Shell32.dll [추가]
            ////         추가 후 -> Shell32 -> 속성 -> Interop 형식 포함 false로
            //Shell32.Shell ShellClass_iNstance = new Shell32.ShellClass();                               // shell32등록
            //System.IO.StreamWriter StreamWriter_iNstance = new System.IO.StreamWriter(MMPS_DiskTop_Path + @"/" + Application_ShortCut_Name + ".lnk", false);    // 등록옵션

            //StreamWriter_iNstance.Close();

            //Shell32.Folder DeskTop_Folder = ShellClass_iNstance.NameSpace(MMPS_DiskTop_Path);
            //Shell32.FolderItem DeskTop_FolderiTem = DeskTop_Folder.Items().Item(Application_ShortCut_Name + ".lnk");
            //Shell32.ShellLinkObject ShortCut_Link = (Shell32.ShellLinkObject)DeskTop_FolderiTem.GetLink;

            //ShortCut_Link.Path = MMPS_Application_FolderPath;
            //ShortCut_Link.Description = "사용자 응용 프로그램";
            //ShortCut_Link.WorkingDirectory = Environment.CurrentDirectory;
            //ShortCut_Link.SetIconLocation(Environment.CurrentDirectory, 0);
            //ShortCut_Link.Save();
            #region 레지스트리시작등록
            //string keyName = "MyProgramName";
            ////TODO : 경로 설정 해줘야됨.
            //string assemblyLocation = "D:\\Pc_Client - Ver_07.01-9\\Pc_Client\\bin\\Debug\\Pc_Client.exe"; // Or the EXE path.

            //// Set Auto-start.
            //Util.SetAutoStart(keyName, assemblyLocation);

            //if (Util.IsAutoStartEnabled(keyName, assemblyLocation))
            //    Util.UnSetAutoStart(keyName);
            //else
            //    Util.SetAutoStart(keyName, assemblyLocation);
            #endregion
            #endregion
            #endregion

            #region Floor서버 접속 실행
            if (flag == 1)
            {
                try
                {
                    sock = new mySocket();
                    control = new CControl(key_start);
                    sock.Start(Window1.Connectip, 8888, control.control_Receive);   //connectip는 서버의 IP
                    HookRegister();

                    ScreenSendThread = new DispatcherTimer();
                    ScreenSendThread.Interval = new TimeSpan(0, 0, 0, 0, 1);
                    ScreenSendThread.Tick += new EventHandler(thread_func);

                    EventCheckThread = new DispatcherTimer();
                    EventCheckThread.Interval = new TimeSpan(0, 0, 0, 1, 0);
                    EventCheckThread.Tick += new EventHandler(CheckStart);


                    control.remote_start = start_receive;
                    control.Check_start = start_check;

                    remote_sock = new Remote_Client(receive);


                    this.Hide();
                    control.trayIconShow();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
           
            #endregion

            #region ip변경됬는지 확인
            if (Window1.Change_ip != "없음")
            {
                try
                {
                    Funtion fun = new Funtion();
                    IPHostEntry IPHost1 = Dns.GetHostByName(Dns.GetHostName());
                    string myIP1 = IPHost1.AddressList[0].ToString();
                    string sendData1 = string.Empty;
                    sendData1 += "CLIENT_IPCHANGESTATE+";
                    sendData1 += Window1.Change_ip + "#";
                    sendData1 += myIP1 + "#";
                    sendData1 += fun.GetMacAddress().ToString() + "#";
                    sendData1 += "TRUE";
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);
                    CControl.send_Function(packet1);



                    FileInfo client_change_ip_txt = new FileInfo(Window1.change_ip);
                    FileStream client_change_ip_txt_stream = client_change_ip_txt.Create();
                    TextWriter tw3 = new StreamWriter(client_change_ip_txt_stream, Encoding.UTF8);
                    tw3.Write("없음");
                    tw3.Close();
                    //======================================
                    client_change_ip_txt_stream.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
            #endregion

            //가지고 있는 파일들을 Send
            #region 파일전송하기 위한 서버 실행
            SendData.AsynchronousSocketListener.port = 8500;
            Thread listener = new Thread(new ThreadStart(AsynchronousSocketListener.StartListening));
            listener.IsBackground = true;
            listener.Start();
            #endregion
         
            //Window1.Connectip == 서버 IP
        }
        #region Chung
        private void HookRegister()
        {
            //HookManager.MouseMove += HookManager_MouseMove;
            //HookManager.MouseClick += HookManager_MouseClick;
            //HookManager.MouseUp += HookManager_MouseUp;
            //HookManager.MouseDown += HookManager_MouseDown;
            //HookManager.MouseDoubleClick += HookManager_MouseDoubleClick;
            //HookManager.MouseWheel += HookManager_MouseWheel;
            //HookManager.KeyDown += HookManager_KeyDown;
            //HookManager.KeyUp += HookManager_KeyUp;
            //HookManager.KeyPress += HookManager_KeyPress;
        }

        #region Hooking 이벤트

        private void HookManager_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            hooking_count += 3;
             }

        private void HookManager_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            hooking_count += 3;
        }      

        private void HookManager_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            hooking_count += 3;
        }

        private void HookManager_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 1;
        }

        private void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 1;
              }


        private void HookManager_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 1;
          }


        private void HookManager_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 1;
          }


        private void HookManager_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 3;       
        }


        private void HookManager_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            hooking_count += 1;
           }

        #endregion

        public void key_start()
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new key_Control(ReceiveKeyControl));
        }

           #region 리시브함수
        // 동작 메시지
        public void receive(Socket s, byte[] data)
        {
            string temp = Encoding.Default.GetString(data);
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new remote_Control(ReceiveRemoteControl), data);
        }
              
        // 실행 메시지
        public void start_receive()
        {
            Remote_Check = true;
            remote_sock.Start(control.Remoteip, 4312);
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new remote_Start(RemoteStart));
        }
        
        public void start_check()
        {
            Event_Checker = true;
            EventCheckThread.Start();
         //   this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new checker_Start(CheckStart));
        }
        #endregion
        #region 원격제어 시작 콜백 함수
        public void RemoteStart()
        {
            ScreenSendThread.Start();
        }
        #endregion
        #region 이벤트 체크 콜백 함수 
        public void CheckStart(object sender, EventArgs e)
        {
            //while (Event_Checker)
            if (Event_Checker==true && sock.Check())
            {
                if (hooking_count >= 50)
                {
                    
                    string sendData1 = string.Empty;
                    sendData1 += "EVENT_CHECK_RESET+";
                    sendData1 += myip + "#";
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);

                    sock.SendData(packet1);

                    hooking_count = 0;
                    compare_count = 10000;
                }
                if (EventChecker.GetIdleTime() > compare_count)
                {
                    string sendData1 = string.Empty;
                    sendData1 += "EVENT_CHECK+";
                    sendData1 += myip + "#";
                    byte[] packet1 = Encoding.Default.GetBytes(sendData1);

                    sock.SendData(packet1);

                    hooking_count = 0;
                    compare_count += 10000;
                }
            }
        }
        #endregion
        #region 키보드&마우스 수신 콜백 함수
        public void ReceiveRemoteControl(byte[] data)
        {
            try
            {
                string temp = Encoding.Default.GetString(data);
                string[] str = temp.Split('+');
                string[] packetData = str[1].Split('#');

                int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;     //모니터의 넓이
                int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;   //모니터의 높이

                try
                {
                    int x = (int)float.Parse(packetData[0]);                           //마우스 x좌표
                    int y = int.Parse(packetData[1]);                                  //마우스 y좌표
                    double num1 = (((float)width) / float.Parse(packetData[3])) * x;   //상대방 x좌표 - 원격 제어 하는 쪽의 넓이,마우스 x좌표와 상대방의 모니터
                    //넓이를 계산하여 해상도가 다르더라도 마우스 위치가 일치하게 해준다
                    double num2 = (((float)height) / float.Parse(packetData[2])) * y;  //상대방 y좌표 - 원격 제어 하는 쪽의 넓이,y좌표와 상대방의 모니터
                    //높이를 계산하여 해상도가 다르더라도 마우스 위치가 일치하게 해준다
                    System.Windows.Point pt = new System.Windows.Point((float)num1, (float)num2);   //최종적으로 연산한 값을 Point객체에 담는다.


                    switch (str[0])
                    {
                        case "MOUSEMOVE":
                            SetCursorPos((int)pt.X, (int)pt.Y);
                            break;
                        case "MOUSELDOWN":
                            SetCursorPos((int)pt.X, (int)pt.Y);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            break;
                        case "MOUSELUP":
                            SetCursorPos((int)pt.X, (int)pt.Y);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            break;
                        case "MOUSERDOWN":
                            SetCursorPos((int)pt.X, (int)pt.Y);
                            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            break;
                        case "MOUSEWHEEL":
                            SetCursorPos((int)pt.X, (int)pt.Y);
                            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, int.Parse(packetData[4]), 0);
                            break;

                    }
                }
                catch
                {
                    switch (str[0])
                    {
                        case "KEYDOWN":
                            int num10 = int.Parse(packetData[1]);
                            if (num10 == 19)
                            {
                                keybd_event(0x15, 0, 0, IntPtr.Zero);
                                keybd_event(0x15, 0, 2, IntPtr.Zero);
                            }
                            if (packetData[0] == "SHIFT")
                            {
                                keybd_event(0x10, 0, 0, IntPtr.Zero);               //0이면 누름
                                keybd_event((byte)num10, 0, 0, IntPtr.Zero);
                                keybd_event((byte)num10, 0, 2, IntPtr.Zero);        //2면 뗌
                                keybd_event(0x10, 0, 2, IntPtr.Zero);
                            }
                            else if (packetData[0] == "CTRL")
                            {
                                keybd_event(0x11, 0, 0, IntPtr.Zero);               //0이면 누름
                                keybd_event((byte)num10, 0, 0, IntPtr.Zero);
                                keybd_event((byte)num10, 0, 2, IntPtr.Zero);        //2면 뗌
                                keybd_event(0x11, 0, 2, IntPtr.Zero);
                            }
                            else if (packetData[0] == "ALT")
                            {
                                keybd_event(0x12, 0, 0, IntPtr.Zero);               //0이면 누름
                                keybd_event((byte)num10, 0, 0, IntPtr.Zero);
                                keybd_event((byte)num10, 0, 2, IntPtr.Zero);        //2면 뗌
                                keybd_event(0x12, 0, 2, IntPtr.Zero);
                            }
                            else
                            {
                                keybd_event((byte)num10, 0, 0, IntPtr.Zero);
                                keybd_event((byte)num10, 0, 2, IntPtr.Zero);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show(ex.Message, "키보드&마우스 수신 콜백 함수");
            }
        }
        
        #endregion

        public void ReceiveKeyControl()
        {
            string keyname = string.Empty;
            for (int i = 0; i < CControl.kls.Count; i++)
            {
                string[] v_str = CControl.kls[i].Key_Value.Split('/');
                string[] s_str = CControl.kls[i].Key_Sleep.Split('/');

                for (int j = 0; j < v_str.Length - 1; j++)
                {
                    int num10 = int.Parse(v_str[j]);

                    keybd_event((byte)num10, 0, 0, IntPtr.Zero);
                    keybd_event((byte)num10, 0, 2, IntPtr.Zero);

                    if (int.Parse(s_str[j]) == 0) s_str[j] = "1";
                    Thread.Sleep((int.Parse(s_str[j])) * 1000);
                }
                keyname = CControl.kls[i].Key_Name;
                CControl.kls.Remove(CControl.kls[i]);
            }

            string sendDataer = string.Empty;
            sendDataer += "CLIENT_KEY_COMPLETE+";    //패킷 타입.
            sendDataer += myip + "#";
            byte[] packeter = Encoding.Default.GetBytes(sendDataer);
            sock.SendData(packeter);

            Console.WriteLine(keyname + " 키 이벤트 완료");
        }

        #region 화면 전송 타이머
        private void thread_func(object sender, EventArgs e)  //개인화면이 뜬 순간부터 완전히 특정 수업에 들어온것이기 때문에 여기서 부터 
        {                           //원격 화면이 가능하게 함
            try
            {
                if (Remote_Check == true && remote_sock.Check())
                {
                    // 주화면의 크기 정보 읽기
                    System.Drawing.Rectangle rect = Screen.PrimaryScreen.Bounds;
                    // 2nd screen = Screen.AllScreens[1]

                    // 픽셀 포맷 정보 얻기 (Optional)
                    int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
                    System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    if (bitsPerPixel <= 16)
                    {
                        pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                    }
                    if (bitsPerPixel == 24)
                    {
                        pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                    }

                    Bitmap flag = new Bitmap(rect.Width, rect.Height, pixelFormat);
                    Graphics flagGraphics = Graphics.FromImage(flag);

                    using (Graphics gr = Graphics.FromImage(flag))
                    {
                        // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                        gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                    }
                    Thread.Sleep(7);
                    MemoryStream ms = new MemoryStream();
                    flag.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    byte[] data = ms.ToArray();

                    remote_sock.SendData(data);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "화면 전송 타이머");
                // Console.WriteLine("연결 끊킴");
                // remote_sock.Start(control.Remoteip, 4312);
            }
        }
        #endregion
     
      
        #endregion
        //연결 체크
        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("10초");

            string sendData1 = string.Empty;
            sendData1 += "TEST+";
            sendData1 += "CONNECT_CHECK+";
            sendData1 += "test" + "#";
            byte[] packet1 = Encoding.Default.GetBytes(sendData1);

            try
            {
                if (!sock.Check())
                    return;

                sock.SendData(packet1);
            }
            catch (Exception)
            {
                Console.WriteLine("연결 끊킴");
                sock.Start(Window1.Connectip, 8888, control.control_Receive);
            }

        }
       
        //관리자 권한 실행 체크
        public bool IsAdministrator()
        {
            // 현재 사용자 정보 얻기
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                // principal이 관리자 계정인지 확인한다.
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }
        #region 전역 이벤트 API 함수
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();
        #endregion

        #region 원격제어 API 함수
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("USER32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, int dwExtraInfo);

        [DllImport("USER32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("USER32.dll")]
        public static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        private const int SM_CYCAPTION = 4;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        #endregion

    }
}
