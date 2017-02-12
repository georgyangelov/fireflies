using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class StaticColor : IOrchestrator {
        public void Update(TimeSpan frameTime, Color[] leds) {
            for (int i = 0; i < leds.Length; i++) {
                leds[i].R = 0;
                leds[i].G = 0;
                leds[i].B = 255;
            }
        }
    }
}
