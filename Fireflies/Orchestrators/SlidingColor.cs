using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class SlidingColor : IOrchestrator {
        private Color color;
        private TimeSpan wrapAroundTime = new TimeSpan(0, 0, 5);

        private float fadeFactor = 0.3f;
        private TimeSpan fadeTime = new TimeSpan(0, 0, 2);

        private Color trailColor = Colors.Aquamarine;

        public SlidingColor(Color color) {
            this.color = color;
        }

        public void Update(Color[] leds, TimeSpan totalTime, TimeSpan frameTime) {
            double progress = (totalTime.Ticks % wrapAroundTime.Ticks) / (double)wrapAroundTime.Ticks;
            int currentLed = (int)(progress * leds.Length);

            double fadeProgress = frameTime.Ticks / (double)fadeTime.Ticks;
            double fadeFactorNow = Math.Pow(fadeFactor, fadeProgress);

            for (int i = 0; i < leds.Length; i++) {
                leds[i].R = (byte)(leds[i].R * fadeFactorNow);
                leds[i].G = (byte)(leds[i].G * fadeFactorNow);
                leds[i].B = (byte)(leds[i].B * fadeFactorNow);
            }

            leds[wraparound(currentLed - 1, leds.Length)] = trailColor;
            leds[currentLed] = color;
        }

        private int wraparound(int index, int max) {
            if (index >= 0) {
                return index % max;
            } else {
                return index % max + max;
            }
        }
    }
}
