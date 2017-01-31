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

namespace Admin.Dialog_Get
{
    /// <summary>
    /// Interaction logic for PcTimer.xaml
    /// </summary>
    public partial class PcTimer
    {
        public string datepicker = null;
        public int am1 = 0;
        public bool checkbox1 = false, checkbox2 = false, checkbox3 = false, checkbox4 = false, checkbox5 = false, checkbox6 = false, checkbox7 = false;
        public PcTimer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void datepicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            check1.IsEnabled = false;
            check2.IsEnabled = false;
            check3.IsEnabled = false;
            check4.IsEnabled = false;
            check5.IsEnabled = false;
            check6.IsEnabled = false;
            check7.IsEnabled = false;

            morning2.IsEnabled = false;
            afternoon2.IsEnabled = false;

            hour2.IsEnabled = false;
            minute2.IsEnabled = false;

            // ... Get DatePicker reference.
            var picker = sender as DatePicker;

            // ... Get nullable DateTime from SelectedDate.
            DateTime? date = picker.SelectedDate;
            datepicker = date.Value.ToString();


        }

        private void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            morning1.IsEnabled = false;
            afternoon1.IsEnabled = false;
            hour1.IsEnabled = false;
            minute1.IsEnabled = false;

            if ((bool)check1.IsChecked)
                checkbox1 = true;
            if ((bool)check2.IsChecked)
                checkbox2 = true;
            if ((bool)check3.IsChecked)
                checkbox3 = true;
            if ((bool)check4.IsChecked)
                checkbox4 = true;
            if ((bool)check5.IsChecked)
                checkbox5 = true;
            if ((bool)check6.IsChecked)
                checkbox6 = true;
            if ((bool)check7.IsChecked)
                checkbox7 = true;
        }

        private void morning1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)morning1.IsChecked)
            {
                am1 = 0;       //0은 오전 1은 오후    
            }

        }

        private void afternoon1_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)afternoon1.IsChecked)
            {
                am1 = 1;//0은 오전 1은 오후


            }
        }

        private void morning2_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)morning2.IsChecked)
            {
                am1 = 0;//0은 오전 1은 오후

            }
        }

        private void afternoon2_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)afternoon2.IsChecked)
            {
                am1 = 1;//0은 오전 1은 오후

            }
        }
    }


}
