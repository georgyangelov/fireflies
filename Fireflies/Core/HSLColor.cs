using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Corrections {
    public struct HSLColor {
        public float H, S, L;

        public HSLColor(float hue, float saturation, float lightness) {
            H = hue;
            S = saturation;
            L = lightness;
        }

        public static explicit operator Color(HSLColor c) {
            float R = 0, G = 0, B = 0;

            float C = (1 - Math.Abs(2 * c.L - 1)) * c.S,
                  H = c.H * 6,
                  X = C * (1 - Math.Abs(H % 2 - 1));

            if (0 <= H && H < 1) {
                R = C;
                G = X;
            } else if (1 <= H && H < 2) {
                R = X;
                G = C;
            } else if (2 <= H && H < 3) {
                G = C;
                B = X;
            } else if (3 <= H && H < 4) {
                G = X;
                B = C;
            } else if (4 <= H && H < 5) {
                R = X;
                B = C;
            } else { // 5 <= H && H < 6
                R = C;
                B = X;
            }

            float m = c.L - C / 2;

            return new Color {
                A = 255,
                R = (byte)Math.Round(255 * (R + m)),
                G = (byte)Math.Round(255 * (G + m)),
                B = (byte)Math.Round(255 * (B + m))
            };
        }

        public static explicit operator HSLColor(Color c) {
            float H, S;
            byte M = Math.Max(Math.Max(c.R, c.G), c.B),
                 m = Math.Min(Math.Min(c.R, c.G), c.B),
                 C = (byte)(M - m),
                 L = (byte)((M + m) / 2);

            if (C == 0) {
                H = 0;
            } else if (M == c.R) {
                H = (c.G - c.B) / (float)C % 6;
            } else if (M == c.G) {
                H = (c.B - c.R) / (float)C + 2;
            } else { // M == c.B
                H = (c.R - c.G) / (float)C + 4;
            }

            if (L == 0 || L == 255) {
                S = 0;
            } else if (L < 127) {
                S = C / (float)(M + m);
            } else {
                S = C / (float)(2 * 255 - M - m);
            }

            return new HSLColor {
                H = moveIntoRange(H / 6),
                S = S,
                L = L / 255f
            };
        }

        private static float moveIntoRange(float value) {
            if (value < 0) {
                return value + 1f;
            } else if (value > 1f) {
                return value - 1f;
            } else {
                return value;
            }
        }
    }
}
