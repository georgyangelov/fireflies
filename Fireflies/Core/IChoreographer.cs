using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies {
    public interface IChoreographer {
        void Update(Color[] leds, int offset, int length, FrameInfo frame);
    }
}
