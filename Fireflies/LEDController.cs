using Fireflies.Capture;
using Fireflies.Corrections;
using Fireflies.Frames;
using Fireflies.Library;
using Fireflies.Orchestrators;
using Fireflies.Transport;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies {
    public class LEDController {
        private const int pixelCount = 97;
        private int keyboardPixelCount;

        public Color[] LEDPixels { get; private set; }
        public Color[] KeyboardPixels { get; private set; }

        private IChoreographer ledOrchestrator;

        private IChoreographer _screenOrchestrator = Library.Orchestrators.black();
        public IChoreographer ScreenOrchestrator {
            get { return _screenOrchestrator; }
            set {
                _screenOrchestrator = value;

                ledOrchestrator = buildLEDOrchestrator();
            }
        }

        private IChoreographer _caseOrchestrator = Library.Orchestrators.black();
        public IChoreographer CaseOrchestrator {
            get { return _caseOrchestrator;  }
            set {
                _caseOrchestrator = value;

                ledOrchestrator = buildLEDOrchestrator();
            }
        }

        private IChoreographer _keyboardOrchestrator = Library.Orchestrators.black();
        public IChoreographer KeyboardOrchestrator {
            get { return _keyboardOrchestrator; }
            set { _keyboardOrchestrator = value; }
        }

        private FPSCounter fps = new FPSCounter();
        private Communicator transport;

        private IFrameSource frameSource;
        private FrameStopwatch frameStopwatch = new FrameStopwatch();
        
        public double CurrentFPS { get => fps.CurrentFPS; }

        public event FrameUpdate FrameReady;

        //private ScreenCapturer screen = new ScreenCapturer(0, 0);
        //private Orchestrators.ScreenColor screenOrchestrator;

        //public byte[] CurrentScreenFrame {
        //    get => screen.CurrentFrame;
        //}

        // public System.Drawing.Bitmap TestFrame {
        //    get => gpuScreenOrchestrator.getBitmap();
        // }

        private KeyboardController keyboard;

        public LEDController(SerialPort serialPort) {
            keyboard = new KeyboardController();
            initializeKeyboard();

            KeyboardPixels = new Color[keyboardPixelCount];
            initializePixelsTo(KeyboardPixels, Colors.Black);

            LEDPixels = new Color[pixelCount];
            initializePixelsTo(LEDPixels, Colors.Black);

            ledOrchestrator = buildLEDOrchestrator();

            ColorCorrectionFunction colorCorrectionForLEDs = ColorCorrectionFn.compose(
                ColorCorrectionFn.correctTemperature(new Color {
                    R = 255, // 255
                    G = 250, // 255
                    B = 170  // 180
                }),
                ColorCorrectionFn.scaleBrightness(0.6f),
                ColorCorrectionFn.correctGamma(1.6f, 1.8f, 2.0f)
            );

            transport = new Communicator(new SerialProtocol(serialPort), colorCorrectionForLEDs);
            
            //var captureTask = new Task(() => screen.Capture(), TaskCreationOptions.LongRunning);
            //captureTask.Start();

            frameSource = new FramerateLimiterSource(transport, 65);
            // frameSource = new RenderingTargetSource();
            frameSource.FrameRequest += handleFrame;

            frameStopwatch.Reset();
            frameSource.Start();
        }

        private void initializeKeyboard() {
            var devices = keyboard.connectedDevices();

            devices.ForEach(connectedDevice => {
                Console.WriteLine("Found device: " + connectedDevice.friendlyName);
            });

            var device = devices.First();

            keyboard.openDevice(device);
            
            keyboardPixelCount = keyboard.getPixelCount();

            keyboard.enableTimerSending();
        }

        private IChoreographer buildLEDOrchestrator() {
            return new Splitter(
                new int[] { 23, 40, 34 },
                new IChoreographer[] { CaseOrchestrator, Library.Orchestrators.black(), ScreenOrchestrator }
            );
        }

        private void handleFrame() {
            FrameInfo frame = frameStopwatch.NewFrame();

            ledOrchestrator.Update(LEDPixels, 0, LEDPixels.Length, frame);
            transport.sendFrame(LEDPixels);

            KeyboardOrchestrator.Update(KeyboardPixels, 0, KeyboardPixels.Length, frame);
            keyboard.setPixels(KeyboardPixels, 0, KeyboardPixels.Length);

            fps.frameReady(frame);

            //screen.NextFrame();

            FrameReady?.Invoke();
        }

        private void initializePixelsTo(Color[] pixels, Color color) {
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = color;
            }
        }
    }
}
