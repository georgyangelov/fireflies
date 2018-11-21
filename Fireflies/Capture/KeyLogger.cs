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
using System.Windows.Input;

namespace Fireflies.Capture {
    public class KeyLogger : IDisposable {
        public struct KeyAction {
            public Key virtualKey;
            public ScanCode scanCode;
            public bool extendedKey;

            public static KeyAction fromLowLevelStruct(KBDLLHOOKSTRUCT lowLevelStruct) {
                bool extendedKey = (lowLevelStruct.flags & 0x01) != 0;
                ScanCode scanCode = ScanCode.Unknown;
                uint scanCodeInt = lowLevelStruct.scanCode;

                if (extendedKey) {
                    scanCodeInt |= 0xE000;
                }

                if (Enum.IsDefined(typeof(ScanCode), scanCodeInt)) {
                    scanCode = (ScanCode)Enum.ToObject(typeof(ScanCode), scanCodeInt);
                }

                return new KeyAction {
                    virtualKey = KeyInterop.KeyFromVirtualKey(lowLevelStruct.vkCode),
                    scanCode = scanCode,
                    extendedKey = extendedKey
                };
            }
        }

        private IntPtr hookHandle = IntPtr.Zero;
        private bool ignoreEvents = true;

        public ConcurrentQueue<KeyAction> PressedKeys { get; private set; } = new ConcurrentQueue<KeyAction>();

        // This is here to keep a reference, so that it is not garbage-collected.
        private LowLevelKeyboardCallback callback;

        public KeyLogger() {
            callback = hookCallback;
            hookHandle = registerKeyPressHook(callback);
        }

        public void Start() {
            ignoreEvents = false;
        }

        public void Stop() {
            lock(this) {
                ignoreEvents = true;
                PressedKeys = new ConcurrentQueue<KeyAction>();
            }
        }

        public void Dispose() {
            Stop();
        }

        // https://msdn.microsoft.com/en-us/library/ms644985(v=VS.85).aspx
        private IntPtr hookCallback(int nCode, IntPtr eventType, IntPtr lowLevelStructPtr) {
            if (!ignoreEvents && nCode >= 0 && (eventType == (IntPtr)WM_KEYDOWN || eventType == (IntPtr)WM_SYSKEYDOWN)) {
                KBDLLHOOKSTRUCT info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lowLevelStructPtr);
                KeyAction action = KeyAction.fromLowLevelStruct(info);

                lock(this) {
                    PressedKeys.Enqueue(action);
                }
            }

            return CallNextHookEx(hookHandle, nCode, eventType, lowLevelStructPtr);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT {
            public int vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;

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

        public enum ScanCode: uint {
            Unknown = 0xFFFFFF,

            Escape = 0x01,
            D1 = 0x02,
            D2 = 0x03,
            D3 = 0x04,
            D4 = 0x05,
            D5 = 0x06,
            D6 = 0x07,
            D7 = 0x08,
            D8 = 0x09,
            D9 = 0x0A,
            D0 = 0x0B,
            Minus = 0x0C,
            Equals = 0x0D,
            Backspace = 0x0E,
            Tab = 0x0F,
            Q = 0x10,
            W = 0x11,
            E = 0x12,
            R = 0x13,
            T = 0x14,
            Y = 0x15,
            U = 0x16,
            I = 0x17,
            O = 0x18,
            P = 0x19,
            BracketLeft = 0x1A,
            BracketRight = 0x1B,
            Enter = 0x1C,
            LeftControl = 0x1D,
            A = 0x1E,
            S = 0x1F,
            D = 0x20,
            F = 0x21,
            G = 0x22,
            H = 0x23,
            J = 0x24,
            K = 0x25,
            L = 0x26,
            Semicolon = 0x27,
            Apostrophe = 0x28,
            Grave = 0x29,
            LeftShift = 0x2A,
            Backslash = 0x2B,
            Z = 0x2C,
            X = 0x2D,
            C = 0x2E,
            V = 0x2F,
            B = 0x30,
            N = 0x31,
            M = 0x32,
            Comma = 0x33,
            Period = 0x34,
            Slash = 0x35,
            NumpadMultiply = 0x37,
            LeftAlt = 0x38,
            Space = 0x39,
            CapsLock = 0x3A,
            F1 = 0x3B,
            F2 = 0x3C,
            F3 = 0x3D,
            F4 = 0x3E,
            F5 = 0x3F,
            F6 = 0x40,
            F7 = 0x41,
            F8 = 0x42,
            F9 = 0x43,
            F10 = 0x44,
            Pause = 0x45,
            ScrollLock = 0x46,
            Numpad7 = 0x47,
            Numpad8 = 0x48,
            Numpad9 = 0x49,
            NumpadMinus = 0x4A,
            Numpad4 = 0x4B,
            Numpad5 = 0x4C,
            Numpad6 = 0x4D,
            NumpadPlus = 0x4E,
            Numpad1 = 0x4F,
            Numpad2 = 0x50,
            Numpad3 = 0x51,
            Numpad0 = 0x52,
            NumpadPeriod = 0x53,
            F11 = 0x57,
            F12 = 0x58,
            Zoom = 0x62,
            Help = 0x63,
            F13 = 0x64,
            F14 = 0x65,
            F15 = 0x66,
            F16 = 0x67,
            F17 = 0x68,
            F18 = 0x69,
            F19 = 0x6a,
            F20 = 0x6b,
            F21 = 0x6c,
            F22 = 0x6d,
            F23 = 0x6e,
            F24 = 0x76,

            MediaPrevious = 0xE010,
            MediaNext = 0xE019,
            NumpadEnter = 0xE01C,
            RightControl = 0xE01D,
            VolumeMute = 0xE020,
            LaunchApp2 = 0xE021,
            MediaPlay = 0xE022,
            MediaStop = 0xE024,
            VolumeDown = 0xE02E,
            VolumeUp = 0xE030,
            BrowserHome = 0xE032,
            NumpadDivide = 0xE035,
            RightShift = 0xE036,
            PrintScreen = 0xE037,
            RightAlt = 0xE038,
            NumLock = 0xE045,
            Home = 0xE047,
            ArrowUp = 0xE048,
            PageUp = 0xE049,
            ArrowLeft = 0xE04B,
            ArrowRight = 0xE04D,
            End = 0xE04F,
            ArrowDown = 0xE050,
            PageDown = 0xE051,
            Insert = 0xE052,
            Delete = 0xE053,
            MetaLeft = 0xE05B,
            MetaRight = 0xE05C,
            Application = 0xE05D,
            Power = 0xE05E,
            Sleep = 0xE05F,
            Wake = 0xE063,
            BrowserSearch = 0xE065,
            BrowserFavorites = 0xE066,
            BrowserRefresh = 0xE067,
            BrowserStop = 0xE068,
            BrowserForward = 0xE069,
            BrowserBack = 0xE06A,
            LaunchApp1 = 0xE06B,
            LaunchEmail = 0xE06C,
            LaunchMedia = 0xE06D
        }
    }
}
