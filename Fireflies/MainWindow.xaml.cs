using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Fireflies.Corrections;
using Fireflies.Transport;
using Fireflies.Frames;
using System.Collections.Generic;

namespace Fireflies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LEDController controller = new LEDController(new SerialPort("COM3", 250000));

        private Dictionary<string, IOrchestrator> orchestrators = new Dictionary<string, IOrchestrator>() {
            { "Disabled", PredefinedOrchestrators.buildDisabled() },
            { "Changing Colors", PredefinedOrchestrators.buildChangingColors() },
            { "Simple Sliding Color", PredefinedOrchestrators.buildSimpleSlidingColor() }
        };
        
        public MainWindow()
        {
            InitializeComponent();
            initializeOrchestrators();

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

                controller.ScreenOrchestrator = orchestrator;
            };

            caseDropdown.SelectionChanged += (target, e) => {
                var orchestrator = orchestrators[(string)caseDropdown.SelectedItem];

                controller.CaseOrchestrator = orchestrator;
            };

            keyboardDropdown.SelectionChanged += (target, e) => {
                var orchestrator = orchestrators[(string)keyboardDropdown.SelectedItem];

                controller.KeyboardOrchestrator = orchestrator;
            };

            screenDropdown.SelectedIndex = 0;
            caseDropdown.SelectedIndex = 0;
            keyboardDropdown.SelectedIndex = 0;
        }

        private void handleFrame() {
            Dispatcher.InvokeAsync(() => {
                caseLEDRenderer.Update(controller.LEDPixels, 0, 23);
                screenLEDRenderer.Update(controller.LEDPixels, 63, 34);

                

                fpsLabel.Content = Math.Round(controller.CurrentFPS);
            });
        }
    }
}
