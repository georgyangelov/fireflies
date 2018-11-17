using Fireflies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Frames {
    public class RenderingTargetSource : IFrameSource {
        public event FrameUpdate FrameRequest;

        private TimeSpan elapsedTime = new TimeSpan(0);

        public void Start() {
            CompositionTarget.Rendering += Update;
        }

        public void Stop() {
            CompositionTarget.Rendering -= Update;
        }

        private void Update(object sender, EventArgs e) {
            var renderEvent = (RenderingEventArgs)e;
            var frameTime = renderEvent.RenderingTime - elapsedTime;

            // CompositionTarget.Rendering is sometimes called multiple times per frame due to layouting changes
            if (frameTime.Ticks <= 0) {
                return;
            }

            elapsedTime = renderEvent.RenderingTime;

            FrameRequest?.Invoke();
        }
    }
}
