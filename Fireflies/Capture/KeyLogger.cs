using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fireflies.Capture {
    public class KeyLogger : IDisposable {
        private static IntPtr hookHandle = IntPtr.Zero;

        public ConcurrentQueue<Keys> PressedKeys { get; private set; } = new ConcurrentQueue<Keys>();

        // This is here to keep a reference, so that it is not garbage-collected.
        private LowLevelKeyboardCallback callback;

        public KeyLogger() {
            callback = hookCallback;
            hookHandle = registerKeyPressHook(callback);
        }

        void IDisposable.Dispose() {
            UnhookWindowsHookEx(hookHandle);
        }

        // https://msdn.microsoft.com/en-us/library/ms644985(v=VS.85).aspx
        private IntPtr hookCallback(int nCode, IntPtr eventType, IntPtr keyCodePtr) {
            if (nCode >= 0 && eventType.ToInt64() == WM_KEYDOWN) {
                int virtualKeyCode = Marshal.ReadInt32(keyCodePtr);
                var key = (Keys)virtualKeyCode;

                PressedKeys.Enqueue(key);
            }

            return CallNextHookEx(hookHandle, nCode, eventType, keyCodePtr);
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private IntPtr registerKeyPressHook(LowLevelKeyboardCallback proc) {
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule) {
                return SetWindowsHookEx(
                    WH_KEYBOARD_LL, 
                    proc,
                    GetModuleHandle(module.ModuleName), 
                    0
                );
            }
        }

        private delegate IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardCallback lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
