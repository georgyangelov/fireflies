using Fireflies.Capture;
using Fireflies.Corrections;
using Fireflies.Frames;
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

        public Color[] Pixels { get; private set; }
        private IOrchestrator orchestrator;

        private FPSCounter fps = new FPSCounter();
        private Communicator transport;

        private IFrameSource frameSource;
        private FrameStopwatch frameStopwatch = new FrameStopwatch();
        
        public double CurrentFPS { get => fps.CurrentFPS; }

        public event FrameUpdate FrameReady;

        private ScreenCapturer screen = new ScreenCapturer(0, 0);

        public System.Drawing.Bitmap CurrentScreenFrame {
            get => screen.CurrentFrame;
        }

        public LEDController(SerialPort serialPort) {
            Pixels = new Color[pixelCount];
            initializePixelsTo(Colors.Black);

            orchestrator = buildOrchestrator();

            Color correction = new Color {
                R = 255,
                G = 255,
                B = 180
            };

            transport = new Communicator(
                new SerialProtocol(serialPort),
                (Color c) =>
                    GammaCorrection.correct(
                        BrightnessCorrection.correct(
                            TemperatureCorrection.correct(c, correction),
                            0.5
                        ),
                        1.6f, 1.8f, 2.0f
                        //2.2f
                    )
            );

            var captureTask = new Task(() => screen.Capture(), TaskCreationOptions.LongRunning);
            captureTask.Start();

            frameSource = new FramerateLimitSource(transport, 60);
            frameSource.FrameRequest += handleFrame;

            frameStopwatch.Reset();
            frameSource.Start();
        }

        private IOrchestrator buildOrchestrator() {
            EasingFunction easing = new Functions.Easing.Polynomial(2).EaseInOut;
            TimingFunction screenTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Loop;
            TimingFunction caseTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Alternating;

            var screenOrchestrator = new Orchestrators.SlidingColor(
                (FrameInfo f) => screenTiming(f),
                (FrameInfo f) => Color.Multiply(Colors.Green, 0.2f),
                (FrameInfo f) => Colors.Green
            );

            var caseOrchestrator = new Orchestrators.SlidingColor(
                new Functions.Timing.Speed((FrameInfo f) => 3 * easing(caseTiming(f)) + 0.3).Function,
                (FrameInfo f) => Colors.Black,
                (FrameInfo f) => Colors.Red
            );

            var blankOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => Colors.Black);

            var alignOrchestrator = new Orchestrators.AlignTest();
            var screenColorOrchestrator = new Orchestrators.ScreenColor(screen);
            var greenOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
                A = 255,
                R = 0x65,
                G = 0xff,
                B = 0x00
            });
            var blueOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
                A = 255,
                R = 30,
                G = 144,
                B = 255
            });
            var grayOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
                A = 255,
                R = 255,
                G = 255,
                B = 255
            });

            return new Orchestrators.Splitter(
                new int[] { 23, 44, 30 },
                new IOrchestrator[] { blankOrchestrator, blankOrchestrator, screenColorOrchestrator }
            );
        }

        private void handleFrame() {
            FrameInfo frame = frameStopwatch.NewFrame();

            orchestrator.Update(Pixels, 0, Pixels.Length, frame);

            transport.sendFrame(Pixels);
            fps.frameReady(frame);

            screen.NextFrame();

            FrameReady?.Invoke();
        }

        private void initializePixelsTo(Color color) {
            for (int i = 0; i < Pixels.Length; i++) {
                Pixels[i] = color;
            }
        }
    }
}
