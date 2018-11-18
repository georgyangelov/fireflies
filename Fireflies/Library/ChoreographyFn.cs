﻿using Fireflies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Library {
    static class ChoreographyFn {
        public delegate void Scene1DPainter(Scene.Scene1D scene, FrameInfo frame);

        public static ChoreographyFunction scene1D(Scene1DPainter painter) {
            return (pixels, offset, length, frame) => {
                for (int i = offset; i < offset + length; i++) {
                    pixels[i] = Colors.Black;
                }

                Scene.Scene1D scene = new Scene.Scene1D(pixels, offset, length);

                painter(scene, frame);
            };
        }

        public static ChoreographyFunction segment(int[] segments, ChoreographyFunction[] choreographers) {
            return (pixels, offset, length, frame) => {
                int subOffset = offset;

                for (int i = 0; i < segments.Length; i++) {
                    int subLength = segments[i];

                    choreographers[i](pixels, subOffset, subLength, frame);

                    subOffset += subLength;
                }
            };
        }

        public static ChoreographyFunction correction(ChoreographyFunction choreographyFn, ColorCorrectionFunction correctionFn) {
            return correction(choreographyFn, (frame) => correctionFn);
        }

        public static ChoreographyFunction correction(ChoreographyFunction choreographyFn, DynamicValue<ColorCorrectionFunction> correctionFn) {
            return (pixels, offset, length, frame) => {
                choreographyFn(pixels, offset, length, frame);

                var correction = correctionFn(frame);

                for (int i = 0; i < length; i++) {
                    pixels[offset + i] = correction(pixels[offset + i]);
                }
            };
        }

        public static ChoreographyFunction dynamic(DynamicValue<ChoreographyFunction> choreographySupplier) {
            return (pixels, offset, length, frame) => choreographySupplier(frame)(pixels, offset, length, frame);
        }

        // --------------------------------------------

        public static ChoreographyFunction randomColor() {
            Random random = new Random();

            return (pixels, offset, length, frame) => {
                for (int i = 0; i < length; i++) {
                    pixels[offset + i].R = (byte)random.Next();
                    pixels[offset + i].G = (byte)random.Next();
                    pixels[offset + i].B = (byte)random.Next();
                }
            };
        }

        public static ChoreographyFunction alignmentTest() {
            return (pixels, offset, length, frame) => {
                for (int i = 0; i < length; i++) {
                    pixels[offset + i] = new Color { A = 255, R = 0, G = 255, B = 0 };
                }

                pixels[offset] = new Color { A = 255, R = 255, G = 0, B = 0 };
                pixels[offset + length - 1] = new Color { A = 255, R = 0, G = 0, B = 255 };
            };
        }

        public static ChoreographyFunction black() {
            return scene1D((scene, frame) => {
                scene.drawLine(0, 1, Colors.Black);
            });
        }

        public static ChoreographyFunction changingColors() {
            Color[] colors = {
                Colors.Orange,
                Colors.LightGreen,
                Colors.LightBlue,
                Colors.Yellow,
                Colors.Magenta
            };

            ProgressFunction progressFn = ProgressFn.linear(TimeSpan.FromMilliseconds(60000));

            return scene1D((scene, frame) => {
                var progress = progressFn(frame);

                scene.drawLine(0, 1, ColorFn.crossfade(colors, progress));
            });
        }

        public static ChoreographyFunction simpleSlidingColor(Color color, ProgressFunction progressFn, float length = 0.3f) {
            return scene1D((scene, frame) => {
                var progress = progressFn(frame);

                scene.pushTransform(new Scene.Transform() { offset = progress, scale = 1 });
                scene.drawFeatheredLine(0, length, color, length / 3, length / 3);
                scene.popTransform();
            });
        }

        public static ChoreographyFunction slidingColorWithVelocity(bool reverse = false) {
            float lineLength = 0.2f;

            ProgressFunction velocityFn = ProgressFn.alternating(ProgressFn.linear(TimeSpan.FromMilliseconds(10000)), 1, -1);
            ProgressFunction progressFn = ProgressFn.looping(
                ProgressFn.bySpeed(
                    (frame) => velocityFn(frame) * 1f,
                    TimeSpan.FromMilliseconds(2000)
                )
            );

            return scene1D((scene, frame) => {
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

        public static ChoreographyFunction rainbow() {
            var progressFn = ProgressFn.linear(TimeSpan.FromMilliseconds(10000));

            Color[] colors = { Colors.Red, Colors.Orange, Colors.Yellow, Colors.Green, Colors.Cyan, Colors.Blue, Colors.Violet };
            var lineLength = 1f / colors.Length;

            return scene1D((scene, frame) => {
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
