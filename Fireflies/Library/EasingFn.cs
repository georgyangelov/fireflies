using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Library {
    static class EasingFn {
        public static EasingFunction linear() {
            return progress => progress;
        }

        public static EasingFunction sineIn() {
            return x => 1 - (float)Math.Cos(x * Math.PI / 2);
        }

        public static EasingFunction sineOut() {
            return x => (float)Math.Sin(x * Math.PI / 2);
        }

        public static EasingFunction sineInOut() {
            return x => (float)(1 - Math.Cos(x * Math.PI)) / 2;
        }

        public static EasingFunction polynomialIn(double exponent) {
            return x => (float)Math.Pow(x, exponent);
        }

        public static EasingFunction polynomialOut(double exponent) {
            return x => (float)(1f - Math.Pow(1 - x, exponent));
        }

        public static EasingFunction polynomialInOut(double exponent) {
            var easeIn = polynomialIn(exponent);
            var easeOut = polynomialOut(exponent);

            return x => {
                if (x < 0.5) {
                    return 0.5f * easeIn(2 * x);
                } else {
                    return 0.5f + 0.5f * easeOut((x - 0.5f) * 2);
                }
            };
        }
    }
}
