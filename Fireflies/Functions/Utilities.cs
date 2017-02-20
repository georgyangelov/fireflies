﻿using System;
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
    }
}