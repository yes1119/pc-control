using Admin.Type;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// computer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class computer : UserControl
    {
        private CComputer pcInfo = new CComputer();

        public CComputer getCPc()
        {
            return pcInfo;
        }
        public void setCPc_Coordinate()
        {
            pcInfo.coordinate = UserControlToolTipXY.X.ToString() + ":" + UserControlToolTipXY.Y.ToString();
        }

        //TODO : 전부다 복사는 아니다...
        public void setCpc(CComputer pc)
        {
            pcInfo.room_num = pc.room_num;
            pcInfo.pc_type_c = pc.pc_type_c;
            pcInfo.ip = pc.ip;
            pcInfo.mac = pc.mac;
            pcInfo.power = pc.power;
            pcInfo.coordinate = pc.coordinate;
            pcInfo.software = pc.software;
            //pcInfo.hardware = pc.hardware;
            pcInfo.image = pc.image;
            pcInfo.exe = pc.exe;
            pcInfo.last_Format = pc.last_Format;
            pcInfo.windowaccount = pc.windowaccount;
            pcInfo.on_time = pc.on_time;
            pcInfo.off_time = pc.off_time;
            pcInfo.panel = pc.panel;
            pcInfo.sprite_Count = pc.sprite_Count;
            pcInfo.check_time = pc.check_time;
            pcInfo.data_Connect = "0:0";
            pcInfo.data_Percent = "0:0";
            pcInfo.pointer_name = pc.pointer_name;
            pcInfo.pc_state = pc.pc_state;
            pcInfo.check_msg = pc.check_msg;
			pcInfo.format_state = pc.format_state;
        }

        public void init()
        {
            Image img = new Image();
            img.Margin = new Thickness(0, 0, 0, 0);
            img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            img.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            img.Stretch = Stretch.Fill;
            img.Width = 124;
            img.Height = 94;

            img.Source = CImage.getInstance().get_Pc_Images()[0][0];
            main_Canvas.Children.Add(img);

            TextBlock tb1 = new TextBlock();
            tb1.Foreground = Brushes.White;
            tb1.Margin = new Thickness(5, 2, 0, 0);
            tb1.Text = pcInfo.ip;

            pcInfo.image_Btn = img;
            
            main_Canvas.Children.Add(tb1);

            Canvas.SetZIndex(progressBar, 99);
        }

        //마우스 클릭가 클릭 된 좌표.
        private Point mousePressPoint = new Point();

        //그리드 되면 나오는 좌표
        private Point staticGridPoint = new Point();

        //부모 윈도우의 창 크기
        double parents_Width;
        double parents_Height;

        public computer()
        {
            InitializeComponent();
        }

        //staticGridPoint 설정.
        public void set_StaticGridPoint(double x, double y)
        {
            staticGridPoint.X = x;
            staticGridPoint.Y = y;
        }

        //부모 윈도우 크기 변경.
        public void set_Parents_SIze(double width, double height)
        {
            parents_Width = width;
            parents_Height = height;
        }

        //부모 윈도우에서 클릭 하였을 경우.
        public void set_MousePressPoint()
        {
            mousePressPoint = Mouse.GetPosition(main_Canvas);
        }

        //캔버스 드래그 -> 부모 윈도우를 통하여 설정가능.
        public void MouseDrag(object sender, MouseEventArgs e)
        {
            if (!area_Check())
                return;

            //마우스 좌 버튼 체크
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (staticGridPoint.X > 0 || staticGridPoint.Y > 0)
                {
                    UserControlToolTipXY.X = staticGridPoint.X;
                    UserControlToolTipXY.Y = staticGridPoint.Y;
                }
                else
                {
                    UserControlToolTipXY.X += e.GetPosition(main_Canvas).X - mousePressPoint.X;
                    UserControlToolTipXY.Y += e.GetPosition(main_Canvas).Y - mousePressPoint.Y;
                }
            }
        }

        public bool child_Grid_Check()
        {
            if (grid_Left.Fill == Brushes.Black)
                return true;
            if (grid_Top.Fill == Brushes.Black)
                return true;
            if (grid_Right.Fill == Brushes.Black)
                return true;
            if (grid_bottom.Fill == Brushes.Black)
                return true;

            return false;
        }

        //영역 체크후 밖으로 못나가게 한다.
        private bool area_Check()
        {
            if (parents_Width < UserControlToolTipXY.X + main_Canvas.Width + 10)
            {
                UserControlToolTipXY.X -= 20;
                mousePressPoint.X += 20;
                return false;
            }
            if (-10 > UserControlToolTipXY.X)
            {
                UserControlToolTipXY.X += 20;
                mousePressPoint.X -= 10;
                return false;
            }
            if (parents_Height < UserControlToolTipXY.Y + main_Canvas.Height + 40)
            {
                UserControlToolTipXY.Y -= 20;
                mousePressPoint.Y += 15;
                return false;
            }
            if (-10 > UserControlToolTipXY.Y)
            {
                UserControlToolTipXY.Y += 20;
                mousePressPoint.Y -= 15;
                return false;
            }
            return true;
        }

        private void main_Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (border_Canvas.Background != Brushes.Red)
                border_Canvas.Background = Brushes.Blue;
        }

        private void main_Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (border_Canvas.Background != Brushes.Red)
            {
                SolidColorBrush sc = new SolidColorBrush();
                sc.Color = Color.FromArgb(0, 0, 0, 0);
                border_Canvas.Background = sc;
            }
        }
    }
}
