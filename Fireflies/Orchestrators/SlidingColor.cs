using Fireflies.Frames;
using System;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class SlidingColor : IOrchestrator {
        private TimeSpan wrapAroundTime = new TimeSpan(0, 0, 0, 0, 2500);

        private TimingFunction timing;
        private ColorFunction background, foreground;

        private float trailLength = 4;
        private float forwardTrailLength = 4;

        public SlidingColor(TimingFunction timing, ColorFunction background, ColorFunction foreground) {
            this.timing = timing;
            this.background = background;
            this.foreground = foreground;
        }

        public void Update(Color[] leds, int offset, int length, FrameInfo frame) {
            double progress = timing(frame),
                   position = progress * length;
            
            for (int i = 0; i < length; i++) {
                double forwardIntensity = Math.Max(trailLength - forwardDistance(i, position, length), 0) / trailLength,
                       backwardIntensity = Math.Max(forwardTrailLength - forwardDistance(position, i, length), 0) / forwardTrailLength,
                       intensity = forwardIntensity + backwardIntensity;
            
                leds[i + offset] = crossfade(background(frame), foreground(frame), intensity);
            }
        }
        
        private Color crossfade(Color a, Color b, double factor) {
            if (factor < 0) {
                factor = 0;
            } else if (factor > 1) {
                factor = 1;
            }

            return new Color() {
                R = (byte)(a.R * (1 - factor) + b.R * factor),
                G = (byte)(a.G * (1 - factor) + b.G * factor),
                B = (byte)(a.B * (1 - factor) + b.B * factor),
                A = 255
            };
        }

        private double forwardDistance(double a, double b, int length) {
            if (a < b) {
                return b - a;
            } else {
                return b + length - a;
            }
        }
    }
}
