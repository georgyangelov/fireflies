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
using Fireflies.Library;
using Fireflies.Core;

namespace Fireflies.Choreographers.Screen {
    public class ScreenColor : IChoreographer {
        private ScreenCapturer screen;
        private ColorCorrectionFunction correction = ColorCorrectionFn.limitBrightness(0.6f);

        public ScreenColor(ScreenCapturer screenCapturer) {
            screen = screenCapturer;
        }

        public void Update(Color[] leds, int offset, int length, FrameInfo timing) {
            screen.NextFrame();

            byte[] frame = screen.CurrentFrame;
            int width = screen.ScreenWidth;
            int height = screen.ScreenHeight;
            
            for (int i = 0; i < length; i++) {
                leds[i + offset] = correction(ColorFn.crossfade(
                    leds[i + offset], 
                    getColorForPixel(i, length, frame, width, height), 
                    0.4f
                ));
            }
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

            for (int y = area.Top; y < area.Bottom; y += 3) {
                for (int x = area.Left; x < area.Right; x += 3) {
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

        private int wrap(int i, int length) {
            if (i >= 0) {
                return i % length;
            } else {
                return (i % length) + length;
            }
        }
    }
}
