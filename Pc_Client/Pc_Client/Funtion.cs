using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pc_Client
{
    class Funtion
    {
        static List<string> list1 = new List<string>();
        public static List<string> list2 =  new List<string>();
        #region DLL
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);
        public const int WM_COMMAND = 0x0111;
        public const int IDYES = 6;
        public const int APPCOMMAND_NEW = 29;
        [DllImport("user32.dll")]
        static extern bool PostMessage(int hWnd, uint Msg, int wParam, int lParam);
        //실행시키기
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);
        #endregion
        public static Send device_send = null;
        public  string Name { get; set; }
        public  string Status { get; set; }
        public  string Work { get; set; }
        public PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }
        public string GetPcName()
        {
            string domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();
            string fqdn = "";
            if (!hostName.Contains(domainName))
                return fqdn = hostName + "." + domainName;
            else
                return fqdn = hostName;
        }
        public static void CreateUserAccount(string login, string password, string fullName, bool isAdmin)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntries entries = dirEntry.Children;
                DirectoryEntry newUser = entries.Add(login, "user");
                //newUser.Properties["FullName"].Add(fullName);
                newUser.Invoke("SetPassword", password);
                newUser.CommitChanges();

                // Remove the if condition along with the else to create user account in "user" group.
                DirectoryEntry grp;
                if (isAdmin)
                {
                    grp = dirEntry.Children.Find("Administrators", "group");
                    if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }
                }
                else
                {
                    grp = dirEntry.Children.Find("Guests", "group");
                    if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        public static long fileCheck(string FileRoute)
        {
            string fileName = FileRoute.Replace("\\", "/");
            long fileLen = 0;
            while (fileName.IndexOf("/") > -1)
            {
                fileName = fileName.Substring(fileName.IndexOf("/") + 1);
                Console.WriteLine(fileName);
            }
            FileInfo fileInfo = new FileInfo(FileRoute);
        
            if (fileInfo.Exists == true)//파일이 존재할때.
                fileLen = fileInfo.Length;
            //else//파일이 존재하지 않을때
            
            return fileLen;
        }
        public static void fileDelete(string FileRoute)
        {
            string fileName = FileRoute.Replace("\\", "/");
            while (fileName.IndexOf("/") > -1)
            {
                fileName = fileName.Substring(fileName.IndexOf("/") + 1);
            }
            FileInfo fileInfo = new FileInfo(FileRoute);
            if (fileInfo.Exists == true)//파일이 존재할때.
                fileInfo.Delete();
        }

        public static void fileRouteDel(string FileRoute)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(FileRoute);
                if (System.IO.Directory.Exists(FileRoute))//파일이 존재할때.
                {
                    System.IO.Directory.Delete(FileRoute, true);
                }
            }
            catch
            {
            }
        }

        public static void remeve(string LoginName)
        {
            DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
            DirectoryEntries users = localDirectory.Children;
            DirectoryEntry user = users.Find(LoginName);
            users.Remove(user);
        }

        #region 제어판
        public static string f_registry()
        {
            //레지스트리 경로 찾아가며 만들기
            //최종경로
            //HKCU\Softwore\Microsoft\Windows\CurrentVersion\Policies\Explorer
            RegistryKey software = Registry.CurrentUser.CreateSubKey("software");
            RegistryKey Microsoft = software.CreateSubKey("Microsoft");
            RegistryKey Windows = Microsoft.CreateSubKey("Windows");
            RegistryKey CurrentVersion = Windows.CreateSubKey("CurrentVersion");
            RegistryKey Policies = CurrentVersion.CreateSubKey("Policies");
            RegistryKey Explorer = Policies.CreateSubKey("Explorer");

            //함수 유무 판단            
            object o = Explorer.GetValue("NoControlPanel");
            string state = string.Empty;

            if(o == null)
            {
                state = "false";
            }
            else if(o != null)
            {
                state = "true";
            }

            
            return state;
        }
        #endregion

        #region 제어판
        public static void f_registry(bool flag)
        {
            //레지스트리 경로 찾아가며 만들기
            //최종경로
            //HKCU\Softwore\Microsoft\Windows\CurrentVersion\Policies\Explorer
            RegistryKey software = Registry.CurrentUser.CreateSubKey("software");
            RegistryKey Microsoft = software.CreateSubKey("Microsoft");
            RegistryKey Windows = Microsoft.CreateSubKey("Windows");
            RegistryKey CurrentVersion = Windows.CreateSubKey("CurrentVersion");
            RegistryKey Policies = CurrentVersion.CreateSubKey("Policies");
            RegistryKey Explorer = Policies.CreateSubKey("Explorer");

            //함수 유무 판단            
            object o = Explorer.GetValue("NoControlPanel");

            //레지스트리에 추가
            if (flag == true)
            {
                if (o == null)
                {
                    // 레지스트리에 함수가 없으면 등록
                    
                    Explorer.SetValue("NoControlPanel", 1);
                    Explorer.Close();
                }
                //있으면 아무 동작 하지 않음
                else { }
            }

            //레지스트리 함수삭제
            else
            {
                // 레지스트리 함수가 있으면 삭제
                if (o != null)
                {
                    Explorer.DeleteValue("NoControlPanel");
                    Explorer.Close();
                }
                //없으면 아무 동작 하지 않음
                else { }
            }
        }

        public static void Kill()       //적용 하기.
        {
            Process[] arryProgram = Process.GetProcesses();

            //arryProgram이라는 배열을 선언했습니다. GetProcesses(); 는 실행되고 있는 모든 프로세스들을 Process배열형태로 반환합니다.

            for (int i = 0; i < arryProgram.Length; i++)

            //arryProgram의 개수만큼 반복하는 for문입니다. i는 0부터 시작해서 arryProgram의 개수보다 1작은 숫자까지 반복되겠죠.
            //배열은 0부터 시작하니까요.
            {
                if (arryProgram[i].ProcessName.Equals("explorer"))
                //아까 나왔던 Hwp라는 이름을 가진 프로그램을 발견하면 참이 되겠죠.
                {
                    arryProgram[i].Kill();
                    //그러면 종료하라는 문장입니다.
                }
            }
        }

        #endregion
        public void DeviceFind(string devicename)
        {
            ManagementObjectSearcher deviceList = new ManagementObjectSearcher("Select Name, Status from Win32_PnPEntity");
            string name = string.Empty;
            string status = string.Empty;
            string work = string.Empty;

            // Any results? There should be!
            if (deviceList != null)
                // Enumerate the devices
                foreach (ManagementObject device in deviceList.Get())
                {
                    // To make the example more simple,
                    name = device.GetPropertyValue("Name").ToString();
                    status = device.GetPropertyValue("Status").ToString();

                    if (devicename == "선택안됨")
                    {
                        Name = "선택안됨";
                        Status = "선택안됨";
                        Work = "선택안됨";
                        break;
                    }
                    if (devicename == name)
                    {
                        Console.WriteLine("Device name: {0}", name);
                        Console.WriteLine("\tStatus: {0}", status);
                        Name = name;
                        Status = status;
                        // Part II, Evaluate the device status.
                        bool working = ((status == "OK") || (status == "Degraded") || (status == "Pred Fail"));
                        Console.WriteLine("\tWorking?: {0}", working);
                        if (working == true)
                        {
                            work = "TRUE";
                            Work = work;
                            break;
                        }
                        else if (working == false)
                        {
                            work = "FALSE";
                            Work = work;
                            string sendData = string.Empty;
                            sendData += "CLIENT_DEVICE_RESULT+";
                            sendData += name + "#";
                            sendData += status + "#";
                            sendData += working;
                            byte[] packeter = Encoding.Default.GetBytes(sendData);
                            device_send(packeter);
                            break;
                        }
                    }
                   

                }
        }

        #region 포맷
        public static void SetInfo(string a)
        {
            //// 얻어온 pc 정보들의 count값을 담을 변수      
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            string str = IPHost.AddressList[0].ToString();
            Funtion fun = new Funtion();//........헐;;;;;노답
            string str1 = GetDnsAddresses();
            string[] DnsS = new string[2];
            DnsS = str1.Split('#');


            FileInfo file = new FileInfo(a);
            FileStream fs = file.Create();
            TextWriter tw = new StreamWriter(fs, Encoding.UTF8);
            tw.WriteLine("1"); 
            tw.WriteLine(str);
            tw.WriteLine(GetSubnetMask());
            tw.WriteLine(GetDefaultGateway().ToString());
            tw.WriteLine(GetDnsAdress().ToString());
            tw.WriteLine(DnsS[1]);
            tw.WriteLine(fun.GetMacAddress().ToString());
            tw.Close();
            fs.Close();
        }
        public static void OneKeyGhostExe()
        {

            string formatflag;
            formatflag = "D://FORMATFINFO.txt";

            FileInfo readFile = new FileInfo(formatflag);
            if (!readFile.Exists)
            {
                SetInfo(formatflag);
            }
            
            int hWnd = 0;
            ShellExecute(0, "open", "D:\\OneKeyGhost.exe", "", "", 5);
            while (true)
            {
                hWnd = FindWindow(null, "OneKey Ghost V14.5.8.215 fix");
                if (hWnd != 0)
                    break;
            }
            Console.WriteLine(hWnd);
            System.Threading.Thread.Sleep(30);
            SendMessage(hWnd, WM_COMMAND, 29, 0);

            while (true)
            {
                hWnd = FindWindow(null, "OneKey Ghost");
                if (hWnd != 0)
                    break;
            }
            System.Threading.Thread.Sleep(30);
            SendMessage(hWnd, WM_COMMAND, IDYES, 0);
        }
        #endregion

        #region ip,서브넷,게이트웨이,기본,보조 DNS
        public static void setIP(string ip_address, string subnet_mask)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setIP;
                        ManagementBaseObject newIP =
                            objMO.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = ip_address.Split(',');
                        newIP["SubnetMask"] = new string[] { subnet_mask };
                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);

                        

                    }
                    catch (Exception)
                    {

                    }

                }

            }

        }
        public static string GetSubnetMask()
        {
            IPAddress ip = GetIPAdress();
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (ip.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask.ToString();
                        }
                    }
                }
            }
            throw new ArgumentException(string.Format("Can't find subnetmask for IP address '{0}'", ip));
        }
        public static IPAddress GetIPAdress()
        {
            IPAddress selectIP = null;

            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;

            for (int i = 0; i < addr.Length; i++)
            {
                string[] test = addr[i].ToString().Split('.');
                if (test.Length == 4)
                {
                    selectIP = addr[i];
                    break;
                }
            }

            return selectIP;
        }
        public static IPAddress GetDefaultGateway() //게이트 웨이
        {
            var card = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            if (card == null) return null;
            var address = card.GetIPProperties().GatewayAddresses.FirstOrDefault();
            return address.Address;
        }
        public static IPAddress GetDnsAdress()//기본 DNs
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        return dnsAdress;
                    }
                }
            }
            throw new InvalidOperationException("Unable to find DNS Address");
        }
        public static string GetDnsAddresses()
        {
            string str = string.Empty;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (dnsServers.Count > 0)
                {
                    if (adapter.Description.ToString() == "Realtek PCIe GBE Family Controller" || adapter.Description.ToString() == "Realtek PCIe GBE Family Controller #2")
                        foreach (IPAddress dns in dnsServers)
                        {
                            str += dns.ToString() + "#";
                        }

                }
            }
            return str;
        } //보조 Dns
        #endregion

        public static void SetInfo()
        {
            string formatflag;
            formatflag = "C://FORMATFINFO.txt";
            FileInfo readFile = new FileInfo(formatflag);
            if(readFile.Exists)
            {
                string[] lines = System.IO.File.ReadAllLines(formatflag);
                if (lines[0] == "1")
                {
                    Funtion.setIP(lines[1], lines[2], lines[3]);
                    Funtion.SetDns(lines[4], lines[5]);
                    
                }
            }
           

        }
        #region??
        public static void setIP(string IPAddress, string SubnetMask, string Gateway)
        {

            ManagementClass objMC = new ManagementClass(
                "Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();


            foreach (ManagementObject objMO in objMOC)
            {

                if (!(bool)objMO["IPEnabled"])
                    continue;



                try
                {
                    ManagementBaseObject objNewIP = null;
                    ManagementBaseObject objSetIP = null;
                    ManagementBaseObject objNewGate = null;


                    objNewIP = objMO.GetMethodParameters("EnableStatic");
                    objNewGate = objMO.GetMethodParameters("SetGateways");



                    //Set DefaultGateway
                    objNewGate["DefaultIPGateway"] = new string[] { Gateway };
                    objNewGate["GatewayCostMetric"] = new int[] { 1 };


                    //Set IPAddress and Subnet Mask
                    objNewIP["IPAddress"] = new string[] { IPAddress };
                    objNewIP["SubnetMask"] = new string[] { SubnetMask };

                    objSetIP = objMO.InvokeMethod("EnableStatic", objNewIP, null);
                    objSetIP = objMO.InvokeMethod("SetGateways", objNewGate, null);



                    Console.WriteLine(
                       "Updated IPAddress, SubnetMask and Default Gateway!");



                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Unable to Set IP : " + ex.Message);
                }
            }
        }
        #endregion

        public static void SetDns(string dns1, string dns2)
        {
            try
            {

                ManagementClass mc =
                new ManagementClass("Win32_NetworkAdapterConfiguration");

                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {

                    if ((bool)mo["IPEnabled"])
                    {

                        ManagementBaseObject objdns = mo.GetMethodParameters(
                        "SetDNSServerSearchOrder");

                        if (objdns != null)
                        {
                            string[] s = { dns1, dns2 };

                            objdns[
                            "DNSServerSearchOrder"] = s;

                            mo.InvokeMethod(
                            "SetDNSServerSearchOrder", objdns, null);

                            Console.WriteLine("DNS is set!");
                        }

                    }

                }

            }

            catch (ManagementException e)
            {

                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);

            }

        }

        public bool IsConnectedToInternet()
        {

            int Desc;
            bool result = InternetGetConnectedState(out Desc, 0);

            Console.WriteLine(Desc);
            Console.WriteLine(Convert.ToString(Desc, 16));
            return result;

        }
        #region 소프트웨어
        public static bool installCheak(string programName)
        {
            bool result = false;

            RegistryKey unInstallkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey temp = null;
            string path = string.Empty;
            string displayName = string.Empty;
            string[] subkeyLists = unInstallkey.GetSubKeyNames();
            foreach (string subkey in subkeyLists)
            {
                path = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + subkey;
                temp = Registry.LocalMachine.OpenSubKey(path);
                if (temp != null)
                {
                    displayName = (string)temp.GetValue("DisplayName");
                    if (displayName != null)
                    {
                        //===============================
                        //여기서 부터 수정
                        //Console.WriteLine(displayName); //콘솔 출력

                        if (programName == displayName) //이름이 같은지 체크
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region 소프트웨어 잘 작동하는지 <= 선희
        public static string installLocation(string programName)
        {
            RegistryKey unInstallkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey temp = null;
            string path = string.Empty;
            string displayName = string.Empty;
            string displayIcon = string.Empty;
            string[] subkeyLists = unInstallkey.GetSubKeyNames();
            foreach (string subkey in subkeyLists)
            {
                path = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + subkey;
                temp = Registry.LocalMachine.OpenSubKey(path);
                if (temp != null)
                {
                    displayName = (string)temp.GetValue("DisplayName");
                    if (displayName != null)
                    {
                        if (programName == displayName) //이름이 같은지 체크
                        {
                            displayIcon = (string)temp.GetValue("DisplayIcon");
                            break;
                        }
                    }
                }
            }
            return displayIcon;
        }

        // 응용프로그램 실행
        public static bool processStart(string programName)
        {
            Process myProcess = new Process();
            //string fileLocation = string.Empty; // 파일 위치
            //fileLocation = installLocation(programName);

            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = programName;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.Start();
                return true;
                // This code assumes the process you are starting will terminate itself. 
                // Given that is is started without a window so you cannot terminate it 
                // on the desktop, it must terminate itself or you can do it programmatically
                // from this application using the Kill method.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        // 프로세스 응답 여부 확인
        public static bool processRespond(string programName)
        {
            // 파일 위치 찾기 & 실행
            string fileLocation = string.Empty;
            fileLocation = installLocation(programName);
            bool startResult = processStart(fileLocation);

            if (startResult.ToString() == "True")
            {
                Process[] pro;
                string[] str = fileLocation.Split('\\');
                string[] name = str[str.Length - 1].Split('.');         // name[0] => 프로세스 이름

                pro = Process.GetProcessesByName(name[0]);
                // Test to see if the process is responding.

                if (pro[0].Responding)
                {
                    Console.WriteLine("프로세스 응답 : True");
                    pro[0].Kill();        // 종료
                    return true;
                }
                else
                {
                    Console.WriteLine("프로세스 응답 : False");
                    pro[0].Kill();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 불필요 ^▽^
        #region 초기 세팅 소프트웨어 list
        public static void NeedlessSoftCheck(string[] packetData)
        {
            list1.Clear();

            for (int i = 1; i < packetData.Length; i++)
            {
                list1.Add(packetData[i]);
            }
            AppList();
        }
        #endregion

        #region 현재 설치 된 소프트웨어 list
        public static void AppList()
        {
            RegistryKey unInstallkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey temp = null;
            string path = string.Empty;
            string displayName = string.Empty;
            string[] subkeyLists = unInstallkey.GetSubKeyNames();
            list2 = new List<string>();
            list2.Clear();
            foreach (string subkey in subkeyLists)
            {
                path = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + subkey;
                temp = Registry.LocalMachine.OpenSubKey(path);
                if (temp != null)
                {
                    if ((string)temp.GetValue("DisplayName") != null)
                        list2.Add((string)temp.GetValue("DisplayName"));
                    //for(int i=0; i<list2.Count; i++)
                    //MessageBox.Show(list2[i]);
                }
            }
        }
        #endregion

        #region 초기 세팅 현재 설치 비교
        public static string Compare()
        {
            string str = string.Empty;
            for (int i = 0; i < list2.Count; i++)
            {
                if (list2[i] == null)
                    list2.RemoveAt(i);
            }
            for (int j = 0; j < list1.Count; j++)
            {
                for (int k = 0; k < list2.Count; k++)
                {
                    if (list1[j] == list2[k])
                        list2.RemoveAt(k);
                }
            }
            for (int l = 0; l < list2.Count; l++)
            {
                str += (l + 1).ToString() + "☆" + list2[l] + "☆";
            }
            return str;
        }
        #endregion

        #region 초기 세팅 현재 설치 비교 2
        public static string Compare2(string filename)
        {
            string str = string.Empty;
            list2.Remove(filename);
            for (int l = 0; l < list2.Count; l++)
            {
                str += (l + 1).ToString() + "☆" + list2[l] + "☆";
            }
            return str;
        }
        #endregion

        #region 강제 삭제
        public static bool ForcedDel(string programName)
        {
            bool result = false;

            RegistryKey unInstallkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            RegistryKey temp = null;
            string path = string.Empty;
            string displayName = string.Empty;
            string displayicon = string.Empty;
            string[] subkeyLists = unInstallkey.GetSubKeyNames();
            foreach (string subkey in subkeyLists)
            {
                path = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + subkey;
                temp = Registry.LocalMachine.OpenSubKey(path);
                if (temp != null)
                {
                    displayName = (string)temp.GetValue("DisplayName");
                    displayicon = (string)temp.GetValue("DisplayIcon");
                    if (displayName != null)
                    {
                        if (programName == displayName) //이름이 같은지 체크
                        {
                            string delroute = string.Empty;
                            string[] displayicon2 = displayicon.Split('\\');
                            for (int i = 0; i < displayicon2.Length - 1; i++)
                            {
                                delroute += displayicon2[i];
                                if (i != displayicon2.Length - 2)
                                    delroute += '\\';

                            }

                            fileRouteDel(delroute);
                            Registry.LocalMachine.DeleteSubKey(@path);
                            
                            result = true;
                        }
                    }

                }
                else
                    return result;
            }
            return result;
        }
        #endregion
        #endregion

    }
}
