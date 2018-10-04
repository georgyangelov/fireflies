using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Functions.Color {
    public class Helpers {
        public static System.Windows.Media.Color crossfade(System.Windows.Media.Color a, System.Windows.Media.Color b, double factor) {
            if (factor < 0) {
                factor = 0;
            } else if (factor > 1) {
                factor = 1;
            }

            return new System.Windows.Media.Color() {
                R = (byte)(a.R * (1 - factor) + b.R * factor),
                G = (byte)(a.G * (1 - factor) + b.G * factor),
                B = (byte)(a.B * (1 - factor) + b.B * factor),
                A = 255
            };
        }

        public static System.Windows.Media.Color darken(System.Windows.Media.Color color, float factor) {
            return crossfade(color, Colors.Black, factor);
        }
    }
}
