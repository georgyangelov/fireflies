using Fireflies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fireflies.Frames {
    public class FramerateLimiterSource : IFrameSource {
        private IFrameSource source;
        private FrameStopwatch stopwatch = new FrameStopwatch();

        public event FrameUpdate FrameRequest;

        public int Limit { get; set; }
        public int MinFrameTimeMillis {
            get => 1000 / Limit;
        }

        public FramerateLimiterSource(IFrameSource source, int limit) {
            this.source = source;
            Limit = limit;
        }

        public void Start() {
            source.FrameRequest += Source_FrameRequest;

            stopwatch.Reset();
            source.Start();
        }

        public void Stop() {
            source.FrameRequest -= Source_FrameRequest;

            source.Stop();
        }

        private void Source_FrameRequest() {
            FrameInfo frame = stopwatch.NewFrame();

            // TODO: Give some time for the actual render logic
            int waitTime = MinFrameTimeMillis - (int)frame.frameTime.TotalMilliseconds - 1;

            if (waitTime > 0) {
                // Yes, Thread.Sleep is not precise. However, it is good enough for our intended use
                Thread.Sleep(waitTime);
                stopwatch.NewFrame();
            }

            FrameRequest?.Invoke();
        }
    }
}
