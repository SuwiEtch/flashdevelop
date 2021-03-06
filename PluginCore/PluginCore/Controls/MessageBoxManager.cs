﻿#pragma warning disable 0618
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
namespace System.Windows.Forms
{
    public class MessageBoxManager
    {
        delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        const int WH_CALLWNDPROCRET = 12;
        const int WM_DESTROY = 0x0002;
        const int WM_INITDIALOG = 0x0110;
        const int WM_TIMER = 0x0113;
        const int WM_USER = 0x400;
        const int DM_GETDEFID = WM_USER + 0;

        const int MBOK = 1;
        const int MBCancel = 2;
        const int MBAbort = 3;
        const int MBRetry = 4;
        const int MBIgnore = 5;
        const int MBYes = 6;
        const int MBNo = 7;


        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

        [DllImport("user32.dll")]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        static extern int GetDlgCtrlID(IntPtr hwndCtl);

        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
        static extern bool SetWindowText(IntPtr hWnd, string lpString);


        [StructLayout(LayoutKind.Sequential)]
        public struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        };

        static readonly HookProc hookProc = MessageBoxHookProc;
        static readonly EnumChildProc enumProc = MessageBoxEnumProc;
        [ThreadStatic] static IntPtr hHook = IntPtr.Zero;
        [ThreadStatic] static int nButton;

        /// <summary>
        /// OK text
        /// </summary>
        public static string OK = "&OK";
        /// <summary>
        /// Cancel text
        /// </summary>
        public static string Cancel = "&Cancel";
        /// <summary>
        /// Abort text
        /// </summary>
        public static string Abort = "&Abort";
        /// <summary>
        /// Retry text
        /// </summary>
        public static string Retry = "&Retry";
        /// <summary>
        /// Ignore text
        /// </summary>
        public static string Ignore = "&Ignore";
        /// <summary>
        /// Yes text
        /// </summary>
        public static string Yes = "&Yes";
        /// <summary>
        /// No text
        /// </summary>
        public static string No = "&No";

        /// <summary>
        /// Enables MessageBoxManager functionality
        /// </summary>
        /// <remarks>
        /// MessageBoxManager functionality is enabled on current thread only.
        /// Each thread that needs MessageBoxManager functionality has to call this method.
        /// </remarks>
        public static void Register()
        {
            if (hHook != IntPtr.Zero)
                throw new NotSupportedException("One hook per thread allowed.");
            hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
        }

        /// <summary>
        /// Disables MessageBoxManager functionality
        /// </summary>
        /// <remarks>
        /// Disables MessageBoxManager functionality on current thread only.
        /// </remarks>
        public static void Unregister()
        {
            if (hHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;
            }
        }

        static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(hHook, nCode, wParam, lParam);

            CWPRETSTRUCT msg = Marshal.PtrToStructure<CWPRETSTRUCT>(lParam);
            IntPtr hook = hHook;

            if (msg.message == WM_INITDIALOG)
            {
                var sb = new StringBuilder(10);
                GetClassName(msg.hwnd, sb, sb.Capacity);
                if (sb.ToString() == "#32770")
                {
                    nButton = 0;
                    EnumChildWindows(msg.hwnd, enumProc, IntPtr.Zero);
                    if (nButton == 1)
                    {
                        var hButton = GetDlgItem(msg.hwnd, MBCancel);
                        if (hButton != IntPtr.Zero)
                            SetWindowText(hButton, OK);
                    }
                }
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        static bool MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            var sb = new StringBuilder(10);
            GetClassName(hWnd, sb, sb.Capacity);
            if (sb.ToString() == "Button")
            {
                int ctlId = GetDlgCtrlID(hWnd);
                switch (ctlId)
                {
                    case MBOK:
                        SetWindowText(hWnd, OK);
                        break;
                    case MBCancel:
                        SetWindowText(hWnd, Cancel);
                        break;
                    case MBAbort:
                        SetWindowText(hWnd, Abort);
                        break;
                    case MBRetry:
                        SetWindowText(hWnd, Retry);
                        break;
                    case MBIgnore:
                        SetWindowText(hWnd, Ignore);
                        break;
                    case MBYes:
                        SetWindowText(hWnd, Yes);
                        break;
                    case MBNo:
                        SetWindowText(hWnd, No);
                        break;
                }
                nButton++;
            }
            return true;
        }
    }
}