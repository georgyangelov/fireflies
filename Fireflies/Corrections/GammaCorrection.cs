using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Corrections {
    public static class GammaCorrection {
        public static Color correct(Color color, float gamma) {
            return new Color {
                A = color.A,
                R = (byte)(255 * Math.Pow(color.R / (double)255, gamma)),
                G = (byte)(255 * Math.Pow(color.G / (double)255, gamma)),
                B = (byte)(255 * Math.Pow(color.B / (double)255, gamma))
            };
        }
    }
}
