using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Functions.Timing {
    class Speed {
        public delegate double SpeedFunction(FrameInfo frame);

        private SpeedFunction speedFn;
        private double position = 0;

        public Speed(SpeedFunction speedFn) {
            this.speedFn = speedFn;
        }

        public double Function(FrameInfo frame) {
            position = (position + speedFn(frame) * frame.frameTime.TotalSeconds) % 1;

            return position;
        }
    }
}
