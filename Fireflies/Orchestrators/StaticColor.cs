using System;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class StaticColor : IOrchestrator {
        public void Update(Color[] leds, FrameInfo frame) {
            for (int i = 0; i < leds.Length; i++) {
                leds[i].R = 0;
                leds[i].G = 0;
                leds[i].B = 255;
            }
        }
    }
}
