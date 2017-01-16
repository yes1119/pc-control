using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Pc_Client
{
    class ProcessCheck : IDisposable
    {
        bool _isRunning = false;

        List<string> _lstCatchProcName;
        ManagementEventWatcher _createWatcher;
        ManagementEventWatcher _deleteWatcher;

        public static string[] create_name;
        public static string[] delete_name;
        public static string compare_name;

        public static int[] create_count;
        public static int[] delete_count;

        public event ProcessCheckedEventHandler ProcessChecked;

        public List<string> CatchNameList { get { return _lstCatchProcName; } }

        public ProcessCheck(ushort interval)
        {
            _lstCatchProcName = new List<string>();

            _createWatcher = new ManagementEventWatcher(
                "SELECT * FROM __InstanceCreationEvent WITHIN " + interval + " WHERE TargetInstance isa \"Win32_Process\"");
            _deleteWatcher = new ManagementEventWatcher(
                "SELECT * FROM __InstanceDeletionEvent WITHIN " + interval + " WHERE TargetInstance isa \"Win32_Process\"");

            _createWatcher.EventArrived += createWatcher_EventArrived;
            _deleteWatcher.EventArrived += deleteWatcher_EventArrived;
        }
        public ProcessCheck(string name, ushort interval)
            : this(interval)
        {
            AddCheckList(name);
        }
        public ProcessCheck(string[] names, ushort interval)
            : this(interval)
        {
            AddCheckList(names);
            create_name = new string[names.Length];
            delete_name = new string[names.Length];
            create_count = new int[names.Length];
            delete_count = new int[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                create_name[i] = names[i];
                delete_name[i] = names[i];
                create_count[i] = 0;
                delete_count[i] = 0;
            }

            Console.WriteLine(names.Length);
        }


        public bool StartWatcher()
        {
            if (_lstCatchProcName.Count > 0 && ProcessChecked != null)
            {
                _isRunning = true;
                _createWatcher.Start();
                _deleteWatcher.Start();

                return true;
            }
            else
                return false;
        }

        public void StopWatcher()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _createWatcher.Stop();
                _deleteWatcher.Stop();
            }
        }

        public void AddCheckList(string procName)
        {
            if (_isRunning)
            {
                _createWatcher.Stop();
                _deleteWatcher.Stop();
            }

            //procName = procName.ToLower();

            if (!procName.EndsWith(".exe"))
                procName += ".exe";

            if (_lstCatchProcName.IndexOf(procName) < 0)
                _lstCatchProcName.Add(procName);

            if (_isRunning)
            {
                _createWatcher.Start();
                _deleteWatcher.Start();
            }
        }

        public void AddCheckList(string[] procNames)
        {
            if (_isRunning)
            {
                _createWatcher.Stop();
                _deleteWatcher.Stop();
            }

            string procName = "";
            for (int i = 0; i < procNames.Length; i++)
            {
                //procName = procNames[i].ToLower();


                if (!procNames[i].EndsWith(".exe"))
                    procNames[i] += ".exe";

                if (_lstCatchProcName.IndexOf(procNames[i]) < 0)
                    _lstCatchProcName.Add(procNames[i]);

            }

            if (_isRunning)
            {
                _createWatcher.Start();
                _deleteWatcher.Start();
            }
        }

        public void RemoveCheckList(string procName)
        {
            int index = _lstCatchProcName.IndexOf(procName);

            if (index >= 0)
                _lstCatchProcName.RemoveAt(index);
        }

        public void ClearCheckList()
        {
            StopWatcher();
            _lstCatchProcName.Clear();
        }

        public void Dispose()
        {
            if (_isRunning)
            {
                _createWatcher.Stop();
                _deleteWatcher.Stop();
            }

            _createWatcher.Dispose();
            _deleteWatcher.Dispose();
        }

        private void createWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {

            ManagementBaseObject mObj = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            ProcessCheckedEventArgs args = makeEventArgs(mObj, CheckedStatus.Created);

            for (int i = 0; i < create_name.Length; i++)
            {
                if (create_name[i] == compare_name)
                    create_count[i]++;

                if (create_count[i] == 1)
                {
                    ProcessChecked(this, args);
                    mObj.Dispose();
                }
            }
        }

        private void deleteWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {

            ManagementBaseObject mObj = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            ProcessCheckedEventArgs args = makeEventArgs(mObj, CheckedStatus.Deleted);

            for (int i = 0; i < delete_name.Length; i++)
            {
                if (delete_name[i] == compare_name)
                {
                    delete_count[i]++;

                    if (delete_count[i] == create_count[i])
                    {
                        ProcessChecked(this, args);
                        mObj.Dispose();
                    }
                }
            }
        }

        private ProcessCheckedEventArgs makeEventArgs(ManagementBaseObject mObj, CheckedStatus status)
        {
            string procName = mObj.Properties["Name"].Value.ToString();
            compare_name = procName;

            if (_lstCatchProcName.IndexOf(procName) >= 0)
            {
                string exePath = mObj.Properties["ExecutablePath"].Value.ToString();

                ProcessCheckedEventArgs args = new ProcessCheckedEventArgs(status, procName, exePath);
                return args;
            }
            else
                return null;
        }
    }

    public enum CheckedStatus { Created, Deleted };

    public delegate void ProcessCheckedEventHandler(object sender, ProcessCheckedEventArgs e);

    public class ProcessCheckedEventArgs : EventArgs
    {
        public CheckedStatus Status { get; private set; }
        public string RegisterdProcessName { get; private set; }
        public string ExecutePath { get; private set; }

        public ProcessCheckedEventArgs(CheckedStatus status, string regedProcName, string path)
        {
            Status = status;
            RegisterdProcessName = regedProcName;
            ExecutePath = path;
        }
    }
}
