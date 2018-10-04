using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Frames;
using Fireflies.Functions;
using Fireflies.Functions.Color;

namespace Fireflies.Orchestrators {
    public class PulsingColors : IOrchestrator {
        private ColorFunction color;
        private int segments;
        private float[] offsets;

        private TimingFunction timing = Utilities.withEasing(
            new Functions.Timing.Looping(TimeSpan.FromSeconds(10)).Alternating,
            Functions.Easing.Sine.EaseInOut
        );

        public PulsingColors(ColorFunction color, int segments) {
            this.color = color;
            this.segments = segments;
            this.offsets = new float[segments];

            var random = new Random();
            for (int i = 0; i < segments; i++) {
                offsets[i] = (float)random.NextDouble();
            }
        }

        void IOrchestrator.Update(Color[] leds, int offset, int length, FrameInfo f) {
            var pixelsPerSegment = length / segments;

            if (pixelsPerSegment == 0) {
                pixelsPerSegment = 1;
            }

            for (int i = 0; i < length; i++) {
                var segmentIndex = i / pixelsPerSegment;
                var colorOffset = offsets[segmentIndex % segments];

                var segmentFrameTime = new FrameInfo() {
                    frameTime = f.frameTime,
                    totalTime = f.totalTime.Add(TimeSpan.FromSeconds(5 * colorOffset))
                };
                // var progress = timing(segmentFrameTime);
                var progress = Utilities.stretch(timing(segmentFrameTime), 0, 0.7);

                leds[offset + i] = Helpers.darken(color(f), (float)progress);
            }
        }
    }
}
