using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Frames;

namespace Fireflies.Orchestrators {
    class VectorChoreographer : IChoreographer {
        public delegate void Painter(Scene.Scene1D scene, FrameInfo frame);
        
        private Painter painter;

        public VectorChoreographer(Painter painter) {
            this.painter = painter;
        }

        void IChoreographer.Update(Color[] leds, int offset, int length, FrameInfo frame) {
            
        }
    }
}
