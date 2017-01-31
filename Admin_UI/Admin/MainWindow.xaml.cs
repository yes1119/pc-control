//================================================================================================
//새로운 여정
//2016-02-03    PC정보 띄우기.
//              LOG 정보 띄우기.
//
//2016-02-05    속성 클릭시 정보 띄우기, 해당 위치 클릭시 위치 조정, 탭 위치 바뀔 시... 속성 없애기 (검색 : #region TODO : 2015-02-05 속성은 이걸로)
//                      ㄴ 강의실, 층 표시 ( 예외처리 해야한다[숫자만 들어가야하는곳... 등등])
//              층 탭 -> 강의실 탭 처럼 원하는것 추가 , 원하는것 삭제하도록.
//              탭 생성시 기존의 탭이 있다면 찾아가기.
//              TOTAL 탭 수정.
//              PC가 한대 일경우 위치 이동 적용 안되던 것 패치
//              PC 좌표 속성에서 입력시 수정.
//
//2016-02-06    PC 속성 띄울때 UI스레드를 사용해야한다.
//              PC 속성 수정 완료(IP변경은 아직 미패치, 소프트웨어 하드웨어 미적용)
//              전체 전송시 전체 프로그래스 바 표현..( 탭키를 닫으면 없어짐)
//              이미지 변경.
//              분할 영역 표시후 원하는 크기대로 조정. (TODO : 추후 원하는 크기대로 조정)
//
//2016-02-09    PC 이동시 스크롤 뷰어로 넓게 이동가능하게 설정.
//              이미지 변경.
//
//2016-02-10    속성 탭 툴팁 추가.(PC에만)
//              
//2016-02-11    패킷 데이터량 변경, 한번에 많은 양의 패킷을 받아야 할 때 가 있다.
//              PC종료 이미지시 파일 유무도 표현할 수 있도록 수정.
//              전체 제어판 제어, 전체 포맷, 전체 제어판 해제 추가
//              남궁한: Total탭 트리구조 구현 및 개선
//              영우  : 소프트웨어 하드웨어 추가,
//              패킷 데이터량 수정
//              
//2016-02-13    컨택스트 메뉴 불필요한 내용 삭제.(대화상자도 지워야한다.)
//              Floor 서버 종료가되면 관련 탭 종료
//              IP변경 시 TEXT창 정보 변경.
//              대화상자 스타일 변경 
//              PC 종료시 EXE, IMAGE를 구별할 수 있도록 이미지 추가
//              IP변경 할 상태가 아닐 시 오류 문구 추가
//              IP 변경 할 시 이미지 변경.(기존에 있었다면 없게)
//              클릭시 탭 속성 포커스
//              영역 선택시 z-index 최고 앞으로 수정
//              소프트웨어,윈도우계정(추가,삭제)리스트 수정
//
//2016-02-16    포멧중 PC켜기하면 부팅 이미지로 변경된다.
//
//2016-02-17    \a 비프음 -> +로 변경
//
//2016-02-23    윈도우 계정 변경시 이미지 추가.
//              강의실 추가시 기존에 켜져 있던 강의실에서 PC가 추가되는 현상 해결.
//              강의실 추가시 대화상자에서 TextBox로 나와있던 텍스트박스 개선
//              프로그램 종료시 쓰레드가 남아있는 현상 수정.
//              계정 대화상자 창에서 삭제를 눌렀을 시 오류나던 현상 수정.
//
//수정 사항     TODO...
//              속성에서 변경 구현 해야됨(IP)
//              컨택스트메뉴에서 필요없는것은 비활성화.
//              다운로드를 하면 가끔가다가 전체 게이지가 100퍼부터 시작하는경우가 있다. - 수정 완료
//              클라이언트 다운로드를 받을때 퍼센트를 소수점을 계산하였기 때문에 지속적으로 1% ~ 2% 사이의 소수점이 발생하게되면 지속적으로 패킷을 보낸다.
//              백업, 예약기능등을 넣어야 한다.
//              속성은 비주얼 스튜디오 처럼, 창은 트리처럼 구현.
//              PC부팅시 이미지 적용.(DB등록 power 2)
//================================================================================================
using Admin.Client;
using Admin.Type;
using Admin.UserControls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Admin
{
    public delegate void titleChange(string titleName);
    public delegate void set_Pc_Info(string floor_text, string class_text, string fl_Name);
    public delegate void add_LogData(CLogAll logData);
    public delegate void add_LogDevice(CDEVICE device);
    public delegate void add_LogSoftware(CSOFTWARE software);
    public delegate void add_LogWinDow(CWINDOW window);
  
    public delegate void Update();
    public delegate void Size_Change();
    public delegate void clear_stackpanel();
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow
    {
        //스태틱으로 리시브 함수 호출.
        public static event Size_Change SizeChange;

        public static bool[] keyPress = new bool[173];        //Key 열거형 총 개수가 173개

        System.Windows.Forms.Timer timer = null;

        CClient client = null;
        private static List<CLogAll> logs = new List<CLogAll>();

        private static WrapPanel main_Attribute;
        public static TabItem tab_Hardware1;
        public static TabItem tab_software1;
        public static TabItem tab_Attribute1;
        public static TabItem tab_Windowaccount1;
        public static SortedList<int, DispatcherTimer> event_timer = new SortedList<int, DispatcherTimer>();    //0225  
        public static int check = 1;            // 룸 컨트롤에서 PC상태 속성에 띄워주기 위한 핸들러

                                                                                     
        string check_a = null; //이거뭐야
        string check_b = null;
        public MainWindow()
        {
          
           this.InitializeComponent();

           this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
           this.Height = System.Windows.SystemParameters.PrimaryScreenHeight - 40;

           Console.WriteLine("====관리자 시작====");
          
           init();
         
           main_Attribute = attribute;
           tab_Attribute1 = tab_Attribute;

           tab_Hardware1 = tab_Hardware;
           tab_software1 = tab_software;
           tab_Windowaccount1 = tab_Windowaccount;

           Room_Control.devicelistclear = DevicelistClear;
           Room_Control.softwareclear = SoftwarelistClear;
           Room_Control.windowlistclear = WindowlistClear;
            
        }

        private void init()
        {
            try
            {
                /////////////////////////////////////////////////////////
                #region 고정 탭 메뉴의 제목 및 툴팁 입력
                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = "층         ";
                txtBlock.ToolTip = "층의 내용을 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                main_Tab.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "속성                           ";
                txtBlock.ToolTip = "속성 내용을 나타냅니다.";
                //txtBlock.TextAlignment = TextAlignment.Center;
                txtBlock.Foreground = Brushes.White;
                tab_Attribute.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "LOG                            ";
                txtBlock.ToolTip = "LOG 내용을 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                tab_Log.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "Total                          ";
                txtBlock.ToolTip = "전체 층의 정보를 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                tab_Total.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "하드웨어";
                txtBlock.ToolTip = "PC 하드웨어의 정보를 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                tab_Hardware.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "소프트웨어";
                txtBlock.ToolTip = "PC 소프트웨어의 정보를 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                tab_software.Header = txtBlock;

                txtBlock = new TextBlock();
                txtBlock.Text = "윈도우계정";
                txtBlock.ToolTip = "PC 윈도우계정 정보를 나타냅니다.";
                txtBlock.Foreground = Brushes.White;
                tab_Windowaccount.Header = txtBlock;


                /////////////////////////////////////////////////////////
                #endregion

                #region 현재 DB에 저장 되어있는 floor,pc 파악
                CClient.receive_E += control_Receive;
                #endregion

                #region 모듈 접속 클라이언트
                // client 소켓 ( 연결 시 Send 후 Receive 처리 )
                client = new CClient("61.81.99.86", 10000);
                Thread workerThread = new Thread(client.StartClient);
                workerThread.IsBackground = true;
                workerThread.Start();

                client.connectDone.WaitOne();
                #endregion

                #region 업데이트 타이머
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 300;
                timer.Tick += new EventHandler(timer_Tick_Update);
                timer.Enabled = true;
                #endregion

                #region 메인 유저컨트롤
                main_Control us = new main_Control();             // 
                CClient.receive_E += us.control_Receive;          // 서버로부터의 메세지를 받기 위한 Receive 델리게이트 추가
                MainWindow.SizeChange += us.window_SizeChanged;   // 메인 윈도우의 사이즈 변환 시 탭 컨트롤도 변환


                us.init(tab, client.Send);
                us.Background = Brushes.Black;
                us.Width = Width - 300;
                us.Height = Height - 360;
                main_Tab.Content = us;
                #endregion
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void timer_Tick_Update(object sender, EventArgs e)
        {
            main_Control us = main_Tab.Content as main_Control;
            us.timer_Tick_Update();
        }
        
        public void control_Receive(string packetType, string[] packetData)
        {
            
            if (packetType == "UPDATE")
            {
                try
                {
                    check_a = null;
                    for (int i = 0; i < packetData.Length - 1; i++)
                    {
                        string floor_text = string.Empty;
                        string class_text = string.Empty;
                        string fl_Name = string.Empty;

                        string[] floor_Data = packetData[i].Split('@');
                        string floor_Name = floor_Data[0];
                        string floor_Ip = floor_Data[1];
                        int floor_PC_Count = int.Parse(floor_Data[2]);
                        int floor_Accept_Count = int.Parse(floor_Data[3]);

                        floor_text = "    층 IP : " + floor_Ip + "\n" + "    층 총 PC 수 : " + floor_PC_Count + "\n" + "    층 접속자 수 : " + floor_Accept_Count;

                        fl_Name = floor_Name;

                        check_a += floor_text + floor_Name;

                        int floor_Rooms_Count = int.Parse(floor_Data[4]);
                        for (int k = 5; k < 5 + (floor_Rooms_Count * 3); k += 3)
                        {
                            int room_Num = int.Parse(floor_Data[k]);
                            int room_PC_Count = int.Parse(floor_Data[k + 1]);
                            int room_Accept_Count = int.Parse(floor_Data[k + 2]);
                            class_text += "    강의실 호 수 : " + room_Num + "\n" + "    강의실 PC 수 : " + room_PC_Count + "\n" + "    강의실 접속자 수 : " + room_Accept_Count + "\n\n";

                        }
                    }

                    if (check_b != check_a)
                    {
                        //클리어 델리게이트 호출
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                           new clear_stackpanel(clear));
                        check_a = null;
                        for (int i = 0; i < packetData.Length - 1; i++)
                        {
                            string floor_text = string.Empty;
                            string class_text = string.Empty;
                            string fl_Name = string.Empty;

                            string[] floor_Data = packetData[i].Split('@');
                            string floor_Name = floor_Data[0];
                            string floor_Ip = floor_Data[1];
                            int floor_PC_Count = int.Parse(floor_Data[2]);
                            int floor_Accept_Count = int.Parse(floor_Data[3]);

                            floor_text = "    층 IP : " + floor_Ip + "\n" + "    층 총 PC 수 : " + floor_PC_Count + "\n" + "    층 접속자 수 : " + floor_Accept_Count;

                            fl_Name = floor_Name;

                            check_a += floor_text + floor_Name;

                            int floor_Rooms_Count = int.Parse(floor_Data[4]);
                            for (int k = 5; k < 5 + (floor_Rooms_Count * 3); k += 3)
                            {
                                int room_Num = int.Parse(floor_Data[k]);
                                int room_PC_Count = int.Parse(floor_Data[k + 1]);
                                int room_Accept_Count = int.Parse(floor_Data[k + 2]);
                                class_text += "    강의실 호 수 : " + room_Num + "\n" + "    강의실 PC 수 : " + room_PC_Count + "\n" + "    강의실 접속자 수 : " + room_Accept_Count + "\n\n";

                            }

                            if (floor_Rooms_Count == 0)
                                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new set_Pc_Info(expander_), floor_text, "", fl_Name);
                            else
                                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new set_Pc_Info(expander_), floor_text, class_text, fl_Name);
                        }
                        check_b = check_a;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (packetType == "ADMIN_LOG")
            {
                try
                {
                    CLogAll logData = new CLogAll();

                    logData.LOG_TYPE = packetData[0];
                    logData.PC_IP = packetData[1];
                    logData.LOG_NAME = packetData[2];
                    logData.LOG_LOG = packetData[3];
                    logData.LOG_DETECTION_DAY = packetData[4];
                    logData.LOG_DETECTION_TIME = packetData[5];

                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogData(add_Log_Data), logData);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if (packetType == "ADMIN_LOG2")
            {
                try
                {
                    CLogAll logData = new CLogAll();

                    logData.PC_IP = packetData[0];
                    logData.LOG_TYPE = packetData[1];
                    logData.LOG_LOG = packetData[2];
                    logData.LOG_NAME = packetData[3];
                    logData.LOG_DETECTION_DAY = packetData[4];
                    logData.LOG_DETECTION_TIME = packetData[5];
                    //   logData.LOG_TODAY = packetData[6];
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogData(add_Log_Data2), logData);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            //TEST
            if (packetType == "DEVICE_LOG")
            {
                try
                {
                    CDEVICE device = new CDEVICE();
                    device.pIP = packetData[0];
                    device.keyboard = packetData[1];
                    device.moniter = packetData[2];
                    device.mouse = packetData[3];
                    device.time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss tt");
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogDevice(add_Log_device), device);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (packetType == "SOFTWARE_LOG")
            {
                try
                {
                    CSOFTWARE software = new CSOFTWARE();
                    software.pIP = packetData[0];
                    software.software = packetData[1];
                    software.Work = packetData[2];
                    software.time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss tt");
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogSoftware(add_Log_software), software);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

		

			
            if (packetType == "SOFTWARE_RESPOND_LOG")       // 선희
            {
                try
                {
                    CSOFTWARE software = new CSOFTWARE();
                    software.pIP = packetData[0];       //ip
                    software.software = packetData[1];  //프로그램명
                    software.Work = packetData[2];      //상태
                    software.time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss tt");

                    if (packetData[2] == "True")
                    {
                        Dialog_Get.Software.label1 = packetData[1] + "이 정상 작동합니다.";
                    }
                    else
                    {
                        Dialog_Get.Software.label1 = packetData[1] + "이 존재하지 않거나 작동하지 않습니다.";
                    }

                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogSoftware(add_Log_software_respond), software);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (packetType == "WINDOW_LOG")
            {
                try
                {
                    CWINDOW window = new CWINDOW();
                    window.Win_ip = packetData[0];
                    window.Win_account = packetData[1];
                    window.Win_PW = packetData[2];
                    window.Win_state = packetData[3];
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                            new add_LogWinDow(add_Log_window), window);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }          
        }
        
        private void add_Log_device(CDEVICE device)
        {          
            lstview.Items.Add(device);
            lstview.ScrollIntoView(lstview.Items[lstview.Items.Count - 1]);
            lstview.SelectedIndex = lstview.Items.Count - 1;
        }

        private void add_Log_software(CSOFTWARE software)
        {
            lstview2.Items.Add(software);
            lstview2.ScrollIntoView(lstview2.Items[lstview2.Items.Count - 1]);
            lstview2.SelectedIndex = lstview2.Items.Count - 1;
        }

        private void add_Log_software_respond(CSOFTWARE software)
        {
            lstview2.Items.Add(software);
            lstview2.ScrollIntoView(lstview2.Items[lstview2.Items.Count - 1]);
            lstview2.SelectedIndex = lstview2.Items.Count - 1;
        }

        private void add_Log_window(CWINDOW window)
        {
            lstview3.Items.Add(window);
            lstview3.ScrollIntoView(lstview3.Items[lstview3.Items.Count - 1]);
            lstview3.SelectedIndex = lstview3.Items.Count - 1;
        }

        private void clear()
        {
            stackpanel.Children.Clear();
        }

        private void expander_(string floor_text, string class_text, string fl_Name)
        {
            var floorExpander = new Expander { Name = "floorExpander", Header = fl_Name, Foreground = Brushes.White, Margin = new Thickness(5, 0, 0, 0) };
            var floorstackPanel = new StackPanel { Name = "floorExpanderStackPanel" };
            var floortextBlock = new TextBlock { Text = floor_text };

            var classExpander = new Expander { Name = "classExpander", Header = "강의실", Foreground = Brushes.White, Margin = new Thickness(15, 0, 0, 0) };
            var classstackPanel = new StackPanel { Name = "classExpanderStackPanel" };
            var classtextBlock = new TextBlock { Text = class_text };

            floorstackPanel.Children.Add(floortextBlock);
            floorExpander.Content = floorstackPanel;
            stackpanel.Children.Add(floorExpander);

            classstackPanel.Children.Add(classtextBlock);
            classExpander.Content = classstackPanel;
            floorstackPanel.Children.Add(classExpander);
        }

        private void add_Log_Data(CLogAll logData)
        {
            //logs.Add(logData);

            //로그정보를 넣을떄.
            lvw.Items.Add(logData);

            lvw.ScrollIntoView(lvw.Items[lvw.Items.Count - 1]);
            lvw.SelectedIndex = lvw.Items.Count-1;
            //lvw.ItemsSource = logs;

            //log.txt 파일 생성
            FileStream fs = new FileStream("log.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(logData.PC_IP.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_TYPE.ToString());
            sw.Write("/");  
            sw.Write(logData.LOG_NAME.ToString());
            sw.Write("/");         
            sw.Write(logData.LOG_LOG.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_DETECTION_DAY.ToString());
            sw.Write("/");
            sw.WriteLine(logData.LOG_DETECTION_TIME.ToString());

            sw.Close();
        }

        private void add_Log_Data2(CLogAll logData)
        {
            FileStream fs = new FileStream("log2.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            sw.Write(logData.PC_IP.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_TYPE.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_NAME.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_LOG.ToString());
            sw.Write("/");
            sw.Write(logData.LOG_DETECTION_DAY.ToString());
            sw.Write("/");
            sw.WriteLine(logData.LOG_DETECTION_TIME.ToString());
            //sw.Write("/");
            //sw.WriteLine(logData.LOG_TODAY.ToString());

            sw.Close();
        }
      
        private void addNew_Tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {            
            TabItem myNewTab = new TabItem();
            myNewTab.Header = "2층";
            tab.Items.Insert(tab.Items.CurrentPosition, myNewTab);

            UserControl us = new UserControl();
            us.Background = Brushes.Red;
            us.Width = Width - 80;
            us.Height = Height - 140;

            myNewTab.Content = us;
            myNewTab.Focus();
        }

        private void MetroWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            int converterIndex = (int)e.Key;
            keyPress[converterIndex] = e.IsDown;
        }

        private void MetroWindow_KeyUp_1(object sender, KeyEventArgs e)
        {
            int converterIndex = (int)e.Key;
            keyPress[converterIndex] = e.IsDown;
        }
 
        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        public static void attribute_Add(UIElement data)
        {
            main_Attribute.Children.Add(data);
        }
        
        public static void attribute_Clear()
        {
            main_Attribute.Children.Clear();
        }

        public static WrapPanel Get_Attribute()
        {
            return main_Attribute;
        }

        //======================================================================
        //탭 변경시 클리어.

        //분할영역 변경시.
        private void TabControl_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            double x = Width - e.NewSize.Width;
            double y = e.NewSize.Height;
            if (SizeChange != null)
                SizeChange();
            
        }

        //화면 영역 변경시
        private void TitleWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double x = e.NewSize.Width;
            double y = e.NewSize.Height;

            int _x = (int)x / 6;
            int _y = (int)y / 6;

            gridCheck.ColumnDefinitions[0].Width = new GridLength(_x*4.5);
        }

        public void DevicelistClear()
        {
            lstview.Items.Clear();
        }
        public void SoftwarelistClear()
        {
            lstview2.Items.Clear();
        }
        public void WindowlistClear()
        {
            lstview3.Items.Clear();
        }

        //======================================================================
        //탭 변경시 클리어.
        private void tab_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            attribute_Clear();
            foreach (KeyValuePair<int, DispatcherTimer> kv in MainWindow.event_timer)   //0225
            {
                if (kv.Value.IsEnabled == true) kv.Value.Stop();
            }
            check = 1;
        }

        private void TitleWindow_Closed(object sender, EventArgs e)
        {
           Thread.CurrentThread.IsBackground = true;
           
        }

        private void MenuItem_LogInfo_Click(object sender, RoutedEventArgs e)
        {
            //로그정보
            Dialog_Get.LogInfo loginfo = new Dialog_Get.LogInfo();

            FileStream fs = new FileStream("log2.txt", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string line = string.Empty;
            int r = 0, rc = 0, t = 0;            // 원격제어 사용시간 변수
            bool sw_check = false, pc_check = false, remote_check = false;
            List<CLogAll> sw_cla = new List<CLogAll>();       // 소프트웨어 사용시간
            List<CLogAll> pc_cla = new List<CLogAll>();       // PC 사용시간
          
            while ((line = sr.ReadLine()) != null)
            {
                string[] str = line.Split('/');

                if (str[1] == "소프트웨어 사용시간")
                {
                    #region 소프트웨어 사용시간
                    str[2] = str[2].Split('.')[0];

                    CLogAll cla = new CLogAll();
                    cla.PC_IP = str[0];
                    cla.LOG_TYPE = str[1];
                    cla.LOG_NAME = str[2];
                    cla.LOG_LOG = str[4];
                    cla.LOG_DETECTION_DAY = str[3];
                    cla.LOG_DETECTION_TIME = str[5];
                    sw_check = false;

                    for (int i = 0; i < sw_cla.Count; i++)
                    {
                        if (cla.LOG_NAME == sw_cla[i].LOG_NAME && cla.PC_IP == sw_cla[i].PC_IP)
                        {
                            string[] SOFT_USE = cla.LOG_DETECTION_DAY.Split(':');
                            string[] SOFT_USE2 = sw_cla[i].LOG_DETECTION_DAY.Split(':');

                            double new_time = double.Parse(SOFT_USE[0]) * 3600 + double.Parse(SOFT_USE[1]) * 60 + double.Parse(SOFT_USE[2]);
                            double original_time = double.Parse(SOFT_USE2[0]) * 3600 + double.Parse(SOFT_USE2[1]) * 60 + double.Parse(SOFT_USE2[2]);
                            double third_time = original_time + new_time;

                            int itime = (int)third_time;
                            int hours = itime / 3600;
                            int minute = itime % 3600 / 60;
                            int second = itime % 3600 % 60;
                            string time = hours.ToString() + ":" + minute.ToString() + ":" + second.ToString();

                            sw_cla[i].LOG_DETECTION_DAY = time;
                            int n = 0;
                            foreach (CLogAll cla5 in loginfo.lvw2.Items)
                            {
                                if (cla5.LOG_NAME == sw_cla[i].LOG_NAME && cla5.PC_IP == sw_cla[i].PC_IP)
                                {
                                    loginfo.lvw2.Items.RemoveAt(n);
                                    break;
                                }
                                n++;
                            }
                            loginfo.lvw2.Items.Add(sw_cla[i]);

                            sw_check = true;
                            break;
                        }
                    }

                    if (sw_check == false)
                    {
                        sw_cla.Add(cla);
                        loginfo.lvw2.Items.Add(cla);
                    }
                    #endregion
                }
                if (str[1] == "PC 사용시간")
                {
                    #region PC 사용시간
                    CLogAll cla = new CLogAll();
                    cla.PC_IP = str[0];              // IP
                    cla.LOG_TYPE = str[1];           // type_name (PC 사용시간)
                    cla.LOG_NAME = str[3];           // 층 이름 
                    cla.LOG_LOG = DateTime.Now.ToString("yyyy:MM:dd") + " " + str[2];            // 켜진 시간
                    cla.LOG_DETECTION_DAY = str[4];  // 사용 시간
                    cla.LOG_DETECTION_TIME = DateTime.Now.ToString("yyyy:MM:dd") + " " + str[5]; // 꺼진 시간
                    //  cla.LOG_TODAY = str[6];          // 최근 날짜

                    pc_check = false;

                    for (int j = 0; j < pc_cla.Count; j++)
                    {
                        if (pc_cla[j].PC_IP == cla.PC_IP)
                        {
                            string[] SOFT_USE = cla.LOG_DETECTION_DAY.Split(':');
                            string[] SOFT_USE2 = pc_cla[j].LOG_DETECTION_DAY.Split(':');

                            double original_time = double.Parse(SOFT_USE[0]) * 3600 + double.Parse(SOFT_USE[1]) * 60 + double.Parse(SOFT_USE[2]);
                            double new_time = double.Parse(SOFT_USE2[0]) * 3600 + double.Parse(SOFT_USE2[1]) * 60 + double.Parse(SOFT_USE2[2]);
                            double third_time = original_time + new_time;

                            int hours = (int)third_time / 3600;
                            int minute = (int)third_time % 3600 / 60;
                            int second = (int)third_time % 3600 % 60;
                            string time = hours.ToString() + ":" + minute.ToString() + ":" + second.ToString();

                            pc_cla[j].LOG_DETECTION_DAY = time;

                            int n = 0;
                            foreach (CLogAll cla5 in loginfo.lvw1.Items)
                            {
                                if (cla5.PC_IP == pc_cla[j].PC_IP)
                                {
                                    loginfo.lvw1.Items.RemoveAt(n);
                                    break;
                                }
                                n++;
                            }
                            loginfo.lvw1.Items.Add(pc_cla[j]);

                            pc_check = true;
                            break;
                        }
                    }

                    if (pc_check == false)
                    {
                        pc_cla.Add(cla);
                        loginfo.lvw1.Items.Add(cla);
                    }

                    #endregion
                }

              
            }
            sr.Close();
            loginfo.Show();
        }
    }
}
