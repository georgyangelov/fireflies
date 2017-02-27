using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Corrections {
    public static class BrightnessCorrection {
        public static Color correct(Color c, double brightness) {
            return new Color {
                A = c.A,
                R = clampByte((int)(c.R * brightness)),
                G = clampByte((int)(c.G * brightness)),
                B = clampByte((int)(c.B * brightness))
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
