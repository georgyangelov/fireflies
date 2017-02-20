﻿using System;
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
        private Color[] colors;
        private IOrchestrator orchestrator;
        
        private Label fpsLabel = new Label();

        public FrameClock FrameClock { get; set; }

        public LEDStripRenderer() {
            ledCount = 90;
            ledsPerPixel = 1;
            visuals = new Ellipse[ledCount * ledsPerPixel];
            colors = new Color[ledCount];

            EasingFunction easing = new Functions.Easing.Polynomial(2).EaseInOut;
            TimingFunction timing = new Functions.Timing.Looping(new TimeSpan(0, 0, 4)).Alternating;

            orchestrator = new Orchestrators.SlidingColor(
                //(FrameInfo f) => easing(timing(f)),
                (FrameInfo f) => Functions.Utilities.wrapExtend(easing(timing(f)), 0, 1, 0.3),
                //new Functions.Timing.Speed((FrameInfo f) => 0.4 * easing(timing(f)) + 0.2).Function,
                new Functions.Color.Static(Colors.DarkBlue).Function,
                new Functions.Color.Static(Colors.Aquamarine).Function
            );

            initializeColorsTo(Colors.Black);

            createVisuals();
            updateVisuals();
            addVisualsAsChildren();

            fpsLabel.Content = "0";
            fpsLabel.FontSize = 20;
            fpsLabel.Foreground = new SolidColorBrush(Colors.White);

            Children.Add(fpsLabel);

            Loaded += (sender, e) => {
                FrameClock.OnFrame += UpdateColor;
            };

            Unloaded += (sender, e) => {
                FrameClock.OnFrame -= UpdateColor;
            };
        }
        
        private void UpdateColor(FrameInfo frame) {
            orchestrator.Update(colors, frame);
            updateVisuals();

            fpsLabel.Content = (int)FrameClock.CurrentFPS;
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

            fpsLabel.Arrange(new Rect(0, 0, 40, 40));

            return finalSize;
        }
    }
}
