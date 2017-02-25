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

namespace Fireflies {
    public class LEDStripRenderer : Panel {
        int ledCount, ledsPerPixel;
        private Ellipse[] visuals;
        public Color[] colors;
        private IOrchestrator orchestrator;

        public IFrameSource FrameSource { get; set; }

        public LEDStripRenderer() {
            ledCount = 97;
            ledsPerPixel = 3;
            visuals = new Ellipse[ledCount * ledsPerPixel];
            colors = new Color[ledCount];

            EasingFunction easing = new Functions.Easing.Polynomial(2).EaseInOut;
            TimingFunction screenTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Loop;
            TimingFunction caseTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Alternating;

            var screenOrchestrator = new Orchestrators.SlidingColor(
                // (FrameInfo f) => Functions.Utilities.wrapExtend(easing(screenTiming(f)), 0, 1, 0.3),
                (FrameInfo f) => screenTiming(f),
                (FrameInfo f) => Color.Multiply(Colors.Green, 0.2f),
                (FrameInfo f) => Colors.Green
            );

            var caseOrchestrator = new Orchestrators.SlidingColor(
                new Functions.Timing.Speed((FrameInfo f) => 3 * easing(caseTiming(f)) + 0.3).Function,
                (FrameInfo f) => Colors.Black,
                (FrameInfo f) => Colors.Red
            );

            //new Orchestrators.SlidingColor(
            //timing,
            //  (FrameInfo f) => easing(timing(f)),
            //(FrameInfo f) => Functions.Utilities.wrapExtend(easing(timing(f)), 0, 1, 0.3),
            //new Functions.Timing.Speed((FrameInfo f) => 0.4 * easing(timing(f)) + 0.2).Function,
            //  new Functions.Color.Static(Colors.Black).Function,
            //  new Functions.Color.Static(Colors.Orange).Function
            //);

            var blankOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => Colors.Black);

            orchestrator = new Orchestrators.Splitter(
                new int[] { 23, 44, 30 },
                new IOrchestrator[] { caseOrchestrator, blankOrchestrator, screenOrchestrator }
            );

            // orchestrator = new Orchestrators.SolidColor((FrameInfo f) => Colors.White);

            initializeColorsTo(Colors.Black);

            createVisuals();
            updateVisuals();
            addVisualsAsChildren();
        }
        
        public void Update(FrameInfo frame) {
            orchestrator.Update(colors, 0, colors.Length, frame);

            Dispatcher.InvokeAsync(() => {
                updateVisuals();
            });
        }

        private void initializeColorsTo(Color color) {
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = color;
            }
        }

        private void createVisuals() {
            for (int i = 0; i < visuals.Length; i++) {
                visuals[i] = new Ellipse();
                visuals[i].Width = 10;
                visuals[i].Height = 10;
                visuals[i].Fill = new SolidColorBrush(Colors.Black);
            }
        }

        private void updateVisuals() {
            for (int i = 0; i < ledCount; i++) {
                for (int j = 0; j < ledsPerPixel; j++) {
                    ((SolidColorBrush)visuals[i * ledsPerPixel + j].Fill).Color = colors[i];
                }
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
            double padding = 50;

            return new RectangleGeometry(
                new Rect(
                    new Point(padding, padding), 
                    new Size(size.Width - 2 * padding, size.Height - 2 * padding)
                )
            );
        }

        protected override Size ArrangeOverride(Size finalSize) {
            var geometry = getRectangleGeometry(finalSize).GetFlattenedPathGeometry();
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

            return finalSize;
        }
    }
}
