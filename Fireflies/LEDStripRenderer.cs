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

namespace Fireflies {
    public class LEDStripRenderer : Panel {
        int ledCount;
        private Ellipse[] visuals;
        private Color[] colors;
        private IOrchestrator orchestrator;

        private TimeSpan elapsedTime = new TimeSpan(0);

        private double currentFps = 0;
        private Label fpsLabel = new Label();

        public LEDStripRenderer() {
            ledCount = 30;
            visuals = new Ellipse[ledCount];
            colors = new Color[ledCount];
            orchestrator = new Orchestrators.RandomColor();

            initializeColorsTo(Color.FromRgb(0, 255, 0));

            createVisuals();
            updateVisuals();
            addVisualsAsChildren();

            fpsLabel.Content = "0";
            fpsLabel.FontSize = 20;

            Children.Add(fpsLabel);

            Loaded += (sender, e) => {
                CompositionTarget.Rendering += UpdateColor;
            };

            Unloaded += (sender, e) => {
                CompositionTarget.Rendering -= UpdateColor;
            };
        }
        
        private void UpdateColor(object sender, EventArgs e) {
            var renderEvent = (RenderingEventArgs)e;
            var frameTime = renderEvent.RenderingTime - elapsedTime;

            if (frameTime.Ticks <= 0) {
                return;
            }

            elapsedTime = renderEvent.RenderingTime;
            
            currentFps = currentFps * 0.95 + (1 / frameTime.TotalSeconds) * 0.05;
            fpsLabel.Content = ((int)currentFps).ToString();

            orchestrator.Update(frameTime, colors);
            updateVisuals();
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
                visuals[i].Fill = new SolidColorBrush(colors[i]);
            }
        }

        private void updateVisuals() {
            for (int i = 0; i < ledCount; i++) {
                ((SolidColorBrush)visuals[i].Fill).Color = colors[i];
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
            var geometry = getCircleGeometry(finalSize).GetFlattenedPathGeometry();
            Point positionPoint;
            Point tangentVector;

            for (int i = 0; i < visuals.Length; i++) {
                UIElement child = visuals[i];

                // TODO: Breaks when the geometry is a point
                geometry.GetPointAtFractionLength(i / (float)visuals.Length, out positionPoint, out tangentVector);

                child.Arrange(
                    new Rect(positionPoint, new Size(10, 10))
                );
            }

            fpsLabel.Arrange(new Rect(0, 0, 40, 40));

            return finalSize;
        }
    }
}
