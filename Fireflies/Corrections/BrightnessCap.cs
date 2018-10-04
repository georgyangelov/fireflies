using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Corrections {
    public class BrightnessCap {
        public static Color correct(Color color, float maxBrightness) {
            byte maxColorValue = Math.Max(color.R, Math.Max(color.G, color.B));
            float clampFactor = maxBrightness / (maxColorValue / (float)255);

            if (clampFactor >= 1) {
                return color;
            }

            return new Color {
                A = color.A,
                R = (byte)(color.R * clampFactor),
                G = (byte)(color.G * clampFactor),
                B = (byte)(color.B * clampFactor)
            };
        }
    }
}
