using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Library {
    static class NumberFn {
        public static double wrap(double value, double min, double max) {
            double valueZero = value - min,
                   maxZero = max - min;

            if (valueZero < 0) {
                return valueZero % maxZero + maxZero + min;
            }

            return valueZero % maxZero + min;
        }

        public static double wrapExtend(double value, double min, double max, double extendWith) {
            return wrap(value * (1 + extendWith) - extendWith / 2, min, max);
        }

        public static double stretch(double value, double offset, double lengthFactor) {
            return value * lengthFactor + offset;
        }

        public static float stretch(float value, float fromMin, float fromMax, float toMin, float toMax) {
            float fraction = (value - fromMin) / (fromMax - fromMin);

            return fraction * (toMax - toMin) + toMin;
        }

        public static byte clampToByte(int value) {
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
