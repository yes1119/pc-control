using Admin.Client;
using Admin.Type;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
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
    public delegate void add_RoomButton_Delegate(CRoom room);
    public delegate void min_RoomButton_Delegate(CRoom room);
    public delegate void update_RoomButton_Delegate(Button btn,int index, string room_Num);
    /// <summary>
    /// Floor_Control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Floor_Control : UserControl
    {
        public string floor_Name = string.Empty;   // == floor_Name
        public int floor_Num = 0;   // == floor_Name
        string f_ip = string.Empty;   // == 플로어아이피
        List<CRoom> rooms;

        public Send send_Data = null;

        public Floor_Control()
        {
            InitializeComponent();
        }

        public void init(List<CRoom> _rooms, string ip)
        {
            rooms = _rooms;
            f_ip = ip;

            //서버 보내기.
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "ADMIN_ROOMS_GET+";
            sendData += f_ip + "+";
            sendData += floor_Num.ToString() + "#";
            sendData += floor_Name.ToString();
            sendData += "&";
            send_Data(sendData);
        }

        #region 업데이트
        public void update()
        {
            Image_Sprite();
        }

        #region 이미지 스프라이트.
        public void Image_Sprite()
        {
            int image_Select_Num = 0;       //이미지가 바뀐다

            for (int i = 0; i < rooms.Count; i++)
            {
                image_Select_Num = 0;

                double percent = ((double)rooms[i].accept_Computer / (double)rooms[i].max_Computer) * 100.0f;
                if (percent > 0.0f && percent <= 30.0f)
                    image_Select_Num = 1;
                else if (percent > 30.0f && percent <= 60.0f)
                    image_Select_Num = 2;
                else if (percent > 60.0f && percent <= 100.0f)
                    image_Select_Num = 3;
                else
                    rooms[i].sprite_Count = 0;

                if (rooms[i].image == null)
                    continue;

                rooms[i].image.Source = CImage.getInstance().get_Class_Images()[image_Select_Num][rooms[i].sprite_Count];
                rooms[i].sprite_Count += 1;
                if (CImage.getInstance().get_Class_Images()[image_Select_Num].Count == rooms[i].sprite_Count)
                    rooms[i].sprite_Count = 0;
            }
        }
        #endregion

        #endregion

        #region 리시브 함수
        public void control_Receive(string packetType, string[] packetData)
        {
            #region 강의실 정보 얻기
            if (packetType == "ADMIN_ROOMS_GET")
            {
                try
                {
                    int typeLen = 3;
                    int floor_Num = int.Parse(packetData[0]);
                    int maxCount = int.Parse(packetData[1]);

                    if (this.f_ip != packetData[2])
                        return;

                    if (rooms.Count != 0)
                    {
                        for (int i = 0; i < rooms.Count; i++)
                        {
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                               new min_RoomButton_Delegate(min_RoomButton), rooms[i]);
                        }

                    }

                    for (int i = 3; i < (maxCount) + 1; i += typeLen)
                    {

                        CRoom room_info = new CRoom();
                        room_info.Floor_num = int.Parse(packetData[i]);
                        room_info.Room_num = int.Parse(packetData[i + 1]);
                        room_info.Room_name = packetData[i + 2];
                        rooms.Add(room_info);

                        //비동기 식으로 접근.
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                              new add_RoomButton_Delegate(add_RoomButton), room_info);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }

            }
            #endregion

            #region 강의실 추가
            if (packetType == "ADMIN_ROOM_ADD")
            {
                try
                {
                    int typeLen = 2;
                    int floor_Num = int.Parse(packetData[0]);
                    int maxCount = int.Parse(packetData[1]);

                    if (this.floor_Num != floor_Num)
                        return;

                    for (int i = 2; i < (maxCount) + 1; i += typeLen)
                    {
                        int count = 0;
                        string room_Num = packetData[i + 1];
                        for (int j = 0; j < rooms.Count; j++)
                        {
                            //TODO :  Room_num 가 중복되는경우 마지막것으로 출력된다.
                            if (rooms[j].Room_num.ToString() == room_Num)
                                count++;
                        }

                        if (count != 0)
                            continue;

                        CRoom room_info = new CRoom();

                        room_info.Floor_num = int.Parse(packetData[0]);
                        room_info.Room_name = packetData[i];
                        room_info.Room_num = int.Parse(packetData[i + 1]);
                        rooms.Add(room_info);

                        //비동기 식으로 접근.
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                              new add_RoomButton_Delegate(add_RoomButton), room_info);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 강의실 삭제.
            if (packetType == "ADMIN_ROOM_MIN")
            {
                try
                {
                    int typeLen = 2;
                    int floor_Num = int.Parse(packetData[0]);
                    int maxCount = int.Parse(packetData[1]);

                    if (this.floor_Num != floor_Num)
                        return;

                    for (int i = 2; i < (maxCount) + 1; i += typeLen)
                    {
                        CRoom room_info = new CRoom();

                        room_info.Room_num = int.Parse(packetData[i]);
                        room_info.Room_name = packetData[i + 1];

                        for (int j = 0; j < rooms.Count; j++)
                        {
                            //TODO :  Room_num 가 중복되는경우 마지막것으로 출력된다.
                            if (rooms[j].Room_num == room_info.Room_num)
                            {
                                //비동기 식으로 접근.
                                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                            new min_RoomButton_Delegate(min_RoomButton), room_info);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 강의실 번호 수정
            if (packetType == "ADMIN_ROOM_NUM_UPDATE")
            {
                try
                {
                    string update_Result = packetData[0];
                    string room_Num = packetData[2];
                    string after_Num = packetData[3];
                    if (update_Result == "0") //실패
                    {
                        MessageBox.Show("변경에 실패하였습니다.");
                        WrapPanel wrapPanel = MainWindow.Get_Attribute().Children[0] as WrapPanel;
                        TextBox txtBox = wrapPanel.Children[1] as TextBox;
                        //TODO : 추후 설정.
                        return;
                    }

                    for (int i = 0; i < rooms.Count; i++)
                    {
                        if (rooms[i].Room_num == int.Parse(room_Num))
                        {
                            rooms[i].Room_num = int.Parse(after_Num);
                            Button btn = rooms[i].btn;
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new update_RoomButton_Delegate(update_RoomButton), btn, 1, after_Num);

                            break;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

            #region 강의실 이름 수정
            if (packetType == "ADMIN_ROOM_NAME_UPDATE")
            {
                try
                {
                    string update_Result = packetData[0];
                    int room_Num = int.Parse(packetData[1]);
                    string room_Name = packetData[2];
                    string after_Name = packetData[3];
                    if (update_Result == "0") //실패
                    {
                        MessageBox.Show("변경에 실패하였습니다.");
                        WrapPanel wrapPanel = MainWindow.Get_Attribute().Children[0] as WrapPanel;
                        TextBox txtBox = wrapPanel.Children[1] as TextBox;
                        //TODO : 추후 설정.
                        return;
                    }

                    for (int i = 0; i < rooms.Count; i++)
                    {
                        if (rooms[i].Room_num == room_Num)
                        {
                            rooms[i].Room_name = after_Name;
                            Button btn = rooms[i].btn;
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                                    new update_RoomButton_Delegate(update_RoomButton), btn, 2, after_Name);

                            break;
                        }
                    }
                }
                catch(Exception ex)
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

                            for (int f = 0; f < rooms.Count; f++)
                            {
                                if (rooms[f].Room_num == room_Num)
                                {
                                    rooms[f].max_Computer = room_PC_Count;
                                    rooms[f].accept_Computer = room_Accept_Count;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(packetType + " : " + ex.Message);
                }
            }
            #endregion

        }
        #endregion

        #region 델리게이트
        private void min_RoomButton(CRoom room)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Room_num == room.Room_num)
                {
                    stp.Children.Remove(rooms[i].btn);
                    rooms.Remove(rooms[i]);
                    return;
                }
            }
        }

        private void add_RoomButton(CRoom room)
        {
            Button btn = new Button();
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
            tb1.Text = room.Room_num.ToString();
            gd.Children.Add(tb1);
            TextBlock tb2 = new TextBlock();
            tb2.Margin = new Thickness(90, 100, 0, 0);
            tb2.Text = room.Room_name;
            gd.Children.Add(tb2);

            btn.Content = gd;

            btn.Click += Button_Click_1;
            btn.MouseDoubleClick += Button_Double_Click;
            btn.MouseRightButtonDown += btn_Right_Click;

            stp.Children.Add(btn);

            //===================================================================
            //속성창 생성
            //===========================================================
                WrapPanel mywrap = new WrapPanel();
                mywrap.Orientation = Orientation.Vertical;

                WrapPanel myGrid = new WrapPanel();

                TextBlock txt1 = new TextBlock();
                txt1.Text = "강의실 호수\t :   ";
                txt1.Foreground = Brushes.White;
                txt1.Margin = new Thickness(1);

                TextBox txtBox = new TextBox();
                txtBox.Text = "";
                txtBox.KeyDown += txtBox_RoomNum_EnterCheck;
                txtBox.Margin = new Thickness(1);
                txtBox.Width = 100;

                myGrid.Children.Add(txt1);
                myGrid.Children.Add(txtBox);

                //===========================================================
                mywrap.Children.Add(myGrid);
                //===========================================================
                // Add the first text cell to the Grid
                WrapPanel test = new WrapPanel();
                txt1 = new TextBlock();
                txt1.Text = "강의실 이름\t :   ";
                txt1.Foreground = Brushes.White;
                txt1.Margin = new Thickness(1);

                txtBox = new TextBox();
                txtBox.Text = "";
                txtBox.KeyDown += txtBox_RoomName_EnterCheck;
                txtBox.Margin = new Thickness(1);
                txtBox.Width = 100;

                // Add the TextBlock elements to the Grid Children collection
                test.Children.Add(txt1);
                test.Children.Add(txtBox);
                //===========================================================
                mywrap.Children.Add(test);
                //===========================================================
            //===================================================================

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].Room_num == room.Room_num)
                {
                    rooms[i].btn = btn;
                    rooms[i].image = img;
                    rooms[i].room_Attribute = mywrap;
                    break;
                }
            }
        }

        private void update_RoomButton(Button btn, int index, string room_Num)
        {
            Grid gd = btn.Content as Grid;
            TextBlock txtBlock = gd.Children[index] as TextBlock;
            txtBlock.Text = room_Num;
        }
        #endregion

        #region 버튼 클릭
        //우
        private void btn_Right_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;
            btn.ContextMenu = null;           //제거

            ContextMenu contextMenu = new ContextMenu();
            //시스템지점
            MenuItem mi1 = new MenuItem();
            mi1.Header = "PC 초기설정";
            mi1.Click += contextMenu_Click_Pc_Set;
            contextMenu.Items.Add(mi1);
            btn.ContextMenu = contextMenu;

            MenuItem mi0 = new MenuItem();
            mi0.Header = "삭제";
            mi0.Click += contextMenu_Click_Delete;
            contextMenu.Items.Add(mi0);
            btn.ContextMenu = contextMenu;
        }

        //더블 클릭 열기
        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (but == null)
                return;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].btn == but)
                {
                    if (rooms[i].roomTab != null)
                    {
                        rooms[i].roomTab.Focus();
                        return;
                    }

                    TabItem myNewTab = new TabItem();
                    myNewTab.Header = rooms[i].Room_num + "호실"; //0225
                    main_Control.tabControl.Items.Insert(main_Control.tabControl.Items.Count, myNewTab);


                    Room_Control room = new Room_Control();
                    room.Background = Brushes.Black;
                    room.Width = Width;
                    room.Height = Height;
                    room.scroolViewer.Width = Width;
                    room.scroolViewer.Height = Height - 24;
           
                    room.send_Data = send_Data;
                    room.room_Num = rooms[i].Room_num;
                    Room_Control.room_Num1 = room.room_Num;
                    room.room_Name = rooms[i].Room_name;
                    CClient.receive_E += room.control_Receive;
                    main_Control.update += room.update;
                    MainWindow.SizeChange += room.window_SizeChanged;

                    room.init(f_ip);
                    myNewTab.Content = room;
                    myNewTab.Focus();
                    rooms[i].roomTab = myNewTab;
                    break;
                }
            }
        }

        //좌 클릭 (속성보기)
        int select_Attribute = 0;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button but = sender as Button;
            if (but == null)
                return;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].btn == but)
                {
                    select_Attribute = i;

                    //===========================================================
                    MainWindow.attribute_Clear();
                    MainWindow.tab_Attribute1.Focus();

                    WrapPanel w1 = rooms[i].room_Attribute.Children[0] as WrapPanel;
                    TextBox txtbox1 = w1.Children[1] as TextBox;
                    txtbox1.Text = rooms[i].Room_num.ToString();
                    WrapPanel w2 = rooms[i].room_Attribute.Children[1] as WrapPanel;
                    TextBox txtbox2 = w2.Children[1] as TextBox;
                    txtbox2.Text = rooms[i].Room_name;


                    MainWindow.attribute_Add(rooms[i].room_Attribute);

                    break;

                }
            }

        }

        private void txtBox_RoomName_EnterCheck(object sender, KeyEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string before = rooms[select_Attribute].Room_name;
                Console.WriteLine("이전 데이터 : " + before + ", " + select_Attribute + "층 : " + txtBox.Text);

                //층 정보 요구하기.
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "ADMIN_ROOM_NAME_UPDATE+";
                sendData += f_ip + "+";
                sendData += rooms[select_Attribute].Room_num.ToString() + "#";
                sendData += before.ToString() + "#";
                sendData += txtBox.Text;

                sendData += "&";
                send_Data(sendData);

            }
        }

        private void txtBox_RoomNum_EnterCheck(object sender, KeyEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                int before = rooms[select_Attribute].Room_num;
                Console.WriteLine("이전 데이터 : " + before + ", " + select_Attribute + "층 : " + txtBox.Text);

                //층 정보 요구하기.
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "ADMIN_ROOM_NUM_UPDATE+";
                sendData += f_ip + "+";
                sendData += rooms[select_Attribute].Room_num.ToString() + "#";
                sendData += txtBox.Text;

                sendData += "&";
                send_Data(sendData);

            }
        }
        #endregion

        #region 우클릭 ( contextMenu 이벤트 체크 )
        private void stp_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int count = 0;
            for (int i = 0; i < stp.Children.Count; i++)
            {
                Button btn = stp.Children[i] as Button;
                if (btn == null)
                    continue;

                if (btn.IsMouseOver == true)
                    count++;
            }
            if (count == 0)
            {
                floor_Grid.ContextMenu = null;           //제거

                ContextMenu contextMenu = new ContextMenu();
                MenuItem mi0 = new MenuItem();
                mi0.Header = "추가";
                mi0.Click += contextMenu_Click_Add;
                contextMenu.Items.Add(mi0);
                floor_Grid.ContextMenu = contextMenu;
            }
        }
        #endregion

        #region contextMenu 이벤트
        private void contextMenu_Click_Delete(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null)
                return;

            for (int i = 0; i < stp.Children.Count; i++)
            {
                Button btn = stp.Children[i] as Button;
                if (btn == null)
                    continue;
                if (btn.ContextMenu == null)
                    continue;

                for (int j = 0; j < btn.ContextMenu.Items.Count; j++)
                {
                    if (btn.ContextMenu.Items[j] == item)
                    {
                        Grid gd = btn.Content as Grid;
                        if (gd == null)
                            return;
                        int roomNum = int.Parse(((TextBlock)(gd.Children[1])).Text);
                        string roomName = (((TextBlock)(gd.Children[2])).Text);

                        //서버 보내기.
                        string sendData = string.Empty;
                        sendData += "TOSERVER+";
                        sendData += "ADMIN_ROOM_MIN+";
                        sendData += f_ip + "+";
                        sendData += floor_Num.ToString() + "#";
                        sendData += roomNum.ToString() + "#";
                        sendData += roomName;
                        sendData += "&";
                        send_Data(sendData);
                        break;
                    }
                }
            }
        }
        //시스템지점
        private void contextMenu_Click_Pc_Set(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null)
                return;

            for (int i = 0; i < stp.Children.Count; i++)
            {
                Button btn = stp.Children[i] as Button;
                if (btn == null)
                    continue;
                if (btn.ContextMenu == null)
                    continue;

                for (int j = 0; j < btn.ContextMenu.Items.Count; j++)
                {
                    if (btn.ContextMenu.Items[j] == item)
                    {
                        Grid gd = btn.Content as Grid;
                        if (gd == null)
                            return;
                        int roomNum = int.Parse(((TextBlock)(gd.Children[1])).Text);
                        string roomName = (((TextBlock)(gd.Children[2])).Text);
                        string myip = string.Empty;
                        IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
                        myip = IPHost.AddressList[0].ToString();
						string exe = System.Windows.Forms.Application.StartupPath + "\\Pc_Set\\Pc_Set_Program.exe";
						Process.Start(exe, f_ip);

                         string sendData = string.Empty;
                        sendData += "TOSERVER+";
                        sendData += "PC_SET_START+";
                        sendData += f_ip + "+";
                        sendData += myip + "#";
                        sendData += floor_Num.ToString() + "#";
                        sendData += roomNum.ToString() + "#";
                        sendData += roomName;
                        sendData += "&";
                        send_Data(sendData);
                        break;
                    }
                }
            }
        }
        // 룸 추가
        private void contextMenu_Click_Add(object sender, RoutedEventArgs e)
        {
            Admin.Dialog_Get.AddRoom ar = new Admin.Dialog_Get.AddRoom();
            ar.ShowDialog();

            if (ar.window_close == true)
            {
                //서버 보내기.
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "ADMIN_ROOM_ADD+";
                sendData += f_ip + "+";
                sendData += floor_Num.ToString() + "#";
                sendData += ar.name.Text + "#";
                sendData += ar.num.Text + "#";
                sendData += ar.ip_Start.Text + "#";
                sendData += ar.ip_End.Text + "#";
                sendData += ar.width_Com.Text;
                sendData += "&";
                send_Data(sendData);
            }
        }
        #endregion

        public void window_SizeChanged()
        {
            Height = main_Control.tabControl.Width;
            Width = main_Control.tabControl.Height;
        }

    }
}
