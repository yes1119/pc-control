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
    /// AddPC.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddPC
    {
        public bool closeCheck = false;
        public AddPC()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            closeCheck = true;
            this.Close();
        }
    }
}
