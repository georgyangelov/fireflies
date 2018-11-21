using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Fireflies.Capture.KeyLogger;

namespace Fireflies.Choreographers.Keyboard {
    public class KeyMapping {
        private Dictionary<Key, List<int>> reverseMapping = new Dictionary<Key, List<int>>();
        private Dictionary<ScanCode, int> reverseScanCodeMapping = new Dictionary<ScanCode, int>();
        private List<int> EMPTY_LIST = new List<int>();

        public KeyMapping() {
            for (var i = 0; i < MAPPING.Length; i++) {
                if (reverseMapping.ContainsKey(MAPPING[i])) {
                    reverseMapping[MAPPING[i]].Add(i);
                } else {
                    reverseMapping.Add(MAPPING[i], new List<int>() { i });
                }
            }

            for (var i = 0; i < SCAN_CODE_MAPPING.Length; i++) {
                reverseScanCodeMapping[SCAN_CODE_MAPPING[i]] = i;
            }
        }

        public Key keyForIndex(int i) {
            return MAPPING[i];
        }

        public List<int> indexForKey(Key key) {
            return GetValueOrDefault(reverseMapping, key, EMPTY_LIST);
        }

        public ScanCode scanCodeForIndex(int i) {
            return SCAN_CODE_MAPPING[i];
        }

        public int indexForScanCode(ScanCode scanCode) {
            return GetValueOrDefault(reverseScanCodeMapping, scanCode, 0);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
            TValue result;

            if (dictionary.TryGetValue(key, out result)) {
                return result;
            } else {
                Console.WriteLine("Unknown key " + key);
                return defaultValue;
            }
        }

        private Key[] MAPPING = {
            Key.Escape,
            Key.F1,
            Key.F2,
            Key.F3,
            Key.F4,
            Key.F5,
            Key.F6,
            Key.F7,
            Key.F8,
            Key.F9,
            Key.F10,
            Key.F11,
            Key.F12,
            Key.PrintScreen,
            Key.Scroll,
            Key.Pause,

            Key.F13,
            Key.OemTilde,
            Key.D1,
            Key.D2,
            Key.D3,
            Key.D4,
            Key.D5,
            Key.D6,
            Key.D7,
            Key.D8,
            Key.D9,
            Key.D0,
            Key.OemMinus,
            Key.OemPlus,
            Key.Back,
            Key.Insert,
            Key.Home,
            Key.PageUp,
            Key.NumLock,
            Key.Divide,
            Key.Multiply,
            Key.Subtract,

            Key.F14,
            Key.Tab,
            Key.Q,
            Key.W,
            Key.E,
            Key.R,
            Key.T,
            Key.Y,
            Key.U,
            Key.I,
            Key.O,
            Key.P,
            Key.OemOpenBrackets,
            Key.OemCloseBrackets,
            Key.OemPipe,
            Key.Delete,
            Key.End,
            Key.PageDown,
            Key.NumPad7,
            Key.NumPad8,
            Key.NumPad9,
            Key.Add,

            Key.F15,
            Key.CapsLock,
            Key.A,
            Key.S,
            Key.D,
            Key.F,
            Key.G,
            Key.H,
            Key.J,
            Key.K,
            Key.L,
            Key.OemSemicolon,
            Key.OemQuotes,
            Key.Enter,
            Key.NumPad4,
            Key.NumPad5,
            Key.NumPad6,

            Key.F16,
            Key.LeftShift,
            Key.Z,
            Key.X,
            Key.C,
            Key.V,
            Key.B,
            Key.N,
            Key.M,
            Key.OemComma,
            Key.OemPeriod,
            Key.OemQuestion,
            Key.RightShift,
            Key.Up,
            Key.NumPad1,
            Key.NumPad2,
            Key.NumPad3,
            // TODO: Num pad enter?
            Key.Return,

            Key.F17,
            Key.LeftCtrl,
            Key.LWin,
            Key.LeftAlt,
            Key.Space,
            Key.RightAlt,
            // Function key
            Key.F18,
            Key.Apps,
            Key.RightCtrl,
            Key.Left,
            Key.Down,
            Key.Right,
            Key.NumPad0,
            Key.Decimal,

            // Logo
            Key.F19
        };

        private ScanCode[] SCAN_CODE_MAPPING = {
            ScanCode.Escape,
            ScanCode.F1,
            ScanCode.F2,
            ScanCode.F3,
            ScanCode.F4,
            ScanCode.F5,
            ScanCode.F6,
            ScanCode.F7,
            ScanCode.F8,
            ScanCode.F9,
            ScanCode.F10,
            ScanCode.F11,
            ScanCode.F12,
            ScanCode.PrintScreen,
            ScanCode.ScrollLock,
            ScanCode.Pause,

            ScanCode.F13,
            ScanCode.Grave,
            ScanCode.D1,
            ScanCode.D2,
            ScanCode.D3,
            ScanCode.D4,
            ScanCode.D5,
            ScanCode.D6,
            ScanCode.D7,
            ScanCode.D8,
            ScanCode.D9,
            ScanCode.D0,
            ScanCode.Minus,
            ScanCode.Equals,
            ScanCode.Backspace,
            ScanCode.Insert,
            ScanCode.Home,
            ScanCode.PageUp,
            ScanCode.NumLock,
            ScanCode.NumpadDivide,
            ScanCode.NumpadMultiply,
            ScanCode.NumpadMinus,

            ScanCode.F14,
            ScanCode.Tab,
            ScanCode.Q,
            ScanCode.W,
            ScanCode.E,
            ScanCode.R,
            ScanCode.T,
            ScanCode.Y,
            ScanCode.U,
            ScanCode.I,
            ScanCode.O,
            ScanCode.P,
            ScanCode.BracketLeft,
            ScanCode.BracketRight,
            ScanCode.Backslash,
            ScanCode.Delete,
            ScanCode.End,
            ScanCode.PageDown,
            ScanCode.Numpad7,
            ScanCode.Numpad8,
            ScanCode.Numpad9,
            ScanCode.NumpadPlus,

            ScanCode.F15,
            ScanCode.CapsLock,
            ScanCode.A,
            ScanCode.S,
            ScanCode.D,
            ScanCode.F,
            ScanCode.G,
            ScanCode.H,
            ScanCode.J,
            ScanCode.K,
            ScanCode.L,
            ScanCode.Semicolon,
            ScanCode.Apostrophe,
            ScanCode.Enter,
            ScanCode.Numpad4,
            ScanCode.Numpad5,
            ScanCode.Numpad6,

            ScanCode.F16,
            ScanCode.LeftShift,
            ScanCode.Z,
            ScanCode.X,
            ScanCode.C,
            ScanCode.V,
            ScanCode.B,
            ScanCode.N,
            ScanCode.M,
            ScanCode.Comma,
            ScanCode.Period,
            ScanCode.Slash,
            ScanCode.RightShift,
            ScanCode.ArrowUp,
            ScanCode.Numpad1,
            ScanCode.Numpad2,
            ScanCode.Numpad3,
            ScanCode.NumpadEnter,

            ScanCode.F17,
            ScanCode.LeftControl,
            ScanCode.MetaLeft,
            ScanCode.LeftAlt,
            ScanCode.Space,
            ScanCode.RightAlt,
            // Function key
            ScanCode.F18,
            ScanCode.Application,
            ScanCode.RightControl,
            ScanCode.ArrowLeft,
            ScanCode.ArrowDown,
            ScanCode.ArrowRight,
            ScanCode.Numpad0,
            ScanCode.NumpadPeriod,

            // Logo
            ScanCode.Unknown
        };
    }
}
