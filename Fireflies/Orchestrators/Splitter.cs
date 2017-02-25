using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    public class Splitter : IOrchestrator {
        private int[] segmentLengths;
        private IOrchestrator[] orchestrators;

        public Splitter(int[] segmentLengths, IOrchestrator[] orchestrators) {
            this.segmentLengths = segmentLengths;
            this.orchestrators = orchestrators;
        }

        public void Update(Color[] leds, int offset, int length, FrameInfo frame) {
            int subOffset = offset;

            for (int i = 0; i < segmentLengths.Length; i++) {
                int subLength = segmentLengths[i];

                orchestrators[i].Update(leds, subOffset, subLength, frame);

                subOffset += subLength;
            }
        }
    }
}
