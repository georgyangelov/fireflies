using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Frames;

namespace Fireflies.Orchestrators {
    public class AlignTest : IChoreographer {
        void IChoreographer.Update(Color[] leds, int offset, int length, FrameInfo frame) {
            leds[offset] = new Color {
                A = 255,
                R = 255,
                G = 0,
                B = 0
            };
        }
    }
}
