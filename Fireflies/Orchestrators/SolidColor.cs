using Fireflies.Frames;
using System;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class SolidColor : IOrchestrator {
        private ColorFunction color;

        public SolidColor(ColorFunction color) {
            this.color = color;
        }

        public void Update(Color[] leds, int offset, int length, FrameInfo frame) {
            var c = color(frame);

            for (int i = 0; i < length; i++) {
                leds[i + offset] = c;
            }
        }
    }
}
