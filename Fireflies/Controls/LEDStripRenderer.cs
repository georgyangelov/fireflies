using Fireflies.Frames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fireflies.Controls {
    public class LEDStripRenderer : Panel {
        public static readonly DependencyProperty PixelCountProperty = DependencyProperty.Register(
            "PixelCount", 
            typeof(int), 
            typeof(LEDStripRenderer),
            new PropertyMetadata(1)
        );
        public static readonly DependencyProperty LEDsPerPixelProperty = DependencyProperty.Register(
            "LEDsPerPixel",
            typeof(int),
            typeof(LEDStripRenderer),
            new PropertyMetadata(1)
        );
        public static readonly DependencyProperty PixelOffsetProperty = DependencyProperty.Register(
            "PixelOffset",
            typeof(int),
            typeof(LEDStripRenderer),
            new PropertyMetadata(0)
        );

        private Ellipse[] visuals;

        public int PixelCount {
            get => (int)GetValue(PixelCountProperty);
            set => SetValue(PixelCountProperty, value);
        }

        public int LEDsPerPixel {
            get => (int)GetValue(LEDsPerPixelProperty);
            set => SetValue(LEDsPerPixelProperty, value);
        }

        public int PixelOffset {
            get => (int)GetValue(PixelOffsetProperty);
            set => SetValue(PixelOffsetProperty, value);
        }

        public void Update(Color[] pixels, int offset, int length) {
            if (visuals == null) {
                return;
            }

            if (length > PixelCount) {
                throw new Exception("Tried to update more pixels than visuals");
            }

            for (int i = 0; i < length; i++) {
                for (int j = 0; j < LEDsPerPixel; j++) {
                    ((SolidColorBrush)visuals[i * LEDsPerPixel + j].Fill).Color = pixels[offset + i];
                }
            }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);

            if (e.Property == PixelCountProperty || e.Property == LEDsPerPixelProperty) {
                resetVisuals();
            }
        }

        private void resetVisuals() {
            if (visuals != null) {
                removeVisualChildren();
            }

            createVisuals();
            addVisualsAsChildren();
        }

        private void createVisuals() {
            visuals = new Ellipse[PixelCount * LEDsPerPixel];

            for (int i = 0; i < visuals.Length; i++) {
                visuals[i] = new Ellipse();
                visuals[i].Width = 10;
                visuals[i].Height = 10;
                visuals[i].Fill = new SolidColorBrush(Colors.Black);
            }
        }

        private void removeVisualChildren() {
            foreach (var visual in visuals) {
                Children.Remove(visual);
            }
        }

        private void addVisualsAsChildren() {
            foreach (var visual in visuals) {
                Children.Add(visual);
            }
        }

        private Geometry getCircleGeometry(Size size) {
            double padding = 5,
                   halfWidth = size.Width / 2,
                   halfHeight = size.Height / 2,
                   radius = Math.Min(halfWidth, halfHeight) - padding;

            return new EllipseGeometry(new Point(halfWidth, halfHeight), radius, radius);
        }

        private Geometry getRectangleGeometry(Size size) {
            double padding = 5;

            return new RectangleGeometry(
                new Rect(
                    new Point(padding, padding), 
                    new Size(size.Width - 2 * padding, size.Height - 2 * padding)
                )
            );
        }

        protected override Size ArrangeOverride(Size finalSize) {
            if (visuals == null) {
                return finalSize;
            }

            var geometry = getRectangleGeometry(finalSize).GetFlattenedPathGeometry();
            Point positionPoint;
            Point tangentVector;

            for (int i = 0; i < visuals.Length; i++) {
                UIElement child = visuals[wrap(i - PixelOffset, visuals.Length)];

                // TODO: Breaks when the geometry is a point
                geometry.GetPointAtFractionLength(i / (float)visuals.Length, out positionPoint, out tangentVector);

                child.Arrange(
                    new Rect(positionPoint, new Size(10, 10))
                );
            }

            return finalSize;
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
