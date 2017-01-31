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

namespace Admin.UserControls
{
    /// <summary>
    /// DeviceChoice.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DeviceChoice 
    {
        public bool devicecheckclose = false;
        public string str;
        public string[] selectstr;
        CheckBox check = new CheckBox();
        public DeviceChoice()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            check = new CheckBox();
            check.Measure(new Size(100, 200));
            f.Children.Add(check);

        }

      
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            str += (keyborad.IsChecked == true) ? "표준 PS/2 키보드" + "#" : "선택안됨" + "#";
            str += (monitor.IsChecked == true) ? "일반 PnP 모니터" + "#" : "선택안됨" + "#";
            str += (mouse.IsChecked == true) ?  "HID 규격 마우스" + "#" : "선택안됨" + "#";

            Room_Control.Choicestr = str;
            devicecheckclose = true;
            this.Close();
        }
    }
}
