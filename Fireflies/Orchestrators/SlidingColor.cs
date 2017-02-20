using System;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class SlidingColor : IOrchestrator {
        private TimeSpan wrapAroundTime = new TimeSpan(0, 0, 0, 0, 2500);

        private TimingFunction timing;
        private ColorFunction background, foreground;

        private float trailLength = 10;
        private float forwardTrailLength = 10;

        public SlidingColor(TimingFunction timing, ColorFunction background, ColorFunction foreground) {
            this.timing = timing;
            this.background = background;
            this.foreground = foreground;
        }

        public void Update(Color[] leds, FrameInfo frame) {
            double progress = timing(frame),
                   position = progress * leds.Length;
            
            for (int i = 0; i < leds.Length; i++) {
                double forwardIntensity = Math.Max(trailLength - forwardDistance(i, position, leds.Length), 0) / trailLength,
                       backwardIntensity = Math.Max(forwardTrailLength - forwardDistance(position, i, leds.Length), 0) / forwardTrailLength,
                       intensity = forwardIntensity + backwardIntensity;
            
                leds[i] = crossfade(background(frame), foreground(frame), intensity);
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
