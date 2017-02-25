using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Frames {
    public class FPSCounter {
        public double CurrentFPS { get; private set; }

        public void frameReady(FrameInfo frame) {
            CurrentFPS = CurrentFPS * 0.95 + (1 / frame.frameTime.TotalSeconds) * 0.05;
        }
    }
}
