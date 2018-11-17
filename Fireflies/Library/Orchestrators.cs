using Fireflies.Orchestrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Library {
    static class Orchestrators {
        public static IChoreographer black() {
            return new VectorChoreographer((scene, frame) => {
                scene.drawLine(0, 1, Colors.Black);
            });
        }

        public static IChoreographer changingColors() {
            Color[] colors = {
                Colors.Orange,
                Colors.LightGreen,
                Colors.LightBlue,
                Colors.Yellow,
                Colors.Magenta
            };

            ProgressFunction progressFn = ProgressFn.linear(TimeSpan.FromMilliseconds(60000));

            return new VectorChoreographer((scene, frame) => {
                var progress = progressFn(frame);

                scene.drawLine(0, 1, ColorFn.crossfade(colors, progress));
            });
        }

        public static IChoreographer simpleSlidingColor(Color color, ProgressFunction progressFn, float length = 0.3f) {
            return new VectorChoreographer((scene, frame) => {
                var progress = progressFn(frame);

                scene.pushTransform(new Scene.Transform() { offset = progress, scale = 1 });
                scene.drawFeatheredLine(0, length, color, length / 3, length / 3);
                scene.popTransform();
            });
        }

        public static IChoreographer slidingColorWithVelocity(bool reverse = false) {
            float lineLength = 0.2f;

            ProgressFunction velocityFn = ProgressFn.alternating(ProgressFn.linear(TimeSpan.FromMilliseconds(10000)), 1, -1);
            ProgressFunction progressFn = ProgressFn.looping(
                ProgressFn.bySpeed(
                    (frame) => velocityFn(frame) * 1f,
                    TimeSpan.FromMilliseconds(2000)
                )
            );

            return new VectorChoreographer((scene, frame) => {
                var velocity = velocityFn(frame);
                var progress = progressFn(frame);

                var crossfadeFactor = Math.Abs(velocity);

                scene.pushTransform(new Scene.Transform() {
                    offset = reverse ? -progress : progress,
                    scale = 1
                });

                scene.drawFeatheredLine(0, lineLength, ColorFn.crossfade(Colors.Green, Colors.Red, crossfadeFactor), lineLength / 2, lineLength / 2);

                scene.popTransform();
            });
        }

        public static IChoreographer rainbow() {
            var progressFn = ProgressFn.linear(TimeSpan.FromMilliseconds(10000));

            Color[] colors = { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Cyan, Colors.Blue, Colors.Violet };
            var lineLength = 1f / colors.Length;

            return new VectorChoreographer((scene, frame) => {
                var progress = progressFn(frame);

                scene.pushTransform(new Scene.Transform() {
                    offset = progress,
                    scale = 1
                });

                for (int i = 0; i < colors.Length - 1; i++) {
                    scene.drawLineWithLength(i * lineLength, lineLength, new Scene.Gradient { from = colors[i], to = colors[i + 1] });
                }

                scene.drawLineWithLength(1 - lineLength, lineLength, new Scene.Gradient { from = colors[colors.Length - 1], to = colors[0] });

                scene.popTransform();
            });
        }
    }
}
