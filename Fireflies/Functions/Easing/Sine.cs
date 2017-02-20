using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Functions.Easing {
    class Sine {
        static public double EaseIn(double x) {
            return 1 - Math.Cos(x * Math.PI / 2);
        }

        static public double EaseOut(double x) {
            return Math.Sin(x * Math.PI / 2);
        }

        static public double EaseInOut(double x) {
            return (1 - Math.Cos(x * Math.PI)) / 2;
        }
    }
}
