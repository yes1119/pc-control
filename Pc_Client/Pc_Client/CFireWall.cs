using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pc_Client
{
    class CFireWall
    {
        private const string CLSID_FIREWALL_MANAGER = "{304CE942-6E39-40D8-943A-B913C40C9CD4}";

        private static NetFwTypeLib.INetFwMgr GetFirewallManager()
        {
            Type objectType = Type.GetTypeFromCLSID(new Guid(CLSID_FIREWALL_MANAGER));
            return Activator.CreateInstance(objectType) as NetFwTypeLib.INetFwMgr;
        }

        public static void FireWallCheck()
        {

            System.Diagnostics.ProcessStartInfo proInfo = new System.Diagnostics.ProcessStartInfo();
            System.Diagnostics.Process pro = new System.Diagnostics.Process();

            // proinfo = 프로세스의 설정
            proInfo.FileName = @"cmd";

            proInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proInfo.CreateNoWindow = true;

            proInfo.UseShellExecute = false;
            proInfo.RedirectStandardOutput = true;
            proInfo.RedirectStandardInput = true;
            proInfo.RedirectStandardError = true;

            pro.StartInfo = proInfo;
            pro.Start();

            INetFwMgr manager = GetFirewallManager();
            bool isFirewallEnabled = manager.LocalPolicy.CurrentProfile.FirewallEnabled;

            if (isFirewallEnabled == true)//방화벽이 열려 있을경우
            {
                pro.StandardInput.Write("netsh firewall set opmode mode=DISABLE" + Environment.NewLine);
                //MessageBox.Show("방화벽 해제");
            }
            else//방화벽이 닫혀 있을 경우
            {
                //pro.StandardInput.Write("netsh firewall set opmode mode=ENABLE" + Environment.NewLine);
                //MessageBox.Show("방화벽 온");
            }

            pro.StandardInput.Close();
            string resultValue = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();
            pro.Close();
        }
    }
}
