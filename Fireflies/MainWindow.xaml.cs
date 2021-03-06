﻿using Fireflies.Core;
using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using Fireflies.Library;
using Fireflies.Choreographers.Keyboard;

namespace Fireflies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LEDController controller = new LEDController(new SerialPort("COM3", 250000));

        private Dictionary<string, ChoreographyFunction> orchestrators = new Dictionary<string, ChoreographyFunction>() {
            { "Disabled", ChoreographyFn.staticColor(new Color { A = 0, R = 0, G = 0, B = 0 }) },
            { "Alignment Test", ChoreographyFn.alignmentTest() },
            { "Random Colors", ChoreographyFn.randomColor() },
            { "Changing Colors", ChoreographyFn.changingColors() },
            { "Simple Sliding Color (Green)", ChoreographyFn.simpleSlidingColor(Colors.Green, ProgressFn.linear(TimeSpan.FromMilliseconds(10000))) },
            { "Sliding Color with velocity", ChoreographyFn.slidingColorWithVelocity() },
            { "Rainbow", ChoreographyFn.rainbow() },
            { "Key Trails", ChoreographyFn.keyTrails() },
            { "Key Trails With Changing color background", ChoreographyFn.keyTrailsWithBackground(ChoreographyFn.darken(ChoreographyFn.changingColors(), 0.5f)) },
            { "Key Trails With White Background", ChoreographyFn.keyTrailsWithBackground(ChoreographyFn.staticColor(ColorFn.darken(Colors.White, 0.9f))) },
            { "Key Trails With Mask on Rainbow", ChoreographyFn.mask(ChoreographyFn.rainbow(), ChoreographyFn.keyTrails()) },
            { "Ambilight", ChoreographyFn.ambilight(new Capture.ScreenCapturer(0, 0)) }
        };
        
        public MainWindow()
        {
            InitializeComponent();
            initializeOrchestrators();
            initializeBrightnessSlider();
            
            Loaded += (target, e) => {
                controller.FrameReady += handleFrame;
            };

            Unloaded += (target, e) => {
                controller.FrameReady -= handleFrame;
            };

            // capturePreview.Update(controller.TestFrame);
        }

        private void initializeOrchestrators() {
            foreach (var key in orchestrators.Keys) {
                screenDropdown.Items.Add(key);
                caseDropdown.Items.Add(key);
                keyboardDropdown.Items.Add(key);
            }

            screenDropdown.SelectionChanged += (target, e) => {
                var orchestrator = orchestrators[(string)screenDropdown.SelectedItem];

                controller.ScreenChoreography = orchestrator;
            };

            caseDropdown.SelectionChanged += (target, e) => {
                var orchestrator = orchestrators[(string)caseDropdown.SelectedItem];

                controller.CaseChoreography = orchestrator;
            };

            keyboardDropdown.SelectionChanged += (target, e) => {
                var orchestrator = orchestrators[(string)keyboardDropdown.SelectedItem];

                controller.KeyboardChoreography = orchestrator;
            };

            screenDropdown.SelectedIndex = 0;
            caseDropdown.SelectedIndex = 0;
            keyboardDropdown.SelectedIndex = 0;
        }

        private void initializeBrightnessSlider() {
            brightnessSlider.ValueChanged += (target, e) => {
                controller.Brightness = (float)brightnessSlider.Value;
            };
        }

        private void handleFrame() {
            Dispatcher.InvokeAsync(() => {
                brightnessSlider.Value = controller.Brightness;

                caseLEDRenderer.Update(controller.LEDPixels, 0, 23);
                screenLEDRenderer.Update(controller.LEDPixels, 63, 34);

                fpsLabel.Content = Math.Round(controller.CurrentFPS);
            });
        }
    }
}
