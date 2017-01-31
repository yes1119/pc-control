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
using Admin.Type;

namespace Admin.Dialog_Get
{
    /// <summary>
    /// Interaction logic for LogInfo.xaml
    /// </summary>
    public partial class LogInfo : MetroWindow
    {
        //bool? private_dialog_result;

        public static TabItem tab_setting1;
        public static TabItem tab_monitoring1;
        public static TabItem tab_control1;

        delegate void FHideWindow();
        public LogInfo()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            TextBlock txtBlock = new TextBlock();
            txtBlock.Text = "PC 사용시간";
            txtBlock.ToolTip = "PC 사용시간 관련 LOG정보를 나타냅니다. ";
            txtBlock.Foreground = Brushes.White;
            tab_setting.Header = txtBlock;

            txtBlock = new TextBlock();
            txtBlock.Text = "소프트웨어 사용시간";
            txtBlock.ToolTip = "소프트웨어 관련 LOG정보를 나타냅니다. ";
            txtBlock.Foreground = Brushes.White;
            tab_monitoring.Header = txtBlock;
        }
    }
}
