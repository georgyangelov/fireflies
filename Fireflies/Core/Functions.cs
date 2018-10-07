using Fireflies.Frames;
using System;
using System.Windows.Media;

namespace Fireflies {
    // [0-1] -> [0, 1]
    public delegate double EasingFunction(double fraction);

    // FrameInfo -> [0, +inf)
    public delegate double TimingFunction(FrameInfo frame);

    // FrameInfo -> any Color
    public delegate Color ColorFunction(FrameInfo frame);
}
