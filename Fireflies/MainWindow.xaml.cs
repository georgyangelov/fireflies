using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Fireflies.Corrections;
using Fireflies.Transport;
using Fireflies.Frames;

namespace Fireflies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static LEDController controller = new LEDController(new SerialPort("COM3", 250000));
        
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (target, e) => {
                controller.FrameReady += handleFrame;
            };

            Unloaded += (target, e) => {
                controller.FrameReady -= handleFrame;
            };

            // capturePreview.Update(controller.TestFrame);
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
