//================================================
//2015-01-28            tabControl.Items.Insert(tabControl.Items.Count , myNewTab); 변경
//                      
//2015-02-02 PC 접속할때마다 UPDATE 
//2015-02-03 중첩되서 생성되는 오류 수정

using Admin.Type;
using System;
using System.Collections.Generic;
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

namespace Admin.UserControls
{
    //UI스레드를 사용하기에, 비동기식으로 접근.
    public delegate void add_FloorButton_Delegate(string floor_Name);
    public delegate void min_FloorButton_Delegate(Button delete_Btn);
    public delegate void update_FloorButton_FloorName_Delegate(Button btn, string floor_Name);
    public delegate void remove_TabControl(TabItem tabItem);

    /// <summary>
    /// main_Control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class main_Control : UserControl
    {
        //TODO : 층 추가 삭제 컨텍스트를 만들어야 한다.
        public static TabControl tabControl = null;
        private static List<CFloor> floors = new List<CFloor>();
        public Admin.Client.Send send_Data = null;
        public static event Update update;
        

        public main_Control()
        {
            InitializeComponent();
          
        }

        public void init(TabControl tabControl, Admin.Client.Send sendFun)
        {
            
            main_Control.tabControl = tabControl;
            send_Data = sendFun;
           
            //TODO : 한번이 호출이 안되서 임시적으로 추가.
            Grid_ContextMenuOpening_1(null, null);

            //층 정보 요구하기.
            Thread.Sleep(300);
            string sendData = string.Empty;
            sendData += "TOMODULE+";
            sendData += "ADMIN_FLOORS_GET+";
            sendData += "&";
            send_Data(sendData);
            
        }

        public void control_Receive(string packetType, string[] packetData)
        {
            #region 전체 층 정보 얻기
            if (packetType == "ADMIN_FLOORS_GET")
            {
                try
                {
                    int maxCount = int.Parse(packetData[0]);
                    int typeLen = 4;

                    if (floors.Count == 0)
                    {
                        for (int i = 1; i < maxCount + 1; i += typeLen)
                        {
                            CFloor floor_info = new CFloor();

                            floor_info.Floor_num = int.Parse(packetData[i]);
                            floor_info.Floor_IP = packetData[i + 1];
                            floor_info.Floor_name = packetData[i + 2];
                            floor_info.Floor_Power = int.Parse(packetData[i + 3]);
                            floor_info.sprite_Count = 0;
                            floors.Add(floor_info);

                            //비동기 식으로 접근.
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new add_FloorButton_Delegate(add_FloorButton), floor_info.Floor_name);
                        }

                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 층 추가 되었을때.
            if (packetType == "ADMIN_FLOOR_ADD")
            {
                try
                {
                    int maxCount = int.Parse(packetData[0]);
                    int typeLen = 4;

                    for (int i = 0 + 1; i < maxCount + 1; i += typeLen)
                    {
                        CFloor floor_info = new CFloor();

                        floor_info.Floor_name = packetData[i];
                        floor_info.Floor_IP = packetData[i + 1];
                        floor_info.Floor_num = int.Parse(packetData[i + 2]);
                        floor_info.Floor_Power = int.Parse(packetData[i + 3]);
                        floor_info.sprite_Count = 0;

                        int count = 0;
                        for (int k = 0; k < floors.Count; k++)
                        {
                            if (floors[k].Floor_IP == floor_info.Floor_IP)
                                count++;
                        }

                        if (count == 0)
                        {
                            floors.Add(floor_info);

                            //비동기 식으로 접근.
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new add_FloorButton_Delegate(add_FloorButton), floor_info.Floor_name);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 층 삭제 되었을때
            if (packetType == "ADMIN_FLOOR_MIN")
            {
                try
                {
                    string Floor_ip = packetData[0];

                    for (int k = 0; k < floors.Count; k++)
                    {
                        if (floors[k].Floor_IP == Floor_ip)
                        {
                            //비동기 식으로 접근.
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                new min_FloorButton_Delegate(min_FloorButton), floors[k].btn);

                            floors.Remove(floors[k]);
                            break;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 층 서버 로그인
            if (packetType == "ADMIN_FLOOR_LOGIN")
            {
                try
                {
                    string login_IP = packetData[0];
                    for (int i = 0; i < floors.Count; i++)
                    {
                        if (floors[i].Floor_IP == login_IP)
                        {
                            floors[i].Floor_Power = 1;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 층 서버 로그아웃
            if (packetType == "ADMIN_FLOOR_LOGOUT")
            {
                try
                {
                    string login_IP = packetData[0];
                    for (int i = 0; i < floors.Count; i++)
                    {
                        if (floors[i].Floor_IP == login_IP)
                        {
                            floors[i].Floor_Power = 0;
                            if (floors[i].rooms != null)
                            {
                                for (int k = 0; k < floors[i].rooms.Count; k++)
                                {
                                    if (floors[i].rooms[k].roomTab != null)
                                    {
                                        //비동기 식으로 접근.
                                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                                new remove_TabControl(romove_Tab), floors[i].rooms[k].roomTab);
                                        floors[i].rooms[k].roomTab = null;
                                    }
                                }
                            }

                            //비동기 식으로 접근.
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new remove_TabControl(romove_Tab), floors[i].floorTab);
                            floors[i].floorTab = null;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 층 이름 변경
            if (packetType == "ADMIN_FLOOR_NAME_UPDATE")
            {
                try
                {
                    string update_Result = packetData[0];
                    int floor_num = int.Parse(packetData[1]);
                    string before_Name = packetData[2];
                    string after_Name = packetData[3];
                    if (update_Result == "0") //실패
                    {
                        MessageBox.Show("변경에 실패하였습니다.");
                        WrapPanel wrapPanel = MainWindow.Get_Attribute().Children[0] as WrapPanel;
                        TextBox txtBox = wrapPanel.Children[1] as TextBox;
                        //TODO : 변경.
                        return;
                    }

                    for (int i = 0; i < floors.Count; i++)
                    {
                        if (floors[i].Floor_num == floor_num)
                        {
                            floors[i].Floor_name = after_Name;
                            Button btn = floors[i].btn;
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new update_FloorButton_FloorName_Delegate(update_FloorButton_Name), btn, after_Name);

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 업데이트
            if (packetType == "UPDATE")
            {
                try
                {
                    for (int i = 0; i < packetData.Length - 1; i++)
                    {
                        string[] floor_Data = packetData[i].Split('@');
                        string floor_Name = floor_Data[0];
                        string floor_Ip = floor_Data[1];
                        int floor_PC_Count = int.Parse(floor_Data[2]);
                        int floor_Accept_Count = int.Parse(floor_Data[3]);

                        int floor_Rooms_Count = int.Parse(floor_Data[4]);
                        for (int k = 5; k < 5 + (floor_Rooms_Count * 3); k += 3)
                        {
                            int room_Num = int.Parse(floor_Data[k]);
                            int room_PC_Count = int.Parse(floor_Data[k + 1]);
                            int room_Accept_Count = int.Parse(floor_Data[k + 2]);
                        }


                        for (int f = 0; f < floors.Count; f++)
                        {
                            if (floors[f].Floor_IP == floor_Ip)
                            {
                                floors[f].max_Computer = floor_PC_Count;
                                floors[f].accept_Computer = floor_Accept_Count;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion          
        }

        void romove_Tab(TabItem tb)
        {
            tabControl.Items.Remove(tb);
        }

        #region 업데이트.

        public void timer_Tick_Update()
        {
            image_Sprite();

            if (update != null)
                update();
        }


        #region 이미지 스프라이트
        public void image_Sprite()
        {
            int image_Select_Num;
            for (int i = 0; i < floors.Count; i++)
            {
                image_Select_Num = 0;
                if (floors[i].Floor_Power == 1) //켜져 있을 때
                {
                    double percent = ((double)floors[i].accept_Computer / (double)floors[i].max_Computer) * 100.0f;
                    image_Select_Num = 1;
                    if (percent > 0.0f && percent <= 30.0f)
                        image_Select_Num = 1;
                    if (percent > 30.0f && percent <= 60.0f)
                        image_Select_Num = 2;
                    if (percent > 60.0f && percent <= 100.0f)
                        image_Select_Num = 3;
                }
                else//꺼져있을때.
                {
                    floors[i].sprite_Count = 0;
                    floors[i].accept_Computer = 0;
                    floors[i].max_Computer = 0;
                }

                if (floors[i].image == null)
                    continue;

                floors[i].image.Source = CImage.getInstance().get_Class_Images()[image_Select_Num][floors[i].sprite_Count];
                floors[i].sprite_Count += 1;
                if (CImage.getInstance().get_Class_Images()[image_Select_Num].Count == floors[i].sprite_Count)
                    floors[i].sprite_Count = 0;
            }

        }
        #endregion

        #endregion

        #region UI스레드 접근 할 델리게이트
        private void update_FloorButton_Name(Button btn, string floor_Name)
        {
            Grid gd = btn.Content as Grid;
            TextBlock txtBlock = gd.Children[1] as TextBlock;
            txtBlock.Text = floor_Name;
        }

        private void add_FloorButton(string floor_Name)
        {
            System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
            btn.Focusable = false;
            btn.Margin = new Thickness(10, 10, 10, 10);
            btn.Height = 130;
            btn.Width = 130;

            Grid gd = new Grid();
            Image img = new Image();
            img.Margin = new Thickness(0, 0, 0, 0);
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            img.Stretch = Stretch.Fill;

            img.Source = CImage.getInstance().get_Class_Images()[0][0];
            gd.Children.Add(img);
            TextBlock tb1 = new TextBlock();
            tb1.Margin = new Thickness(10, 10, 0, 0);
            tb1.Text = floor_Name;
            gd.Children.Add(tb1);

            btn.Content = gd;

            btn.MouseDoubleClick += btn_Double_Click;
            btn.Click += btn_Click;

            int power = 0;
            btn.ContextMenu = new ContextMenu();

            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].Floor_name == floor_Name)
                    power = floors[i].Floor_Power;
            }

            MenuItem mi0 = new MenuItem();            
            if (power == 0)
            {
                mi0 = new MenuItem();
                mi0.Header = "층 제거";
                mi0.Click += contextMenu_Click_Min;
                btn.ContextMenu.Items.Add(mi0);
            }
        
            //=========================================
            //속성창
                WrapPanel mywrap = new WrapPanel();
                mywrap.Orientation = Orientation.Vertical;

                WrapPanel myItem = new WrapPanel();

                TextBlock txt1 = new TextBlock();
                txt1.Text = "층 이름\t :   ";
                txt1.Foreground = Brushes.White;
                txt1.Margin = new Thickness(1);

                TextBox txtBox = new TextBox();
                txtBox.Text = "0";
                txtBox.KeyDown += txtBox_FloorName_EnterCheck;
                txtBox.Margin = new Thickness(1);
                txtBox.Width = 150;

                myItem.Children.Add(txt1);
                myItem.Children.Add(txtBox);

                //===========================================================
                mywrap.Children.Add(myItem);
                //===========================================================
                // Add the first text cell to the Grid
                WrapPanel myItem2 = new WrapPanel();
                txt1 = new TextBlock();
                txt1.Text = "IP\t :   ";
                txt1.Foreground = Brushes.White;
                txt1.Margin = new Thickness(1);

                TextBlock txt2 = new TextBlock();
                txt2.Text = "0";
                txt2.Foreground = Brushes.White;
                txt2.Margin = new Thickness(1);

                // Add the TextBlock elements to the Grid Children collection
                myItem2.Children.Add(txt1);
                myItem2.Children.Add(txt2);

                //===========================================================
                mywrap.Children.Add(myItem2);
                //===========================================================


            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].Floor_name == floor_Name)
                {
                    floors[i].image = img;
                    floors[i].btn = btn;
                    floors[i].floor_Attribute = mywrap;
                    //floors[i].floorTab = tabItem;
                }
            }
            stp.Children.Add(btn);
        }

        
        private void min_FloorButton(Button delete_Btn)
        {
            stp.Children.Remove(delete_Btn);
        }
        #endregion

        private void btn_Double_Click(object sender, RoutedEventArgs e)
        {
            //왼쪽 버튼을 누르지 않았을경우 반환.
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;
            int selectFloor = 0;
            for (; selectFloor < floors.Count; selectFloor++)
            {
                System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
                if (floors[selectFloor].btn == btn)
                    break;
            }

            if (floors[selectFloor].image.Source == CImage.getInstance().get_Class_Images()[0][0])
            {
                //TODO : 꺼졌을 시.. 관련된 탭들 다 종료.
                MessageBox.Show("서버 PC가 꺼져 있습니다.");
                return;
            }
            if (floors[selectFloor].floorTab != null)
            {
                floors[selectFloor].floorTab.Focus();
                return;
            }

            TabItem myNewTab = new TabItem();
            myNewTab.Header = floors[selectFloor].Floor_name ;
            tabControl.Items.Insert(tabControl.Items.Count , myNewTab);

            Floor_Control fr = new Floor_Control();
            fr.Background = Brushes.Black;
            fr.Width = Width;
            fr.Height = Height;
            fr.floor_Name = floors[selectFloor].Floor_name;
            fr.floor_Num = floors[selectFloor].Floor_num;
            fr.send_Data = send_Data;
           
           Admin.Client.CClient.receive_E += fr.control_Receive;
            main_Control.update += fr.update;
            MainWindow.SizeChange += fr.window_SizeChanged;

            myNewTab.Content = fr;
            myNewTab.Focus();
            floors[selectFloor].floorTab = myNewTab;

             if(floors[selectFloor].rooms == null)
                floors[selectFloor].rooms = new List<CRoom>();

             fr.init(floors[selectFloor].rooms, floors[selectFloor].Floor_IP);
        }
        
        #region TODO : 2015-02-05 속성은 이걸로
        //====================================================================
        //클릭... 시 속성창 띄우기.
        int select_Attribute = 0;
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            int selectFloor = 0;
            for (; selectFloor < floors.Count; selectFloor++)
            {
                System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
                if (floors[selectFloor].btn == btn)
                {
                    select_Attribute = selectFloor;
                    break;
                }
            }
            //===========================================================
            MainWindow.attribute_Clear();
            MainWindow.tab_Attribute1.Focus();
            //===========================================================

            WrapPanel w1 = floors[selectFloor].floor_Attribute.Children[0] as WrapPanel;
            TextBox txtbox1 = w1.Children[1] as TextBox;
            txtbox1.Text = floors[selectFloor].Floor_name;
            WrapPanel w2 = floors[selectFloor].floor_Attribute.Children[1] as WrapPanel;
            TextBlock txtblock1 = w2.Children[1] as TextBlock;
            txtblock1.Text = floors[selectFloor].Floor_IP;
           
            MainWindow.attribute_Add(floors[selectFloor].floor_Attribute);
        }

        private void txtBox_FloorName_EnterCheck(object sender, KeyEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string before = floors[select_Attribute].Floor_name;
                Console.WriteLine("이전 데이터 : " + before + ", " + select_Attribute + "층 : " + txtBox.Text);

                //층 정보 요구하기.
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "ADMIN_FLOOR_NAME_UPDATE+";
                sendData += floors[select_Attribute].Floor_IP + "+";
                sendData += floors[select_Attribute].Floor_num + "#";
                sendData += floors[select_Attribute].Floor_name + "#";
                sendData += txtBox.Text;

                sendData += "&";
                send_Data(sendData);
            }
        }
        //====================================================================
        #endregion
     
        public static bool close_Tab(TabItem tabItem)
        {
            //층
            if (tabItem.Content == "Admin.UserControls.Room_Control")
                MainWindow.event_timer.Remove(int.Parse(tabItem.Header.ToString().Split('호')[0]));			//0224
            for (int selectFloor = 0; selectFloor < floors.Count; selectFloor++)
            {
                if (floors[selectFloor].floorTab == tabItem)
                {
                    // 여기에다가 room 윈도우 다 제거하면 됨..
                    Admin.Client.CClient.receive_E -= ((Floor_Control)(floors[selectFloor].floorTab.Content)).control_Receive;
                    main_Control.update -= ((Floor_Control)(floors[selectFloor].floorTab.Content)).update;
                    MainWindow.SizeChange -= ((Floor_Control)(floors[selectFloor].floorTab.Content)).window_SizeChanged;
                    //floors[selectFloor].rooms.Clear();
                    //floors[selectFloor].rooms = null;

                    floors[selectFloor].floorTab = null;

                    //탭이 닫히면 포커스되는 위치.
                    int index = tabControl.Items.IndexOf(tabItem) ;

                    if (index - 1 <= 0)
                        index = 0;

                    ((TabItem)tabControl.Items.GetItemAt(index)).Focus();
                    return true;
                }
            }

            //강의실
            for (int selectFloor = 0; selectFloor < floors.Count; selectFloor++)
            {
                if (floors[selectFloor].rooms != null)
                {
                    for (int selectRoom = 0; selectRoom < floors[selectFloor].rooms.Count; selectRoom++)
                    {
                        if (floors[selectFloor].rooms[selectRoom].roomTab == tabItem)
                        {
                          
                            Admin.Client.CClient.receive_E -= ((Room_Control)(tabItem.Content)).control_Receive;
                            main_Control.update -= ((Room_Control)(tabItem.Content)).update;
                            MainWindow.SizeChange -= ((Room_Control)(tabItem.Content)).window_SizeChanged;
                            //((Room_Control)(floors[selectFloor].rooms[selectRoom].roomTab.Content)).coms.Clear();
                            floors[selectFloor].rooms[selectRoom].roomTab = null;

                            //탭이 닫히면 포커스되는 위치.
                            int index = tabControl.Items.IndexOf(tabItem) ;

                            if (index - 1 <= 0)
                                index = 0;

                            ((TabItem)tabControl.Items.GetItemAt(index)).Focus();
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        #region 버튼 컨텍스트메뉴
        private void contextMenu_Click_Add(object sender, RoutedEventArgs e)
        {
            Admin.Dialog_Get.AddFloor addPc = new Admin.Dialog_Get.AddFloor();
            addPc.ShowDialog();
            if (addPc.window_close == true)
            {
                for (int k = 0; k < floors.Count; k++)
                {
                    if (floors[k].Floor_IP == addPc.ip.Text)
                    {
                        MessageBox.Show("동일아이피에 서버가 존재합니다(중복생성 불가).");
                        return;
                    }
                }
                string sendData = string.Empty;
                sendData += "TOMODULE+";
                sendData += "ADMIN_FLOOR_ADD+"; //모듈에서 메세지를 받고 db생성처리해야함
                sendData += addPc.name.Text + "#";
                sendData += addPc.ip.Text;
                sendData += "&";
                send_Data(sendData);
            }
        }       

        private void contextMenu_Click_Min(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null)
                return;

            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].btn.ContextMenu.Items.Count == 0) continue;
                if (item == floors[i].btn.ContextMenu.Items.GetItemAt(0) as MenuItem)
                {
                    Console.WriteLine(floors[i].Floor_name);
                    string sendData = string.Empty;
                    sendData += "TOMODULE+";
                    sendData += "ADMIN_FLOOR_MIN+";
                    sendData += floors[i].Floor_IP;
                    sendData += "&";
                    send_Data(sendData);
                }
            }
        }

        #endregion

        private void Grid_ContextMenuOpening_1(object sender, ContextMenuEventArgs e)
        {
            main_Grid.ContextMenu = new ContextMenu();
            MenuItem mi0 = new MenuItem();
            mi0.Header = "층 추가";
            mi0.Click += contextMenu_Click_Add;
            main_Grid.ContextMenu.Items.Add(mi0);
        }

        public void window_SizeChanged()
        {
            Height = tabControl.Width;
            Width = tabControl.Height;
        }
    }
}
