using Fireflies.Functions.Color;
using Fireflies.Orchestrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Fireflies.Core.Functional;

namespace Fireflies {
    class PredefinedOrchestrators {
        public static IOrchestrator buildDisabled() {
            return new VectorOrchestrator((scene, frame) => {
                scene.drawLine(0, 1, Colors.Black);
            });
        }

        public static IOrchestrator buildChangingColors() {
            Color[] colors = {
                Colors.Orange,
                Colors.LightGreen,
                Colors.LightBlue,
                Colors.Yellow,
                Colors.Magenta
            };

            ProgressFunction progressFn = Fn.linearProgress(TimeSpan.FromMilliseconds(60000));

            return new VectorOrchestrator((scene, frame) => {
                var progress = progressFn(frame);

                scene.drawLine(0, 1, Helpers.crossfade(colors, progress));
            });
        }

        public static IOrchestrator buildSimpleSlidingColor(bool reverse = false) {
            float lineLength = 0.2f;

            ProgressFunction velocityFn = Fn.alternating(Fn.linearProgress(TimeSpan.FromMilliseconds(10000)), 1, -1);
            ProgressFunction progressFn = Fn.looping(
                Fn.speedProgress(
                    (frame) => velocityFn(frame) * 1f,
                    TimeSpan.FromMilliseconds(2000)
                )
            );

            return new VectorOrchestrator((scene, frame) => {
                var velocity = velocityFn(frame);
                var progress = progressFn(frame);

                var crossfadeFactor = Math.Abs(velocity);

                scene.pushTransform(new Scene.Transform() {
                    offset = reverse ? -progress : progress,
                    scale = 1
                });

                scene.drawFeatheredLine(0, lineLength, Helpers.crossfade(Colors.Green, Colors.Red, crossfadeFactor), lineLength / 2, lineLength / 2);

                scene.popTransform();
            });
        }
    }
}
