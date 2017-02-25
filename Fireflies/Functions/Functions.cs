using Fireflies.Frames;
using System;
using System.Windows.Media;

namespace Fireflies {
    // [0-1] -> [0, 1]
    delegate double EasingFunction(double fraction);

    // FrameInfo -> [0, +inf)
    delegate double TimingFunction(FrameInfo frame);

    // FrameInfo -> any Color
    delegate Color ColorFunction(FrameInfo frame);
}
