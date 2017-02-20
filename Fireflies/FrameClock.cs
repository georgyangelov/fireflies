using System;
using System.Windows.Media;

namespace Fireflies {
    public struct FrameInfo {
        public TimeSpan frameTime;
        public TimeSpan totalTime;
    }

    // TODO: Fix the frame limiting logic
    public class FrameClock {
        private int fpsLimit;
        private double currentFps = 0;

        private TimeSpan elapsedTime = new TimeSpan(0);

        public delegate void FrameUpdate(FrameInfo frame);
        public event FrameUpdate OnFrame;

        public FrameClock(int fpsLimit = 0) {
            this.fpsLimit = fpsLimit;
        }

        public double CurrentFPS {
            get => currentFps;
        }

        public void Start() {
            CompositionTarget.Rendering += Update;
        }

        public void Stop() {
            CompositionTarget.Rendering -= Update;
        }

        private void Update(object sender, EventArgs e) {
            var renderEvent = (RenderingEventArgs)e;
            var frameTime = renderEvent.RenderingTime - elapsedTime;

            if (frameTime.Ticks <= 0) {
                return;
            }

            if (fpsLimit != 0 && frameTime.TotalSeconds < 1 / (double)fpsLimit) {
                return;
            }

            currentFps = currentFps * 0.95 + (1 / frameTime.TotalSeconds) * 0.05;
            elapsedTime = renderEvent.RenderingTime;

            var frame = new FrameInfo() {
                totalTime = elapsedTime,
                frameTime = frameTime
            };

            OnFrame?.Invoke(frame);
        }
    }
}
