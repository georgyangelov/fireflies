using Fireflies.Core;
using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Orchestrators {
    class Fn {
        public static Functional.ProgressFunction linearProgress(TimeSpan speed) {
            return (frame) => {
                return frame.totalTime.Ticks / (float)speed.Ticks;
            };
        }

        public static Functional.ProgressFunction loopingProgress(TimeSpan loopTime) {
            return looping(linearProgress(loopTime));
        }

        public delegate float SpeedAdjustmentFunction(FrameInfo frame);

        public static Functional.ProgressFunction speedProgress(SpeedAdjustmentFunction speedFunction, TimeSpan unitMeasure, float speedSmoothing = 0f) {
            float progress = 0;
            float speed = 0;

            return (frame) => {
                speed = speed * speedSmoothing + speedFunction(frame) * (1 - speedSmoothing);
                progress += (float)frame.frameTime.Ticks * speed / unitMeasure.Ticks;

                return progress;
            };
        }

        public static SpeedAdjustmentFunction cpuUsageSpeedup(float factor) {
            var totalCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            return (frame) => {
                float usage = totalCpuCounter.NextValue();

                Console.WriteLine("Usage: " + usage + "%");

                return usage / 100 * factor;
            };
        }

        public static Functional.ProgressFunction looping(Functional.ProgressFunction progressFunction) {
            return (frame) => {
                return progressFunction(frame) % 1f;
            };
        }

        public static Functional.ProgressFunction alternating(Functional.ProgressFunction progressFunction, float from, float to) {
            return (frame) => {
                return (1 - Math.Abs((progressFunction(frame) % 2f) - 1f)) * (to - from) + from;
            };
        }

        //public static Core.Functional.OrchestratorFunction solidColor(Color color) {
        //    return (progress, pixel, totalPixels) => {
        //        return color;
        //    };
        //}

        //public static Core.Functional.OrchestratorFunction sliding(Core.Functional.OrchestratorFunction orchestratorFn) {
        //    return (Scene scene) => {
        //        Scene sourceScene = scene.clone();
        //        Orchestrator orchestrator = orchestratorFn(sourceScene);

        //        return (Frame frame) => {
        //            float pixelOffset = frame.progress * frame.pixelCount;

        //            orchestrator(frame);

        //            for (int i = 0; i < scene.pixelCount; i++) {
        //                int pixelAIndex = (i - offset) % sourceScene

        //                Color pixelColor = crossfade(sourceScene.pixels[pixelAIndex], sourceScene.pixels[pixelBIndex], pixelProgress);
        //            }

        //            return frame;
        //        };
        //    };
        //}

        //private static Color crossfade(Color a, Color b, double factor) {
        //    if (factor < 0) {
        //        factor = 0;
        //    } else if (factor > 1) {
        //        factor = 1;
        //    }

        //    return new Color() {
        //        R = (byte)(a.R * (1 - factor) + b.R * factor),
        //        G = (byte)(a.G * (1 - factor) + b.G * factor),
        //        B = (byte)(a.B * (1 - factor) + b.B * factor),
        //        A = 255
        //    };
        //}
    }
}
