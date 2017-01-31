using System;
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
using MahApps.Metro.Controls;
using System.Threading;
using Admin.Type;
using System.Collections;
using Admin.UserControls;


namespace Admin.Dialog_Get
{
    /// <summary>
    /// Interaction logic for PcTimerSelectDel.xaml
    /// </summary>
    public partial class PcTimerSelectDel
    {

        public static Admin.Client.Send dbsend2 = null;
        public static string ip = string.Empty;

        public static List<OnTimer> user1;
        public static List<OffTimer> user2;

        public PcTimerSelectDel(string pcip)
        {
            InitializeComponent();
            ip = pcip;
            Init();
        }

        #region 함수
        private void Init()
        {
            string sendData = string.Empty;
            sendData += "TOSERVER+";
            sendData += "DB_Pctimer_select_on+";
            sendData += Admin.UserControls.Room_Control.f_ip + "+";
            sendData += ip + "#";
            sendData += "&";
            dbsend2(sendData);

            string sendData2 = string.Empty;
            sendData2 += "TOSERVER+";
            sendData2 += "DB_Pctimer_select_off+";
            sendData2 += Admin.UserControls.Room_Control.f_ip + "+";
            sendData2 += ip + "#";
            sendData2 += "&";
            dbsend2(sendData2);

            Thread.Sleep(400);
        }

        public void listviewinit()
        {
            this.lvw1.ItemsSource = user1;
            this.lvw2.ItemsSource = user2;
        }

        public void labelinit()
        {
            //check_result.Text = label1;
        }
        #endregion


        private void lvw1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView lv = (sender as ListView);
                // int index = lv.SelectedIndex;
                IList ls = e.AddedItems;
                OnTimer alarm = ls[0] as OnTimer;
                alarm_on_idx.Text = alarm.idx.ToString();
                alarm_on_datedays.Text = alarm.date_days.ToString();
                alarm_on_ampm.Text = alarm.am_pm.ToString();
                alarm_on_time.Text = alarm.time.ToString();
            }
            catch 
            {
              
            }
        }


        private void lvw2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListView lv = (sender as ListView);
                //int index = lv.SelectedIndex;
                IList ls = e.AddedItems;
                OffTimer alarm = ls[0] as OffTimer;
                alarm_off_idx.Text = alarm.idx.ToString();
                alarm_off_datedays.Text = alarm.date_days.ToString();
                alarm_off_ampm.Text = alarm.am_pm.ToString();
                alarm_off_time.Text = alarm.time.ToString();

            }
            catch 
            {
               
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string ampm = this.alarm_on_ampm.Text;
            int am_pm = 0;
            string time = this.alarm_on_time.Text;
            string datedays = this.alarm_on_datedays.Text;
            bool a = datedays.Contains('-');       //문자열에서 특정문자찾기 맞으면  true 틀리면  false

            #region 날짜삭제일때
            if (a == true)
            {
                string D = datedays;
                string[] split = D.Split('-');
                string year = split[0];
                string month = split[1];
                string day = split[2];

                if (ampm == "오전")
                {
                    am_pm = 0;
                }
                if (ampm == "오후")
                {
                    am_pm = 1;
                }

                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "DB_Pctimer_select_Delete+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "On" + "#";
                sendData += "date" + "#";
                sendData += ip + "#";
                sendData += year + "#";
                sendData += month + "#";
                sendData += day + "#";
                sendData += am_pm + "#";
                sendData += time + "#";
                sendData += "&";
                dbsend2(sendData);
                Thread.Sleep(100);

                //for (int i = 0; i < user1.Count; i++)
                //{
                //    if (alarm_on_idx.Text == user1[i].idx.ToString())
                //    {
                //        user1.RemoveAt(i);
                //        this.lvw1.Items.Refresh();
                //        this.alarm_on_idx.Text = "";
                //        this.alarm_on_datedays.Text = "";
                //        this.alarm_on_ampm.Text = "";
                //        this.alarm_on_time.Text = "";

                //        int num1 = 1;
                //        int num2 = 0;
                //        while (true)
                //        {
                //            if ((i + num1) == user1.Count) break;
                //            user1[i + num1].idx = user1[i + num2].idx;
                //            num1++;
                //            num2++;
                //        }
                //    }
                //}
            }
            #endregion

            #region 요일삭제일때
            else if (a == false)
            {
                string days = string.Empty;
                if (datedays == "월")
                {
                    days = "1";
                }
                if (datedays == "화")
                {
                    days = "2";
                }
                if (datedays == "수")
                {
                    days = "3";
                }
                if (datedays == "목")
                {
                    days = "4";
                }
                if (datedays == "금")
                {
                    days = "5";
                }
                if (datedays == "토")
                {
                    days = "6";
                }
                if (datedays == "일")
                {
                    days = "7";
                }
                if (ampm == "오전")
                {
                    am_pm = 0;
                }
                if (ampm == "오후")
                {
                    am_pm = 1;
                }
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "DB_Pctimer_select_Delete+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "On" + "#";
                sendData += "days" + "#";
                sendData += ip + "#";
                sendData += days + "#";
                sendData += am_pm + "#";
                sendData += time + "#";
                sendData += "&";
                dbsend2(sendData);
                Thread.Sleep(100);              
            }

            for (int i = 0; i < user1.Count; i++)
            {
                if (alarm_on_idx.Text == user1[i].idx.ToString())
                {
                    this.alarm_on_idx.Text = "";
                    this.alarm_on_datedays.Text = "";
                    this.alarm_on_ampm.Text = "";
                    this.alarm_on_time.Text = "";

                    int num1 = 1;
                    int num2 = 0;
                    while (true)
                    {
                        if ((i + num1) == user1.Count) break;
                        user1[i + num1].idx = user1[i + num2].idx;
                        num1++;
                        num2++;
                    }
                    user1.RemoveAt(i);
                    this.lvw1.Items.Refresh();
                }              
            }
            
            #endregion
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            string ampm = this.alarm_off_ampm.Text;
            int am_pm = 0;
            string time = this.alarm_off_time.Text;
            string datedays = this.alarm_off_datedays.Text;
            bool a = datedays.Contains('-');       //문자열에서 특정문자찾기 맞으면  true 틀리면  false

            #region 날짜삭제일때
            if (a == true)
            {
                string D = datedays;
                string[] split = D.Split('-');
                string year = split[0];
                string month = split[1];
                string day = split[2];

                if (ampm == "오전")
                {
                    am_pm = 0;
                }
                if (ampm == "오후")
                {
                    am_pm = 1;
                }

                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "DB_Pctimer_select_Delete+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "Off" + "#";
                sendData += "date" + "#";
                sendData += ip + "#";
                sendData += year + "#";
                sendData += month + "#";
                sendData += day + "#";
                sendData += am_pm + "#";
                sendData += time + "#";
                sendData += "&";
                dbsend2(sendData);
                Thread.Sleep(100);

                //for (int i = 0; i < user2.Count; i++)
                //{
                //    if (alarm_off_idx.Text == user2[i].idx.ToString())
                //    {
                //        try
                //        {
                //            user2.RemoveAt(i);
                //            this.lvw2.Items.Refresh();
                //            this.alarm_off_idx.Text = "";
                //            this.alarm_off_datedays.Text = "";
                //            this.alarm_off_ampm.Text = "";
                //            this.alarm_off_time.Text = "";


                //            int num1 = 1;
                //            int num2 = 0;
                //            while (true)
                //            {
                //                if ((i + num1) == user2.Count) break;
                //                user2[i + num1].idx = user2[i + num2].idx;
                //                num1++;
                //                num2++;
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine("~~~~~~~~~~~~~~~~" + ex.Message);
                //        }
                //    }
                //}
            }
            #endregion

            #region 요일삭제일때
            else if (a == false)
            {

                string days = string.Empty;
                if (datedays == "월")
                {
                    days = "1";
                }
                if (datedays == "화")
                {
                    days = "2";
                }
                if (datedays == "수")
                {
                    days = "3";
                }
                if (datedays == "목")
                {
                    days = "4";
                }
                if (datedays == "금")
                {
                    days = "5";
                }
                if (datedays == "토")
                {
                    days = "6";
                }
                if (datedays == "일")
                {
                    days = "7";
                }
                if (ampm == "오전")
                {
                    am_pm = 0;
                }
                if (ampm == "오후")
                {
                    am_pm = 1;
                }
                string sendData = string.Empty;
                sendData += "TOSERVER+";
                sendData += "DB_Pctimer_select_Delete+";
                sendData += Admin.UserControls.Room_Control.f_ip + "+";
                sendData += "Off" + "#";
                sendData += "days" + "#";
                sendData += ip + "#";
                sendData += days + "#";
                sendData += am_pm + "#";
                sendData += time + "#";
                sendData += "&";
                dbsend2(sendData);
                Thread.Sleep(100);
                try
                {
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!" + ex.Message);
                }


            }

            for (int i = 0; i < user2.Count; i++)
            {
                if (alarm_off_idx.Text == user2[i].idx.ToString())
                {                                 
                    this.alarm_off_idx.Text = "";
                    this.alarm_off_datedays.Text = "";
                    this.alarm_off_ampm.Text = "";
                    this.alarm_off_time.Text = "";

                    int num1 = 1;
                    int num2 = 0;
                    while (true)
                    {
                        if ((i + num1) == user2.Count) break;
                        user2[i + num1].idx = user2[i + num2].idx;
                        num1++;
                        num2++;
                    }
                    user2.RemoveAt(i);
                    this.lvw2.Items.Refresh();
                }              
            }
           
            #endregion
           
        }

    }
}
