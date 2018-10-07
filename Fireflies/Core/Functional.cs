using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Frames;

namespace Fireflies.Core {
    class Functional : IOrchestrator {
        private ProgressFunction progressFunction;
        private OrchestratorFunction orchestratorFunction;

        public delegate float ProgressFunction(FrameInfo frame);
        public delegate Color[] OrchestratorFunction(double progress, int pixel, int totalPixels);

        public Functional(ProgressFunction progressFunction, OrchestratorFunction orchestratorFunction) {
            this.progressFunction = progressFunction;
            this.orchestratorFunction = orchestratorFunction;
        }

        void IOrchestrator.Update(Color[] leds, int offset, int length, FrameInfo frame) {
            //int pixelCount = length - offset;

            //for (int i = offset; i < length; i++) {
            //    int pixelIndex = i - offset;

            //    leds[i] = orchestratorFunction(progressFunction(frame), pixelIndex, pixelCount);
            //}
        }
    }
}
