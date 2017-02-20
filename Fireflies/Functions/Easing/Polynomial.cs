using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Functions.Easing {
    class Polynomial {
        private double exponent;

        public Polynomial(double exponent) {
            this.exponent = exponent;
        }

        public double EaseIn(double fraction) {
            return Math.Pow(fraction, exponent);
        }

        public double EaseOut(double fraction) {
            return 1 - Math.Pow(1 - fraction, exponent);
        }

        public double EaseInOut(double fraction) {
            if (fraction < 0.5) {
                return 0.5 * EaseIn(2 * fraction);
            } else {
                return 0.5 + 0.5 * EaseOut((fraction - 0.5) * 2);
            }
        }
    }
}
