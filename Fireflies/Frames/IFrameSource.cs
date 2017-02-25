using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Frames {
    

    public delegate void FrameUpdate();

    public interface IFrameSource {
        event FrameUpdate FrameRequest;

        void Start();
        void Stop();
    }
}
