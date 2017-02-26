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
        private static LEDController controller = new LEDController(new SerialPort("COM4", 250000));
        
        public MainWindow()
        {
            InitializeComponent();
            
            // var capturer = new Capture.ScreenCapturer(0, 0);
            //
            // var captureTask = new Task(() => capturer.Capture(), TaskCreationOptions.LongRunning);
            // captureTask.Start();
            //
            // capturePreview.Capturer = capturer;

            Loaded += (target, e) => {
                controller.FrameReady += handleFrame;
            };

            Unloaded += (target, e) => {
                controller.FrameReady -= handleFrame;
            };
        }

        private void handleFrame() {
            Dispatcher.InvokeAsync(() => {
                // capturePreview.Update();

                caseLEDRenderer.Update(controller.Pixels, 0, 23);
                screenLEDRenderer.Update(controller.Pixels, 67, 30);
                fpsLabel.Content = Math.Round(controller.CurrentFPS);
            });
        }
    }
}
