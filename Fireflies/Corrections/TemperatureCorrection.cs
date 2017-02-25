using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Corrections {
    public static class TemperatureCorrection {
        public static Color correct(Color c, Color scale) {
            return new Color {
                A = c.A,
                R = clampByte((int)(c.R * scale.R / 255f)),
                G = clampByte((int)(c.G * scale.G / 255f)),
                B = clampByte((int)(c.B * scale.B / 255f))
            };
        }

        public static Color correctLinear(Color c, int correction) {
            return new Color {
                A = c.A,
                R = clampByte(c.R + correction),
                G = clampByte(c.G),
                B = clampByte(c.B - correction)
            };
        }

        private static HSLColor getColorForTemperature(float temperature) {
            double r, g, b;

            temperature = temperature / 100;

            if (temperature <= 66) {
                r = 255;
            } else {
                r = temperature - 60;
                r = 329.698727446 * Math.Pow(r, -0.1332047592);
            }

            if (temperature <= 66) {
                g = temperature;
                g = 99.4708025861 * Math.Log(g) - 161.1195681661;
            } else {
                g = temperature - 60;
                g = 288.1221695283 * Math.Pow(g, -0.0755148492);
            }

            if (temperature >= 66) {
                b = 255;
            } else if (temperature <= 19) {
                b = 0;
            } else {
                b = temperature - 10;
                b = 138.5177312231 * Math.Log(b) - 305.0447927307;
            }

            return (HSLColor)new Color {
                R = clampByte((int)r),
                G = clampByte((int)g),
                B = clampByte((int)b)
            };
        }

        private static byte clampByte(int value) {
            if (value < 0) {
                return 0;
            } else if (value > 255) {
                return 255;
            } else {
                return (byte)value;
            }
        }
    }
}
