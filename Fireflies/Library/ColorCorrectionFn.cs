using Fireflies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Library {
    static class ColorCorrectionFn {
        public static ColorCorrectionFunction compose(params ColorCorrectionFunction[] fns) {
            return color => {
                foreach (var fn in fns) {
                    color = fn(color);
                }

                return color;
            };
        }

        public static ColorCorrectionFunction limitBrightness(float maxBrightness) {
            return color => {
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
            };
        }

        public static ColorCorrectionFunction scaleBrightness(float factor) {
            return color => new Color {
                A = color.A,
                R = NumberFn.clampToByte((int)(color.R * factor)),
                G = NumberFn.clampToByte((int)(color.G * factor)),
                B = NumberFn.clampToByte((int)(color.B * factor))
            };
        }

        public static ColorCorrectionFunction correctGamma(float gamma) {
            return color => new Color {
                A = color.A,
                R = (byte)(255 * Math.Pow(color.R / (double)255, gamma)),
                G = (byte)(255 * Math.Pow(color.G / (double)255, gamma)),
                B = (byte)(255 * Math.Pow(color.B / (double)255, gamma))
            };
        }

        public static ColorCorrectionFunction correctGamma(float gammaR, float gammaG, float gammaB) {
            return color => new Color {
                A = color.A,
                R = (byte)(255 * Math.Pow(color.R / (double)255, gammaR)),
                G = (byte)(255 * Math.Pow(color.G / (double)255, gammaG)),
                B = (byte)(255 * Math.Pow(color.B / (double)255, gammaB))
            };
        }

        public static ColorCorrectionFunction correctTemperature(Color scale) {
            return color => new Color {
                A = color.A,
                R = NumberFn.clampToByte((int)(color.R * scale.R / 255f)),
                G = NumberFn.clampToByte((int)(color.G * scale.G / 255f)),
                B = NumberFn.clampToByte((int)(color.B * scale.B / 255f))
            };
        }

        public static ColorCorrectionFunction correctTemperatureLinear(int correction) {
            return color => new Color {
                A = color.A,
                R = NumberFn.clampToByte(color.R + correction),
                G = NumberFn.clampToByte(color.G),
                B = NumberFn.clampToByte(color.B - correction)
            };
        }
    }
}
