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
    /// AddRoom.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddRoom
    {
        public bool window_close = false;
        public AddRoom()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            window_close = true;
            this.Close();
        }
    }
}
