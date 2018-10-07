using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Frames;

namespace Fireflies.Orchestrators {
    class VectorOrchestrator : IOrchestrator {
        public delegate void Painter(Scene.Scene1D scene, float progress);

        private Core.Functional.ProgressFunction progressFn;
        private Painter painter;

        public VectorOrchestrator(Core.Functional.ProgressFunction progressFn, Painter painter) {
            this.progressFn = progressFn;
            this.painter = painter;
        }

        void IOrchestrator.Update(Color[] leds, int offset, int length, FrameInfo frame) {
            for (int i = offset; i < offset + length; i++) {
                leds[i] = Colors.Black;
            }

            Scene.Scene1D scene = new Scene.Scene1D(leds, offset, length);

            painter(scene, progressFn(frame));
        }
    }
}
