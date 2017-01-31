using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Admin.Type;
using MahApps.Metro.Controls;
using Admin.UserControls;

namespace Admin.Dialog_Get
{
    /// <summary>
    /// Interaction logic for Software.xaml
    /// </summary>
    public partial class Software
    {
        public static Admin.Client.Send dbsend1 = null;
        public static string ipdata;
        public static string label1;

        public static List<SOFTWARE> user1;
        public static List<SOFTWARE> user2;
        public static List<SOFTWARE> user3;
        public static List<SOFTWARE> user4;

        public Software()
        {
            InitializeComponent();
            Init();
        }

        public Software(string ip)
        {
            InitializeComponent();
            Init();
            ipdata = ip;
        }

        #region 함수
        private void Init()
        {
           string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "DB_Soft_Init+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += "SELECT * From SOFTWARE" + "#";
            sendData += "&";
            dbsend1(sendData);
        }

        public void listviewinit()
        {
            this.lvw1.ItemsSource = user1;
            this.lvw2.ItemsSource = user2;
            this.lvw4.ItemsSource = user4;
        }
        
        public void labelinit()
        {
            check_result.Text = label1;
        }
        #endregion

        #region 설치여부
        private void Button_Click(object sender, RoutedEventArgs e)     //추가
        {
            string file = this.file_name1.Text;
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "DB_Soft_Insert+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += "INSERT into CLIENT_SOFTWARE_SET(SOFTWARE_NAME) values('" + file + "')";
            sendData += "&";
            dbsend1(sendData);
            Thread.Sleep(100);
            this.lvw1.ItemsSource = user1;
            this.file_name1.Text = "";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)       //삭제
        {
            string file = this.file_name1.Text;
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "DB_Soft_Delete+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += "DELETE FROM  CLIENT_SOFTWARE_SET WHERE SOFTWARE_NAME='" + file + "'";
            sendData += "&";
            dbsend1(sendData);
            Thread.Sleep(100);
            this.lvw1.ItemsSource = user1;
            this.file_name1.Text = "";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)           //검사
        {
            int indx = this.lvw1.Items.IndexOf(this.lvw1.SelectedItem);
            IEnumerable items = this.lvw1.Items;
            indx += 1;
            string sendData = string.Empty;
            string filename = string.Empty;

            if (indx == 0)
            {
                MessageBox.Show("목록을 선택 하세요...");
            }
            else
            {
                foreach (SOFTWARE employee in items)
                {
                    if (employee.id.ToString() == indx.ToString())
                    {
                        UserControls.Room_Control.filename = employee.SOFTWARENAME.ToString();
                        filename = employee.SOFTWARENAME.ToString();
                        file_name1.Text = employee.SOFTWARENAME.ToString();
                        break;
                    }

                }
                file_name1.Text = "";
                sendData += "TOSERVER+";
                sendData += "CLIENT_software+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "CLIENT_software#";
                sendData += Admin.UserControls.Room_Control.room_Num1.ToString() + "#";
                sendData += ipdata + "#";
                sendData += filename;
                sendData += "&";
                dbsend1(sendData);

            }
        }

        private void lvw1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView lv = (sender as ListView);
                int index = lv.SelectedIndex;
                IList ls = e.AddedItems;
                SOFTWARE employee = ls[0] as SOFTWARE;
                file_name1.Text = employee.SOFTWARENAME.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 : " + ex.Message);
            }
        }
        #endregion

        #region 사용순위
        public void lvw2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView lv = (sender as ListView);
                int index = lv.SelectedIndex;
                IList ls = e.AddedItems;
                SOFTWARE employee = ls[0] as SOFTWARE;
                file_name2.Text = employee.SOFTWARENAME.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("lvw2_SelectionChanged오류 : " + ex.Message);
            }
        }

        //추가(===============창근변경==================)
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string file = this.file_name2.Text;
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "ADMIN_SOFTWARE_USE_NAME_INSERT+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += file + "#";
            sendData += Admin.UserControls.Room_Control.room_Num1.ToString();
            sendData += "&";
            dbsend1(sendData);
            this.lvw2.ItemsSource = user2;
            this.file_name2.Text = "";
        }

        //삭제(===============창근변경==================)
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string file = this.file_name2.Text;
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "ADMIN_SOFTWARE_USE_NAME_DELETE+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += file + "#";
            sendData += Admin.UserControls.Room_Control.room_Num1.ToString();
            sendData += "&";
            dbsend1(sendData);
            this.lvw2.ItemsSource = user2;
            this.file_name2.Text = "";
        }
        #endregion

        #region 제대로 작동
        // 선희 <==========
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //검사할 파일 입력하고 받아서 날리고 true면 label1 제대로 작동 합니닿 아니면 오작동합니닿 출력
            string ip = string.Empty;
            string sendData = string.Empty;
            string filename = string.Empty;

            if (file_name3.Text == null)
            {
                check_result.Text = "파일명을 입력하세요...";
            }
            else
            {
                UserControls.Room_Control.filename = file_name3.Text;
                filename = file_name3.Text;
                file_name3.Text = "";
                sendData += "TOSERVER+";
                sendData += "CLIENT_soft_respond+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "CLIENT_soft_respond#";
                sendData += Admin.UserControls.Room_Control.room_Num1.ToString() + "#";
                sendData += ipdata + "#";
                sendData += filename;
                sendData += "&";
                dbsend1(sendData);
                Thread.Sleep(400);
                check_result.Text = label1;
            }

        }

        private void check_result_TargetUpdated(object sender, DataTransferEventArgs e)
        {
        }

        private void file_name3_GotMouseCapture(object sender, MouseEventArgs e)
        {
            file_name3.Text = "";
        }
        #endregion

        #region 불필요 소프트웨어
        private void lvw4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView lv = (sender as ListView);
                int index = lv.SelectedIndex;
                IList ls = e.AddedItems;
                SOFTWARE employee = ls[0] as SOFTWARE;
                file_name4.Text = employee.SOFTWARENAME.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 : " + ex.Message);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)           //삭제
        {
            int indx = this.lvw4.Items.IndexOf(this.lvw4.SelectedItem);
            IEnumerable items = this.lvw4.Items;
            indx += 1;
            string sendData = string.Empty;
            string filename = string.Empty;

            if (indx == 0)
            {
                MessageBox.Show("목록을 선택 하세요...");
            }
            else
            {
                foreach (SOFTWARE employee in items)
                {
                    if (employee.id.ToString() == indx.ToString())
                    {
                        UserControls.Room_Control.filename = employee.SOFTWARENAME.ToString();
                        filename = employee.SOFTWARENAME.ToString();
                        file_name4.Text = employee.SOFTWARENAME.ToString();
                        break;
                    }

                }
                file_name4.Text = "";
                sendData += "TOSERVER+";
                sendData += "CLIENT_SOFTWARE_DEL+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "CLIENT_SOFTWARE_DEL#";
                sendData += Admin.UserControls.Room_Control.room_Num1.ToString() + "#";
                sendData += ipdata + "#";
                sendData += filename;
                sendData += "&";
                dbsend1(sendData);

            }
        }
        #endregion       

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Room_Control.softlist1.Reset();
            Room_Control.softlist2.Reset();
            Room_Control.softlist4.Reset();
        }
        
    }
}
