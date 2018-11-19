using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fireflies.Choreographers.Keyboard {
    public class KeyMapping {
        private Dictionary<Keys, List<int>> reverseMapping = new Dictionary<Keys, List<int>>();
        private List<int> EMPTY_LIST = new List<int>();

        public KeyMapping() {
            for (var i = 0; i < MAPPING.Length; i++) {
                if (reverseMapping.ContainsKey(MAPPING[i])) {
                    reverseMapping[MAPPING[i]].Add(i);
                } else {
                    reverseMapping.Add(MAPPING[i], new List<int>() { i });
                }
            }
        }

        public Keys keyForIndex(int i) {
            return MAPPING[i];
        }

        public List<int> indexForKey(Keys key) {
            return GetValueOrDefault(reverseMapping, key, EMPTY_LIST);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue) {
            TValue result;

            if (dictionary.TryGetValue(key, out result)) {
                return result;
            } else {
                return defaultValue;
            }
        }

        private Keys[] MAPPING = {
            Keys.Escape,
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
            Keys.F9,
            Keys.F10,
            Keys.F11,
            Keys.F12,
            Keys.PrintScreen,
            Keys.Scroll,
            Keys.Pause,

            Keys.F13,
            Keys.Oemtilde,
            Keys.D1,
            Keys.D2,
            Keys.D3,
            Keys.D4,
            Keys.D5,
            Keys.D6,
            Keys.D7,
            Keys.D8,
            Keys.D9,
            Keys.D0,
            Keys.OemMinus,
            Keys.Oemplus,
            Keys.Back,
            Keys.Insert,
            Keys.Home,
            Keys.PageUp,
            Keys.NumLock,
            Keys.Divide,
            Keys.Multiply,
            Keys.Subtract,
            
            Keys.F14,
            Keys.Tab,
            Keys.Q,
            Keys.W,
            Keys.E,
            Keys.R,
            Keys.T,
            Keys.Y,
            Keys.U,
            Keys.I,
            Keys.O,
            Keys.P,
            Keys.OemOpenBrackets,
            Keys.OemCloseBrackets,
            Keys.OemBackslash,
            Keys.Delete,
            Keys.End,
            Keys.PageDown,
            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.Add,

            Keys.F15,
            Keys.CapsLock,
            Keys.A,
            Keys.S,
            Keys.D,
            Keys.F,
            Keys.G,
            Keys.H,
            Keys.J,
            Keys.K,
            Keys.L,
            Keys.OemSemicolon,
            Keys.OemQuotes,
            Keys.Enter,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            
            Keys.F16,
            Keys.LShiftKey,
            Keys.Z,
            Keys.X,
            Keys.C,
            Keys.V,
            Keys.B,
            Keys.N,
            Keys.M,
            Keys.Oemcomma,
            Keys.OemPeriod,
            // Ummm, backslash?
            Keys.OemBackslash,
            Keys.RShiftKey,
            Keys.Up,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            // TODO: Num pad enter?
            Keys.OemClear,

            Keys.F17,
            Keys.LControlKey,
            Keys.LWin,
            Keys.LMenu,
            Keys.Space,
            Keys.RMenu,
            // Function key
            Keys.F18,
            // Menu key
            Keys.RMenu,
            Keys.RControlKey,
            Keys.Left,
            Keys.Down,
            Keys.Right,
            Keys.NumPad0,
            // Numpad period
            Keys.OemPeriod,
            
            // Logo
            Keys.F19
        };
    }
}
