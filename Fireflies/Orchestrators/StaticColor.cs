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
                leds[i] = Color.FromRgb(0, 0, 255);
            }
        }
    }
}
