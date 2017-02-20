using System;
using System.Threading.Tasks;
using System.Windows;

namespace Fireflies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var frameClock = new FrameClock();

            ledRenderer.FrameClock = frameClock;
            capturePreview.FrameClock = frameClock;

            // var capturer = new Capture.ScreenCapturer(0, 0);
            //
            // var captureTask = new Task(() => capturer.Capture(), TaskCreationOptions.LongRunning);
            // captureTask.Start();
            //
            // capturePreview.Capturer = capturer;

            frameClock.Start();
        }
    }
}
