using Fireflies.Capture;
using Fireflies.Core;
using Fireflies.Corrections;
using Fireflies.Frames;
using Fireflies.Functions;
using Fireflies.Functions.Color;
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

        private IOrchestrator ledOrchestrator;

        private IOrchestrator _screenOrchestrator = PredefinedOrchestrators.buildDisabled();
        public IOrchestrator ScreenOrchestrator {
            get { return _screenOrchestrator; }
            set {
                _screenOrchestrator = value;

                ledOrchestrator = buildLEDOrchestrator();
            }
        }

        private IOrchestrator _caseOrchestrator = PredefinedOrchestrators.buildDisabled();
        public IOrchestrator CaseOrchestrator {
            get { return _caseOrchestrator;  }
            set {
                _caseOrchestrator = value;

                ledOrchestrator = buildLEDOrchestrator();
            }
        }

        private IOrchestrator _keyboardOrchestrator = PredefinedOrchestrators.buildDisabled();
        public IOrchestrator KeyboardOrchestrator {
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
            KeyboardOrchestrator = buildKeyboardOrchestrator();

            Color correction = new Color {
                R = 255, // 255
                G = 250, // 255
                B = 170  // 180
            };

            transport = new Communicator(
                new SerialProtocol(serialPort),
                (Color c) =>
                    GammaCorrection.correct(
                        BrightnessCorrection.correct(
                            TemperatureCorrection.correct(c, correction),
                            0.6
                        ),
                        1.6f, 1.8f, 2.0f
                        //2.2f
                    )
            );

            //var captureTask = new Task(() => screen.Capture(), TaskCreationOptions.LongRunning);
            //captureTask.Start();

            frameSource = new FramerateLimitSource(transport, 65);
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

        private IOrchestrator buildFadingColorOrchestrator() {
            Color[] colors = {
                Colors.Orange,
                Colors.LightGreen,
                Colors.LightBlue,
                Colors.Yellow,
                Colors.Magenta
            };

            Functional.ProgressFunction progressFn = Fn.linearProgress(TimeSpan.FromMilliseconds(60000));

            return new VectorOrchestrator((scene, frame) => {
                var progress = progressFn(frame);

                scene.drawLine(0, 1, Helpers.crossfade(colors, progress));
            });
        }

        private IOrchestrator buildSimpleSlidingColorOrchestrator(bool reverse = false) {
            float lineLength = 0.2f;

            Functional.ProgressFunction velocityFn = Fn.alternating(Fn.linearProgress(TimeSpan.FromMilliseconds(10000)), 1, -1);
            Functional.ProgressFunction progressFn = Fn.looping(
                Fn.speedProgress(
                    (frame) => velocityFn(frame) * 1f,
                    TimeSpan.FromMilliseconds(2000)
                )
            );

            return new VectorOrchestrator((scene, frame) => {
                var velocity = velocityFn(frame);
                var progress = progressFn(frame);

                var crossfadeFactor = Math.Abs(velocity);

                scene.pushTransform(new Scene.Transform() {
                    offset = reverse ? -progress : progress,
                    scale = 1
                });

                scene.drawFeatheredLine(0, lineLength, Helpers.crossfade(Colors.Green, Colors.Red, crossfadeFactor), lineLength / 2, lineLength / 2);

                scene.popTransform();
            });
        }

        private IOrchestrator buildSlidingRainbowOrchestrator() {
            float lineLength = 1f;

            Functional.ProgressFunction progressFn = Fn.looping(
                //Fn.speedProgress(
                //    Fn.cpuUsageSpeedup(10f),
                //    TimeSpan.FromMilliseconds(30000),
                //    0.98f
                //)
                Fn.linearProgress(TimeSpan.FromMilliseconds(10000))
            );

            return new VectorOrchestrator(
                (scene, frame) => {
                    var progress = progressFn(frame);

                    scene.pushTransform(new Scene.Transform() {
                        offset = progress,
                        scale = 1
                    });

                    float p1 = 0, 
                          p2 = lineLength / 3, 
                          p3 = 2 * lineLength / 3,
                          p4 = lineLength;
                    
                    scene.drawLine(p1, p2, new Scene.Gradient() {
                        from = Colors.Blue,
                        to = Colors.Green
                    });

                    scene.drawLine(p2, p3, new Scene.Gradient() {
                        from = Colors.Green,
                        to = Colors.Red
                    });

                    scene.drawLine(p3, p4, new Scene.Gradient() {
                        from = Colors.Red,
                        to = Colors.Blue
                    });

                    scene.popTransform();
                }
            );
        }

        private IOrchestrator buildSlidingColorOrchestrator() {
            float lineLength = 0.5f;

            Functional.ProgressFunction velocityFn = Fn.alternating(Fn.linearProgress(TimeSpan.FromMilliseconds(10000)), 1, -1);
            Functional.ProgressFunction progressFn = Fn.looping(
                Fn.speedProgress(
                    (frame) => velocityFn(frame) * 2,
                    TimeSpan.FromMilliseconds(1000)
                )
            );

            return new VectorOrchestrator(
                (scene, frame) => {
                    var velocity = velocityFn(frame);
                    var progress = progressFn(frame);
                    var darkening = 1 - Utilities.stretch(Math.Abs(velocity), 0f, 1f, 0.5f, 1f);

                    scene.pushTransform(new Scene.Transform() {
                        offset = progress,
                        scale = 1
                    });

                    scene.drawFeatheredLine(0, lineLength, Helpers.darken(Colors.Green, darkening), lineLength / 3, lineLength / 3);
                    scene.drawFeatheredLine(0.5f, 0.5f + lineLength, Helpers.darken(Colors.DarkGreen, darkening), lineLength / 3, lineLength / 3);

                    scene.popTransform();
                }
            );
        }

        private IOrchestrator buildLEDOrchestrator() {
            //var slidingColor = buildFadingColorOrchestrator();
            var blank = new SolidColor((FrameInfo f) => Colors.Black);
            //screenOrchestrator = new Orchestrators.ScreenColor(screen);

            return new Splitter(
                new int[] { 23, 40, 34 },
                new IOrchestrator[] { CaseOrchestrator, blank, ScreenOrchestrator }
            );
        }

        private IOrchestrator buildKeyboardOrchestrator() {
            return buildFadingColorOrchestrator();
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
