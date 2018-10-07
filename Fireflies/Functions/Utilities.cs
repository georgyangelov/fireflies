using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Functions {
    public static class Utilities {
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

        public static TimingFunction withEasing(TimingFunction timing, EasingFunction easing) {
            return (FrameInfo f) => easing(timing(f));
        }

        public static double stretch(double value, double offset, double lengthFactor) {
            return value * lengthFactor + offset;
        }

        public static float stretch(float value, float fromMin, float fromMax, float toMin, float toMax) {
            float fraction = (value - fromMin) / (fromMax - fromMin);

            return fraction * (toMax - toMin) + toMin;
        }
    }
}
