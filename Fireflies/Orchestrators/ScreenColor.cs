using Fireflies.Capture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fireflies.Frames;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Fireflies.Orchestrators {
    public class ScreenColor : IOrchestrator {
        private ScreenCapturer screen;

        public ScreenColor(ScreenCapturer screenCapturer) {
            screen = screenCapturer;
        }

        void IOrchestrator.Update(Color[] leds, int offset, int length, FrameInfo timing) {
            System.Drawing.Bitmap frame = screen.CurrentFrame;
            int width = frame.Width;
            int height = frame.Height;
            byte[] pixels = bitmapToArray(frame);
            
            for (int i = 0; i < length; i++) {
                leds[i + offset] = crossfade(leds[i + offset], getColorForPixel(i, length, pixels, width, height), 0.5);
            }
        }

        // TODO: Extract from here and from SlidingColor
        private Color crossfade(Color a, Color b, double factor) {
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

        private Color getColorForPixel(int i, int pixelCount, byte[] screenFrame, int screenWidth, int screenHeight) {
            return averageColor(screenFrame, getRectangleForPixel(i, 5, pixelCount, screenWidth, screenHeight), screenWidth, screenHeight);
        }

        private System.Drawing.Rectangle getRectangleForPixel(int i, int offset, int pixelCount, int screenWidth, int screenHeight) {
            System.Windows.Point positionPoint;
            System.Windows.Point tangentVector;

            var geometry = new RectangleGeometry(
                new System.Windows.Rect(0, 0, screenWidth, screenHeight)
            ).GetFlattenedPathGeometry();

            var pixelpos = wrap(i - offset, pixelCount);
            var progress = pixelpos / (double)pixelCount;

            geometry.GetPointAtFractionLength(progress, out positionPoint, out tangentVector);

            int w = screenWidth / 4,
                h = screenHeight / 3;

            var r = new System.Drawing.Rectangle(
                new System.Drawing.Point((int)positionPoint.X - w / 2, (int)positionPoint.Y - h / 2), 
                new System.Drawing.Size(w, h)
            );

            r.Intersect(new System.Drawing.Rectangle(0, 0, screenWidth, screenHeight));

            return r;
        }

        private Color averageColor(byte[] pixels, System.Drawing.Rectangle area, int screenWidth, int screenHeight) {
            int pixelOffset;
            int width = area.Width;
            int height = area.Height;

            byte r, g, b;
            int rSum = 0, gSum = 0, bSum = 0, pixelCount = 0;

            for (int y = area.Top; y < area.Bottom; y++) {
                for (int x = area.Left; x < area.Right; x++) {
                    pixelOffset = y * screenWidth * 4 + x * 4;
                    r = pixels[pixelOffset + 2];
                    g = pixels[pixelOffset + 1];
                    b = pixels[pixelOffset + 0];

                    rSum += r;
                    gSum += g;
                    bSum += b;
                    pixelCount++;
                }
            }

            if (pixelCount == 0) {
                return Colors.Black;
            }

            return new Color {
                A = 255,
                R = (byte)(rSum / pixelCount),
                G = (byte)(gSum / pixelCount),
                B = (byte)(bSum / pixelCount)
            };
        }

        private byte[] bitmapToArray(System.Drawing.Bitmap frame) {
            byte[] pixelData = new byte[frame.Height * frame.Width * 4];

            BitmapData bitmap = frame.LockBits(
                new System.Drawing.Rectangle(0, 0, frame.Width, frame.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            int r = 0, g = 0, b = 0;
            IntPtr ptr = bitmap.Scan0;

            for (int y = 0; y < bitmap.Height; y++) {
                Marshal.Copy(ptr, pixelData, y * frame.Width * 4, frame.Width * 4);
                ptr = IntPtr.Add(ptr, bitmap.Stride);
            }

            frame.UnlockBits(bitmap);

            return pixelData;
        }

        private int wrap(int i, int length) {
            if (i >= 0) {
                return i % length;
            } else {
                return (i % length) + length;
            }
        }
    }
}
