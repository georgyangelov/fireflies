using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class SlidingColor : IOrchestrator {
        private Color color;
        private TimeSpan wrapAroundTime = new TimeSpan(0, 0, 0, 0, 2500);

        private EasingFunction easingFunction = (new Easing.Polynomial(1.4)).EaseInOut;
        // private EasingFunction easingFunction = Easing.Sine.EaseIn;
        private float trailLength = 40;
        private float forwardTrailLength = 5;

        private Color lightColor = Colors.Yellow;
        private Color darkColor = Colors.Red;

        public SlidingColor(Color color) {
            this.color = color;
        }

        public void Update(Color[] leds, TimeSpan totalTime, TimeSpan frameTime) {
            float progress = (float)easingFunction((totalTime.Ticks % wrapAroundTime.Ticks) / (double)wrapAroundTime.Ticks),
                  position = progress * leds.Length;
            
            for (int i = 0; i < leds.Length; i++) {
                float forwardIntensity = Math.Max(trailLength - forwardDistance(i, position, leds.Length), 0) / trailLength,
                      backwardIntensity = Math.Max(forwardTrailLength - forwardDistance(position, i, leds.Length), 0) / forwardTrailLength,
                      intensity = forwardIntensity + backwardIntensity;

                leds[i] = crossfade(darkColor, lightColor, intensity);
            }
        }
        
        private Color crossfade(Color a, Color b, float factor) {
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

        private float forwardDistance(float a, float b, int length) {
            if (a < b) {
                return b - a;
            } else {
                return b + length - a;
            }
        }
    }
}
