using Fireflies.Frames;
using System;
using System.Windows.Media;

namespace Fireflies.Core {
    public delegate void ChoreographyFunction(Color[] leds, int offset, int length, FrameInfo frame);

    // FrameInfo -> [0, +inf)
    public delegate float ProgressFunction(FrameInfo frame);

    // FrameInfo -> (-inf, +inf)
    public delegate float SpeedAdjustmentFunction(FrameInfo frame);

    // [0, 1] -> [0, 1]
    public delegate float EasingFunction(float fraction);

    // FrameInfo -> any Color
    public delegate Color ColorFunction(FrameInfo frame);

    // Color -> Color
    public delegate Color ColorCorrectionFunction(Color color);

    // FrameInfo -> T
    public delegate T DynamicValue<T>(FrameInfo frame);
}
