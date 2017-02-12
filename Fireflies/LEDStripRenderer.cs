using System;
using System.Collections.Generic;
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

namespace Fireflies {
    public class LEDStripRenderer : Panel {
        int ledCount;
        private Ellipse[] visuals;
        private Color[] colors;

        public LEDStripRenderer() {
            ledCount = 30;
            visuals = new Ellipse[ledCount];
            colors = new Color[ledCount];

            initializeColorsTo(Color.FromRgb(0, 255, 0));

            createVisuals();
            updateVisuals();
            addVisualsAsChildren();
        }

        private void initializeColorsTo(Color color) {
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = color;
            }
        }

        private void createVisuals() {
            for (int i = 0; i < ledCount; i++) {
                visuals[i] = new Ellipse();
                visuals[i].Width = 10;
                visuals[i].Height = 10;
            }
        }

        private void updateVisuals() {
            for (int i = 0; i < ledCount; i++) {
                visuals[i].Fill = new SolidColorBrush(colors[i]);
            }
        }

        private void addVisualsAsChildren() {
            foreach (var visual in visuals) {
                Children.Add(visual);
            }
        }

        private Geometry getCircleGeometry(Size size) {
            double padding = 20,
                   halfWidth = size.Width / 2,
                   halfHeight = size.Height / 2,
                   radius = Math.Min(halfWidth, halfHeight) - padding;

            return new EllipseGeometry(new Point(halfWidth, halfHeight), radius, radius);
        }

        private Geometry getRectangleGeometry(Size size) {
            double padding = 20;

            return new RectangleGeometry(
                new Rect(
                    new Point(padding, padding), 
                    new Size(size.Width - 2 * padding, size.Height - 2 * padding)
                )
            );
        }

        protected override Size ArrangeOverride(Size finalSize) {
            int childrenCount = Children.Count;

            var geometry = getCircleGeometry(finalSize).GetFlattenedPathGeometry();
            Point positionPoint;
            Point tangentVector;

            for (int i = 0; i < childrenCount; i++) {
                UIElement child = Children[i];

                geometry.GetPointAtFractionLength(i / (float)childrenCount, out positionPoint, out tangentVector);

                child.Arrange(
                    new Rect(positionPoint, new Size(10, 10))
                );
            }

            return finalSize;
        }
    }
}
