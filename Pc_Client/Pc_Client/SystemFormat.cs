using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pc_Client
{
    public class SystemFormat
    {
        //복원
        public static void SystemRestore(int num)
        {
            int num1 = 9;

            Thread.Sleep(1000);
            Window();

            Thread.Sleep(5000);

            if (num == 6) 
                num1 = 10;

            for (int i = 0; i < num1; i++)
            {             
                Tab();
                Thread.Sleep(1000);
            }

            if (num == 6) 
                num = 5;

            Enter();

            Thread.Sleep(1000);

            for (int i = 0; i < num; i++)
            {
                Thread.Sleep(200);
                Tab();
            }

            Thread.Sleep(1000);
            Enter();

            Thread.Sleep(1000);
            keybd_event(16, 0, 0, IntPtr.Zero);
            keybd_event(70, 0, 0, IntPtr.Zero);
            keybd_event(70, 0, 2, IntPtr.Zero);
            keybd_event(16, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(79, 0, 0, IntPtr.Zero);
            keybd_event(79, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(82, 0, 0, IntPtr.Zero);
            keybd_event(82, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(77, 0, 0, IntPtr.Zero);
            keybd_event(77, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(65, 0, 0, IntPtr.Zero);
            keybd_event(65, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);
            keybd_event(84, 0, 0, IntPtr.Zero);
            keybd_event(84, 0, 2, IntPtr.Zero);

            Thread.Sleep(200);
            Tab();

            Thread.Sleep(1000);
            Enter();

            Thread.Sleep(10000);
            Enter();

            Thread.Sleep(1000);

            CControl.FinishEvent.Set();
        }

        public static void Start_Backup(int a, string state)
        {
            Thread.Sleep(1000);

            Window();
            Thread.Sleep(3000);

            for (int i = 0; i < 9; i++)
            {
                Tab();
                Thread.Sleep(200);
            }

            Enter();

            #region 복원
            Thread.Sleep(1000);
            Tab();

            Thread.Sleep(1000);
            Enter();

            //==================================
            if (state == "1")
            {
                Thread.Sleep(1000);
                Tab();

                Thread.Sleep(200);
                keybd_event(67, 0, 0, IntPtr.Zero);
                keybd_event(67, 0, 2, IntPtr.Zero);
                Thread.Sleep(200);
                Tab();
            }
            //================================

            Thread.Sleep(3000);
            Enter();

            Thread.Sleep(200);
            Tab();

            if (state == "1") 
            {
                Tab();
                Thread.Sleep(200);
                Tab();
                Thread.Sleep(200);
                Enter();
                Thread.Sleep(500);
                Left();
                Thread.Sleep(200);
                Enter();
            }

            else 
            { 
            //탭탭 엔터
            Thread.Sleep(200);
            keybd_event(77, 0, 0, IntPtr.Zero);
            keybd_event(77, 0, 2, IntPtr.Zero);
            Thread.Sleep(200);

            Tab();

            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(1);
                Down();
            }

            for (int i = 0; i < a; i++)
            {
                if (a == 2)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        Thread.Sleep(200);
                        Up();
                    }
                }
                Thread.Sleep(200);
                Up();
            }

            for (int i = 0; i <= 2; i++)
            {
                Thread.Sleep(200);
                Tab();
            }

            Thread.Sleep(1000);
            Enter();

            Thread.Sleep(1000);
            Enter();
            //for (int i = 0; i < 1; i++)
            //{
            //    Thread.Sleep(200);
            //    Tab();
            //}

            //Thread.Sleep(1000);
            //Enter();

            //Thread.Sleep(1500);
            //Tab();

            //Thread.Sleep(1500);
            //Tab();

            //Thread.Sleep(1500);
            //Tab();

            //Thread.Sleep(1000);
            //Enter();
            #endregion
            }
        }
        // 윈도우 버튼
        static void Window()
        {
            keybd_event(0x5B, 0, 0, IntPtr.Zero);
            keybd_event(0x13, 0, 0, IntPtr.Zero);
            keybd_event(0x5B, 0, 2, IntPtr.Zero);
            keybd_event(0x13, 0, 2, IntPtr.Zero);
        }

        // ← 버튼
        static void Left()
        {
            keybd_event(0x29, 0, 0, IntPtr.Zero);
            keybd_event(0x29, 0, 2, IntPtr.Zero);
        }

        // → 버튼
        static void Right()
        {
            keybd_event(0x27, 0, 0, IntPtr.Zero);
            keybd_event(0x27, 0, 2, IntPtr.Zero);
        }

        // ↓ 버튼
        static void Down()
        {
            keybd_event(0x28, 0, 0, IntPtr.Zero);
            keybd_event(0x28, 0, 2, IntPtr.Zero);
        }

        // ↑ 버튼 4번
        static void Up()
        {
            keybd_event(0x26, 0, 0, IntPtr.Zero);
            keybd_event(0x26, 0, 2, IntPtr.Zero);
        }

        // Enter 버튼
        static void Enter()
        {
            keybd_event(0x0D, 0, 0, IntPtr.Zero);
            keybd_event(0x0D, 0, 2, IntPtr.Zero);
        }

        // Tab 버튼 4번
        static void Tab()
        {
            keybd_event(0x09, 0, 0, IntPtr.Zero);
                keybd_event(0x09, 0, 2, IntPtr.Zero);
        }

        [DllImport("USER32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, int dwExtraInfo);

        [DllImport("USER32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        [DllImport("USER32.dll")]
        public static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        private const int SM_CYCAPTION = 4;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
    }

}
