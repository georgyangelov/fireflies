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
        // https://github.com/FastLED/FastLED/blob/03d12093a92ee2b64fabb03412aa0c3e4f6384dd/color.h
        private static Color temperature = new Color {
            R = 255,
            G = 241,
            B = 224
        };

        private FPSCounter fps = new FPSCounter();
        private Communicator transport = new Communicator(
            new SerialProtocol(new SerialPort("COM4", 250000)),
            // (Color c) => TemperatureCorrection.correct(GammaCorrection.correct(c, 2.8f), 1f),
            // (Color c) => GammaCorrection.correct(TemperatureCorrection.correct(c, temperature), 2.8f)
            (Color c) => GammaCorrection.correct(c, 2.2f)
            // (Color c) => c
        );
        private IFrameSource frameSource;
        private FrameStopwatch frameStopwatch = new FrameStopwatch();

        public MainWindow()
        {
            InitializeComponent();

            frameSource = new FramerateLimitSource(transport, 60);
            // frameSource = new RenderingTargetSource();
            frameSource.FrameRequest += handleFrame;

            // var capturer = new Capture.ScreenCapturer(0, 0);
            //
            // var captureTask = new Task(() => capturer.Capture(), TaskCreationOptions.LongRunning);
            // captureTask.Start();
            //
            // capturePreview.Capturer = capturer;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            frameStopwatch.Reset();
            frameSource.Start();
        }

        private void handleFrame() {
            FrameInfo frame = frameStopwatch.NewFrame();

            ledRenderer.Update(frame);
            transport.sendFrame(ledRenderer.colors);
            fps.frameReady(frame);

            // capturePreview.Update();
            
            Dispatcher.InvokeAsync(() => {
               fpsLabel.Content = Math.Round(fps.CurrentFPS);
            });
        }
    }
}
