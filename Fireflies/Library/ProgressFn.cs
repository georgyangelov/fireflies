using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fireflies.Library {
    static class ProgressFn {
        public static ProgressFunction linear(TimeSpan speed) {
            return (frame) => {
                return frame.totalTime.Ticks / (float)speed.Ticks;
            };
        }

        public static ProgressFunction looping(ProgressFunction progressFunction, EasingFunction easingFn) {
            return (frame) => {
                return easingFn(progressFunction(frame) % 1f);
            };
        }

        public static ProgressFunction looping(ProgressFunction progressFunction) {
            return looping(progressFunction, EasingFn.linear());
        }

        public static ProgressFunction looping(TimeSpan loopTime) {
            return looping(linear(loopTime), EasingFn.linear());
        }

        public static ProgressFunction looping(TimeSpan loopTime, EasingFunction easingFn) {
            return looping(linear(loopTime), easingFn);
        }

        public static ProgressFunction alternating(ProgressFunction progressFunction, float from, float to, EasingFunction easingFn) {
            return (frame) => {
                return easingFn(1 - Math.Abs((progressFunction(frame) % 2f) - 1f)) * (to - from) + from;
            };
        }

        public static ProgressFunction alternating(ProgressFunction progressFunction, float from, float to) {
            return alternating(progressFunction, from, to, EasingFn.linear());
        }

        public static ProgressFunction bySpeed(SpeedAdjustmentFunction speedFunction, TimeSpan unitMeasure, float speedSmoothing = 0.01f) {
            double progress = 0;
            double speed = 0;

            return (frame) => {
                speed = speed * speedSmoothing + speedFunction(frame) * (1 - speedSmoothing);
                progress += frame.frameTime.Ticks * speed / unitMeasure.Ticks;

                return (float)progress;
            };
        }
    }
}
