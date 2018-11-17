using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class RandomColor : IChoreographer {
        private Random random = new Random();

        public void Update(Color[] leds, int offset, int length, FrameInfo frame) {
            for (int i = 0; i < length; i++) {
                leds[i].R = (byte)random.Next();
                leds[i].G = (byte)random.Next();
                leds[i].B = (byte)random.Next();
            }
        }
    }
}
