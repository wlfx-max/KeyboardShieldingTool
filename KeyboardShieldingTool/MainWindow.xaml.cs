using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace KeyboardShieldingTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isKeyboardDisabled = false; // 控制键盘输入的布尔变量
        private IntPtr hookId = IntPtr.Zero;
        private LowLevelKeyboardProc proc;

        public MainWindow()
        {
            InitializeComponent();
            UpdateKeyboardStatus();
            proc = HookCallback;
        }

        private void DisableKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            isKeyboardDisabled = true;
            hookId = SetHook(proc);
            UpdateKeyboardStatus();
        }

        private void EnableKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            isKeyboardDisabled = false;
            UnhookWindowsHookEx(hookId);
            UpdateKeyboardStatus();
        }

        private void UpdateKeyboardStatus()
        {
            KeyboardStatusTextBox.Text = isKeyboardDisabled ? "键盘输入已禁用" : "键盘输入已启用";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWindowsHookEx(hookId);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && isKeyboardDisabled)
            {
                return (IntPtr)1; // 阻止键盘输入
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
    }
}