using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Library {
    static class ColorFn {
        public static Color crossfade(Color a, Color b, float factor) {
            if (factor < 0) {
                factor = 0;
            } else if (factor > 1) {
                factor = 1;
            }

            return new Color() {
                R = (byte)(a.R * (1 - factor) + b.R * factor),
                G = (byte)(a.G * (1 - factor) + b.G * factor),
                B = (byte)(a.B * (1 - factor) + b.B * factor),
                A = (byte)(a.A * (1 - factor) + b.A * factor)
            };
        }

        public static Color crossfade(Color[] colors, float factor) {
            factor = factor % 1;

            if (factor < 0) {
                factor += 1;
            }

            int indexA = (int)(colors.Length * factor);
            int indexB = indexA + 1;

            Color colorA = colors[indexA];
            Color colorB = colors[indexB % colors.Length];

            float colorFactor = factor * colors.Length - indexA;

            return crossfade(colorA, colorB, colorFactor);
        }

        public static Color darken(Color color, float factor) {
            return crossfade(color, Colors.Black, factor);
        }

        public static Color lighten(Color color, float factor) {
            return crossfade(color, Colors.White, factor);
        }

        public static Color resetAlpha(Color color) {
            return new Color {
                A = 255,
                R = color.R,
                G = color.G,
                B = color.B
            };
        }

        public static Color alphaen(Color color, float factor) {
            return new Color() {
                R = color.R,
                G = color.G,
                B = color.B,
                A = (byte)(color.A * (1 - factor))
            };
        }

        public static Color mask(Color color, Color mask) {
            return new Color() {
                R = color.R,
                G = color.G,
                B = color.B,
                A = mask.A
            };
        }

        public static Color blendSecondWithAlpha(Color a, Color b) {
            return crossfade(a, resetAlpha(b), (float)b.A / 255);
        }
    }
}
