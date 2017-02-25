using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Fireflies.Corrections;
using Fireflies.Transport;

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

        private FrameClock frameClock = new FrameClock();
        private Communicator transport = new Communicator(
            new SerialProtocol(new SerialPort("COM4", 250000)),
            // (Color c) => TemperatureCorrection.correct(GammaCorrection.correct(c, 2.8f), 1f),
            // (Color c) => GammaCorrection.correct(TemperatureCorrection.correct(c, temperature), 2.8f)
            (Color c) => GammaCorrection.correct(c, 2.2f)
            // (Color c) => c
        );

        public MainWindow()
        {
            InitializeComponent();
            // serial.sendLEDUpdate(ledRenderer.colors);

            ledRenderer.FrameClock = frameClock;
            capturePreview.FrameClock = frameClock;

            // var capturer = new Capture.ScreenCapturer(0, 0);
            //
            // var captureTask = new Task(() => capturer.Capture(), TaskCreationOptions.LongRunning);
            // captureTask.Start();
            //
            // capturePreview.Capturer = capturer;

            frameClock.Start();

            frameClock.OnFrame += UpdateFPSCounter;
        }

        private void UpdateFPSCounter(FrameInfo frame) {
            transport.sendFrame(ledRenderer.colors);

            fpsLabel.Content = Math.Round(frameClock.CurrentFPS);
        }
    }
}
