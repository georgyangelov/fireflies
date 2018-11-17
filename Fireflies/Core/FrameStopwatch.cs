using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Core {
    public struct FrameInfo {
        public TimeSpan frameTime;
        public TimeSpan totalTime;
    }

    public class FrameStopwatch {
        private TimeSpan lastFrameTime;
        private Stopwatch stopwatch = new Stopwatch();

        public FrameStopwatch() {
            Reset();
        }

        public void Reset() {
            lastFrameTime = new TimeSpan(0);
            stopwatch.Restart();
        }

        public FrameInfo NewFrame() {
            var totalTime = stopwatch.Elapsed;
            var frameTime = totalTime - lastFrameTime;

            lastFrameTime = totalTime;

            return new FrameInfo() {
                totalTime = totalTime,
                frameTime = frameTime
            };
        }
    }
}
