using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    public class Splitter : IChoreographer {
        private int[] segmentLengths;
        private IChoreographer[] orchestrators;

        public Splitter(int[] segmentLengths, IChoreographer[] orchestrators) {
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
