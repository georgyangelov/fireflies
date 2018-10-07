using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Scene {
    struct Matrix2D {
        public float a1, a2;
        public float b1, b2;

        public Matrix2D(float a1, float a2, float b1, float b2) {
            this.a1 = a1;
            this.a2 = a2;
            this.b1 = b1;
            this.b2 = b2;
        }

        public Matrix2D multiplyBy(Matrix2D y) {
            return new Matrix2D(
                a1 * y.a1 + a2 * y.b1, a1 * y.a2 + a2 * y.b2,
                b1 * y.a1 + b2 * y.b1, b1 * y.a2 + b2 * y.b2
            );
        }

        public Vector2D multiplyBy(Vector2D vector) {
            return new Vector2D(a1 * vector.x + a2 * vector.y, b1 * vector.x + b2 * vector.y);
        }
    }

    struct Vector2D {
        public float x, y;

        public Vector2D(float x, float y) {
            this.x = x;
            this.y = y;
        }
    }

    struct Transform {
        public float offset;
        public float scale;
        // TODO: Color transformation?

        public Matrix2D asMatrix() {
            return new Matrix2D(
                scale, offset,
                0,     1
            );
        }
    }

    struct Gradient {
        public Color from;
        public Color to;

        public Color colorAt(float factor) {
            if (factor < 0) {
                factor = 0;
            } else if (factor > 1) {
                factor = 1;
            }

            return new Color() {
                R = (byte)(from.R * (1 - factor) + to.R * factor),
                G = (byte)(from.G * (1 - factor) + to.G * factor),
                B = (byte)(from.B * (1 - factor) + to.B * factor),
                A = (byte)(from.A * (1 - factor) + to.A * factor)
            };
        }
    }

    class Scene1D {
        Stack<Matrix2D> transforms = new Stack<Matrix2D>();

        Color[] pixels;
        int pixelsOffset;
        int pixelsLength;

        public Scene1D(Color[] pixels, int pixelsOffset, int pixelsLength) {
            this.pixels = pixels;
            this.pixelsOffset = pixelsOffset;
            this.pixelsLength = pixelsLength;

            transforms.Push(new Matrix2D(
                1, 0,
                0, 1
            ));
        }

        public void pushTransform(Transform transform) {
            Matrix2D currentTransform = transforms.Peek();

            transforms.Push(currentTransform.multiplyBy(transform.asMatrix()));
        }

        public void popTransform() {
            transforms.Pop();
        }

        public void drawFeatheredLine(float from, float to, Color color, float startFeatherLength, float endFeatherLength) {
            drawFeatheredLine(from, to, new Gradient() { from = color, to = color }, startFeatherLength, endFeatherLength);
        }

        public void drawFeatheredLine(float from, float to, Gradient gradient, float startFeatherLength, float endFeatherLength) {
            drawLineWithLength(from, startFeatherLength, new Gradient() {
                from = Color.FromArgb(0, gradient.from.R, gradient.from.G, gradient.from.B),
                to = gradient.from
            });

            drawLineWithLength(from + startFeatherLength, to - from - startFeatherLength - endFeatherLength, gradient);
            
            drawLineWithLength(to - endFeatherLength, endFeatherLength, new Gradient() {
                from = gradient.to,
                to = Color.FromArgb(0, gradient.to.R, gradient.to.G, gradient.to.B)
            });
        }

        public void drawLine(float from, float to, Gradient gradient) {
            drawLineWithLength(from, to - from, gradient);
        }

        public void drawLine(float from, float to, Color color) {
            drawLineWithLength(from, to - from, new Gradient() { from = color, to = color });
        }

        public void drawLineWithLength(float x, float length, Color color) {
            drawLineWithLength(x, length, new Gradient() { from = color, to = color });
        }

        public void drawLineWithLength(float x, float length, Gradient gradient) {
            if (isZero(length)) {
                return;
            }

            Matrix2D currentTransform = transforms.Peek();

            float x1 = currentTransform.multiplyBy(new Vector2D(x, 1)).x;
            float x2 = currentTransform.multiplyBy(new Vector2D(x + length, 1)).x;

            float x1Pixel = x1 * pixelsLength;
            float x2Pixel = x2 * pixelsLength;

            float lengthOnScreen = x2Pixel - x1Pixel;

            if (lengthOnScreen < 0.00000001) {
                return;
            }

            int fromPixel = (int)x1Pixel;
            int toPixel = (int)Math.Ceiling(x2Pixel);

            float distance = x1Pixel - fromPixel;
            setPixel(fromPixel, new Color() {
                R = gradient.from.R,
                G = gradient.from.G,
                B = gradient.from.B,
                A = (byte)(gradient.from.A * Math.Min(1, Math.Max(0, 1 - distance)))
            });

            for (int i = fromPixel + 1; i < toPixel; i++) {
                float progress = (i - x1Pixel) / lengthOnScreen;

                setPixel(i, gradient.colorAt(progress));
            }

            distance = toPixel - x2Pixel;
            setPixel(toPixel, new Color() {
                R = gradient.to.R,
                G = gradient.to.G,
                B = gradient.to.B,
                A = (byte)(gradient.to.A * Math.Min(1, Math.Max(0, 1 - distance)))
            });
        }

        private int pixelInScene(int index) {
            int result = index % pixelsLength;

            if (result < 0) {
                result = result + pixelsLength;
            }

            return result + pixelsOffset;
        }

        private void setPixel(int sceneIndex, Color color) {
            int arrayIndex = pixelInScene(sceneIndex);

            pixels[arrayIndex] = blend(pixels[arrayIndex], color, (float)color.A / 255);
        }

        private static Color blend(Color a, Color b, float factor) {
            if (factor < 0) {
                factor = 0;
            } else if (factor > 1) {
                factor = 1;
            }

            return new Color() {
                R = (byte)(a.R * (1 - factor) + b.R * factor),
                G = (byte)(a.G * (1 - factor) + b.G * factor),
                B = (byte)(a.B * (1 - factor) + b.B * factor),
                A = 255
            };
        }

        private static bool isZero(float x) {
            return Math.Abs(x) < 0.000001;
        }
    }
}