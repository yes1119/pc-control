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
using System.Windows.Shapes;

namespace Admin.Dialog_Get
{
    /// <summary>
    /// WindowCreate.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class WindowCreate
    {
        public bool checkclose = false;
        public string window_account;
        public string window_pw;
        public WindowCreate()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            window_account = this.account.Text;
            window_pw = this.pw.Text;
            checkclose = true;
            this.Close();
        }
    }
}
