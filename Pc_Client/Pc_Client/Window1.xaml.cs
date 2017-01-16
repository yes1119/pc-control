using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Pc_Client
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window1 : Window
    {
        #region Member
        public static string serverip, myip, myname, mymac, formatflag, formatflag2, mytype, change_ip;
        public static string Connectip { get; set; }    // serverip를 읽은 내용
        public static string Client_type { get; set; }    // type 변경2
        public static string Change_ip { get; set; }

        #endregion
        public Window1()
        {
            InitializeComponent();
            myip = "C:\\IP.txt";
            mytype = "C:\\TYPE.txt";
            myname = "C:\\PCNAME.txt";
            mymac = "C:\\MAC.txt";
            serverip = "C:\\SERVERIP.txt";
            formatflag = "C:\\FORMATFLAG.txt";
            formatflag2 = "C:\\FORMATFLAG2.txt";
            change_ip = "C:\\CHANGEIP.txt";



            try
            {
                FileInfo formatflag_txt = new FileInfo(formatflag);
                FileStream formatflag_stream = formatflag_txt.Create();
                formatflag_stream.Close();
            }
            catch
            {

            }
            FileInfo readFile = new FileInfo(serverip);

            if (!readFile.Exists)
            {
                this.ShowDialog();
                return;
            }
            else
            {
                using (TextReader reader = File.OpenText(serverip))
                {
                    Connectip = reader.ReadLine();
                }

                
                using (TextReader reader = File.OpenText(mytype))
                {
                    Client_type = reader.ReadLine();
                }

                using (TextReader reader = File.OpenText(change_ip))
                {
                    Change_ip = reader.ReadLine();
                }
                //=========================================
                #region 트래쉬
                //FileInfo readFile2 = new FileInfo(myip);
                //FileInfo readFile3 = new FileInfo(myname);
                //FileInfo readFile4 = new FileInfo(mymac);

                //if (readFile2.Exists == readFile3.Exists == readFile4.Exists)
                //{
                //    FileInfo formatflag2_txt = new FileInfo(formatflag2);
                //    if (formatflag2_txt.Exists)
                //    {
                //        SettingIP(0, 0);
                //        //DeleteFile(formatflag2_txt);
                //        // PcreBoot(0,0,);
                //    }
                //}
                #endregion
            }

        }


        public void SettingIP(int wParam, int IParam)
        {
            String MyIpAddr;
            MyIpAddr = GetInfo(myip);
            string ip = GetInfo(myip),
                   Mask = "255.255.255.0",
                   GateWap = "61.81.99.1",
                   MainDns = "168.126.63.1",
                   SubDns = "59.26.6.6";

            //string[] Command_IP = new string[1024];
            //string[] Command_DBS = new string[1024];
            //string[] Command_SUBDNS = new string[1024];

            SaveInfo(0, 0);
        }
        public string GetInfo(string a)
        {
            string str = string.Empty;

            FileInfo txt = new FileInfo(a);
            if (txt.Exists)
            {
                TextReader tr = txt.OpenText();

                while (true)
                {
                    string line = tr.ReadLine();
                    if (line != null)
                    {
                        line.Replace("\n", "");
                        str += line;
                    }
                    else
                    {
                        tr.Close();
                        break;
                    }
                }
            }
            return str;
        }
        public void SaveInfo(int wParam, int IParam) //미완성
        {
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            string myIP = IPHost.AddressList[0].ToString();

            Funtion cs = new Funtion();
            SetInfo(myname, cs.GetPcName());//pc 네임

            SetInfo(mymac, cs.GetMacAddress().ToString());//mac

            SetInfo(myip, myIP); //my ip

        }
        public void SetInfo(string a, string b) //미완성
        {
            FileInfo file = new FileInfo(a);
            FileStream fs = file.Create();
            TextWriter tw = new StreamWriter(fs, Encoding.UTF8);
            tw.WriteLine(b);
            tw.WriteLine(b.Length);
            tw.Close();
            fs.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = this.server_ip_txt.Text;
            string str2 = this.client_type_txt.Text;
            Connectip = this.server_ip_txt.Text;
            Client_type = this.client_type_txt.Text;
            FileInfo serverip_txt = new FileInfo(serverip);
            FileStream serverip_txt_stream = serverip_txt.Create();
            TextWriter tw = new StreamWriter(serverip_txt_stream, Encoding.UTF8);
            tw.Write(str);
            tw.Close();
            serverip_txt_stream.Close();
           
            FileInfo client_type_txt = new FileInfo(mytype);
            FileStream client_type_txt_stream = client_type_txt.Create();
            TextWriter tw2 = new StreamWriter(client_type_txt_stream, Encoding.UTF8);
            tw2.Write(str2);
            tw2.Close();
            
            client_type_txt_stream.Close();

            FileInfo client_change_ip_txt = new FileInfo(Window1.change_ip);
            FileStream client_change_ip_txt_stream = client_change_ip_txt.Create();
            TextWriter tw3 = new StreamWriter(client_change_ip_txt_stream, Encoding.UTF8);
            tw3.Write("없음");
            tw3.Close();
            //======================================
            client_change_ip_txt_stream.Close();

            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
