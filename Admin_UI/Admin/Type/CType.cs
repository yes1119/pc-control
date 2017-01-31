using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

//============================
using Admin.UserControls;
//============================

namespace Admin.Type
{
    public class OnTimer
    {
        public int idx { get; set; }
        public string date_days { get; set; }
        public string am_pm { get; set; }
        public string time { get; set; }

    }
    public class OffTimer
    {
        public int idx { get; set; }
        public string date_days { get; set; }
        public string am_pm { get; set; }
        public string time { get; set; }
    }
    public class CSOFTWAREList
    {
        public string id { get; set; }
        public string filename { get; set; }
    }

    public class SOFTWARE
    {
        public int id { get; set; }
        public string idx { get; set; }
        public string SOFTWARENAME { get; set; }
        public string SOFTWARETIME { get; set; }
    }

    public class CWINDOW
    {
        public string Win_ip { get; set; }
        public string Win_account { get; set; }
        public string Win_PW { get; set; }
        public string Win_state { get; set; }
    }
    public class CDEVICE
    {
        public string pIP { get; set; }
        public string keyboard { get; set; }
        public string mouse { get; set; }
        public string moniter { get; set; }
        public string time { get; set; }
    }

    public class CSOFTWARE
    {
        public string pIP { get; set; }
        public string software { get; set; }
        public string Work { get; set; }
        public string time { get; set; }
    }
    //db타입.
    public class CLogAll
    {
        public string LOG_TYPE { get; set; }
        public string PC_IP { get; set; }
        public string LOG_NAME { get; set; }
        public string LOG_LOG { get; set; }
        public string LOG_DETECTION_DAY { get; set; }
        public string LOG_DETECTION_TIME { get; set; }
        public string LOG_TODAY { get; set; }
    }

    public class CFloor
    {
        public int Floor_num;
        public string Floor_name;
        public string Floor_IP;
        public int Floor_Power;
        public int Floor_Connect;
        public int max_Computer;
        public int accept_Computer;
        public string Image_File = "알 수 없음";
        public string Exe_File = "알 수 없음";
        public Button btn;
        public Image image;
        public int sprite_Count;
        public TabItem floorTab;
        public WrapPanel floor_Attribute;

        public List<CRoom> rooms;
    }

    public class CRoom
    {
        public int Floor_num;
        public string Room_name;
        public int Room_num;
        public int max_Computer;
        public int accept_Computer;
        public Button btn;
        public Image image;
        public int sprite_Count;
        public TabItem roomTab;
        public WrapPanel room_Attribute;
    }

    public class CComputer
    {
        public int room_num;
        public string pc_type_c;
        public string ip;
        public string mac;
        public int power;
        public string coordinate;
        public string software;
        //public int hardware;
        public int image;
        public int exe;
        public string last_Format;
        public int windowaccount;
        public int on_time;
        public int off_time;
        public string panel;
        public int check_time = 0;
        public string check_msg = "알 수 없음";
        public string control_panel = "알 수 없음";
        public string pointer_name;
        public string pc_state;
		public int format_state;

        public UserControl userControl;
        public Image image_Btn;
        public int sprite_Count;
        public WrapPanel com_Attribute;

        public string data_Percent;
        public string data_Connect;
    }

    public class CImage//싱글톤 기법 [버튼 스프라이트].
    {
        private static CImage instance = null;
        private List<List<BitmapImage>> class_Image = new List<List<BitmapImage>>();
        private List<List<BitmapImage>> pc_Image = new List<List<BitmapImage>>();

        public List<List<BitmapImage>> get_Class_Images()
        {
            return class_Image;
        }

        public List<List<BitmapImage>> get_Pc_Images()
        {
            return pc_Image;
        }

        public static CImage getInstance()
        {
            if (instance == null)
            {
                instance = new CImage();
                instance.init();
            }

            return instance;
        }

        private CImage()
        {
        }

        public void init()
        {
            //======================================================
            //CLASS
            List<BitmapImage> image = new List<BitmapImage>();
            string url = "re\\class\\none\\ClroomOff.png";
            BitmapImage bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);
            class_Image.Add(image);

            //======================================================
            image = new List<BitmapImage>();
            url = "re\\class\\min\\ClroomOn_One.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\min\\ClroomOn_One1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\min\\ClroomOn_One2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\min\\ClroomOn_One3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\min\\ClroomOn_One4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            class_Image.Add(image);
            //======================================================
            image = new List<BitmapImage>();
            url = "re\\class\\mid\\ClroomOn_Two.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\mid\\ClroomOn_Two1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\mid\\ClroomOn_Two2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\mid\\ClroomOn_Two3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\mid\\ClroomOn_Two4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            class_Image.Add(image);
            //======================================================
            image = new List<BitmapImage>();
            url = "re\\class\\max\\ClroomOn_Three.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\max\\ClroomOn_Three1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\max\\ClroomOn_Three2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\max\\ClroomOn_Three3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\class\\max\\ClroomOn_Three4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            class_Image.Add(image);



            //======================================================
            //PC off 0
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_off\\pc_off_not_file.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);

            //==================================================
            //==================================================
            //베이직 1 
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_basic\\pc_basic_\\pc_basic_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_\\pc_basic_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_\\pc_basic_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_\\pc_basic_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);
            pc_Image.Add(image);

            //======================================================
            //======================================================
            //포맷 2 
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_format\\pc_format_\\pc_format_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_5.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_6.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_7.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_\\pc_format_8.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);

            //======================================================
            //======================================================
            //부팅 3
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_on\\pc_on_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_5.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_6.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_7.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_8.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_on\\pc_on_9.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);

            //======================================================
            //======================================================
            //윈도우 계정 변경 4
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_5.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_6.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_7.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_8.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_\\pc_account_add_9.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);

            //======================================================
            //PC off 5
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_off\\pc_off_timer\\pc_off_not_file.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);
            //==================================================
            //==================================================
            //베이직 6
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_basic\\pc_basic_timer\\pc_basic_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_timer\\pc_basic_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_timer\\pc_basic_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_basic\\pc_basic_timer\\pc_basic_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);
            pc_Image.Add(image);

            //======================================================
            //======================================================
            //포맷 7
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_5.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_6.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_7.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_format\\pc_format_timer\\pc_format_8.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);

            //======================================================
            //======================================================
            //윈도우 계정 변경 8
            image = new List<BitmapImage>();
            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_1.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_2.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_3.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_4.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_5.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_6.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_7.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_8.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            url = "re\\pc\\pc_account_add\\pc_account_add_timer\\pc_account_add_9.png";
            bitmapimage = new BitmapImage(new Uri(url, UriKind.Relative));
            image.Add(bitmapimage);

            pc_Image.Add(image);


        }
    }
}
