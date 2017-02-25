using Fireflies.Frames;
using System;

namespace Fireflies.Functions.Timing {
    class Looping {
        private TimeSpan loopTime;

        public Looping(TimeSpan loopTime) {
            this.loopTime = loopTime;
        }

        public double Loop(FrameInfo frame) {
            return (frame.totalTime.Ticks % loopTime.Ticks) / (double)loopTime.Ticks;
        }

        public double Alternating(FrameInfo frame) {
            return 1 - Math.Abs((Loop(frame) - 0.5) * 2);
        }
    }
}
