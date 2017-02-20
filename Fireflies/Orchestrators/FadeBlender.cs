using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class FadeBlender : IOrchestrator {
        private Color[] blendColors;
        private IOrchestrator orchestrator;

        public FadeBlender(int ledCount, IOrchestrator orchestrator) {
            this.orchestrator = orchestrator;

            blendColors = new Color[ledCount];
            for (int i = 0; i < blendColors.Length; i++) {
                blendColors[i] = Colors.Black;
            }
        }

        private float blendFactorToBlack = 0.1f;
        private float blendFactorToWhite = 0.000001f;
        private TimeSpan blendTime = new TimeSpan(0, 0, 1);

        public void Update(Color[] leds, FrameInfo frame) {
            orchestrator.Update(blendColors, frame);

            double blendProgress = frame.frameTime.Ticks / (double)blendTime.Ticks;
            double blendFactorNowToBlack = Math.Pow(blendFactorToBlack, blendProgress);
            double blendFactorNowToWhite = Math.Pow(blendFactorToWhite, blendProgress);

            for (int i = 0; i < leds.Length; i++) {
                if (leds[i].R < blendColors[i].R) {
                    leds[i].R = (byte)(blendFactorNowToWhite * leds[i].R + (1 - blendFactorNowToWhite) * blendColors[i].R);
                } else {
                    leds[i].R = (byte)(blendFactorNowToBlack * leds[i].R + (1 - blendFactorNowToBlack) * blendColors[i].R);
                }

                if (leds[i].G < blendColors[i].G) {
                    leds[i].G = (byte)(blendFactorNowToWhite * leds[i].G + (1 - blendFactorNowToWhite) * blendColors[i].G);
                } else {
                    leds[i].G = (byte)(blendFactorNowToBlack * leds[i].G + (1 - blendFactorNowToBlack) * blendColors[i].G);
                }

                if (leds[i].B < blendColors[i].B) {
                    leds[i].B = (byte)(blendFactorNowToWhite * leds[i].B + (1 - blendFactorNowToWhite) * blendColors[i].B);
                } else {
                    leds[i].B = (byte)(blendFactorNowToBlack * leds[i].B + (1 - blendFactorNowToBlack) * blendColors[i].B);
                }
            }
        }
    }
}
