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

namespace Admin.Dialog_Get
{
    /// <summary>
    /// Interaction logic for PcTimerDel.xaml
    /// </summary>
    public partial class PcTimerDel
    {
        public bool checkbox1 = false;
        public bool checkbox2 = false;
        public PcTimerDel()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void checkon_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)check1.IsChecked)
                checkbox1 = true;
            if ((bool)check2.IsChecked)
                checkbox2 = true;
        }
    }
}
