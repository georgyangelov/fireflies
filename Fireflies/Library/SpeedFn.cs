using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Library {
    static class SpeedFn {
        public static SpeedAdjustmentFunction cpuUsage(float factor) {
            var totalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            return (frame) => {
                float usage = totalCpuCounter.NextValue();

                return usage / 100 * factor;
            };
        }
    }
}
