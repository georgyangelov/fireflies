using System;

namespace Fireflies.Functions.Color {
    class Static {
        private System.Windows.Media.Color color;

        public Static(System.Windows.Media.Color color) {
            this.color = color;
        }

        public System.Windows.Media.Color Function(FrameInfo frame) {
            return color;
        }
    }
}
