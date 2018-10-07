﻿using Fireflies.Capture;
using Fireflies.Core;
using Fireflies.Corrections;
using Fireflies.Frames;
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

        public Color[] Pixels { get; private set; }
        private IOrchestrator orchestrator;

        private FPSCounter fps = new FPSCounter();
        private Communicator transport;

        private IFrameSource frameSource;
        private FrameStopwatch frameStopwatch = new FrameStopwatch();
        
        public double CurrentFPS { get => fps.CurrentFPS; }

        public event FrameUpdate FrameReady;

        private ScreenCapturer screen = new ScreenCapturer(0, 0);
        private Orchestrators.ScreenColor screenOrchestrator;

        public byte[] CurrentScreenFrame {
            get => screen.CurrentFrame;
        }

        // public System.Drawing.Bitmap TestFrame {
        //    get => gpuScreenOrchestrator.getBitmap();
        // }

        public LEDController(SerialPort serialPort) {
            Pixels = new Color[pixelCount];
            initializePixelsTo(Colors.Black);

            orchestrator = buildOrchestrator();

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

            var captureTask = new Task(() => screen.Capture(), TaskCreationOptions.LongRunning);
            captureTask.Start();

            frameSource = new FramerateLimitSource(transport, 65);
            // frameSource = new RenderingTargetSource();
            frameSource.FrameRequest += handleFrame;

            frameStopwatch.Reset();
            frameSource.Start();
        }

        private IOrchestrator buildSlidingRainbowOrchestrator() {
            float lineLength = 1f;

            return new VectorOrchestrator(
                Fn.looping(
                    //Fn.speedProgress(
                    //    Fn.cpuUsageSpeedup(10f),
                    //    TimeSpan.FromMilliseconds(30000),
                    //    0.98f
                    //)
                    Fn.linearProgress(TimeSpan.FromMilliseconds(30000))
                ),
                (scene, progress) => {
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

            return new VectorOrchestrator(
                Fn.looping(
                    Fn.linearProgress(TimeSpan.FromMilliseconds(3000))
                ),
                (scene, progress) => {
                    scene.pushTransform(new Scene.Transform() {
                        offset = progress,
                        scale = 1
                    });

                    scene.drawFeatheredLine(0, lineLength, Colors.Green, lineLength / 3, lineLength / 3);
                    scene.drawFeatheredLine(0.5f, 0.5f + lineLength, Colors.DarkRed, lineLength / 3, lineLength / 3);

                    scene.popTransform();
                }
            );
        }

        private IOrchestrator buildOrchestrator() {
            //var slidingColor = buildSlidingRainbowOrchestrator();
            var slidingColor = buildSlidingColorOrchestrator();
            var blank = new SolidColor((FrameInfo f) => Colors.Black);
            screenOrchestrator = new Orchestrators.ScreenColor(screen);

            return new Splitter(
                new int[] { 23, 40, 34 },
                new IOrchestrator[] { blank, blank, screenOrchestrator }
            );
        }

        //private IOrchestrator buildOrchestrator() {
        //    EasingFunction easing = new Functions.Easing.Polynomial(2).EaseInOut;
        //    TimingFunction screenTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Loop;
        //    TimingFunction caseTiming = new Functions.Timing.Looping(new TimeSpan(0, 0, 7)).Alternating;

        //    //  var screenOrchestrator = new Orchestrators.SlidingColor(
        //    //      (FrameInfo f) => screenTiming(f),
        //    //      (FrameInfo f) => Color.Multiply(Colors.Green, 0.2f),
        //    //      (FrameInfo f) => Colors.Green
        //    //  );

        //    var caseOrchestrator = new Orchestrators.SlidingColor(
        //        new Functions.Timing.Speed((FrameInfo f) => 3 * easing(caseTiming(f)) + 0.3).Function,
        //        (FrameInfo f) => Colors.Blue,
        //        (FrameInfo f) => Colors.White
        //    );
        //    var staticCaseOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => Colors.Green);

        //    var blankOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => Colors.Black);

        //    var alignOrchestrator = new Orchestrators.AlignTest();
        //    var screenColorOrchestrator = new Orchestrators.ScreenColor(screen);
        //    var greenOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
        //        A = 255,
        //        R = 0x65,
        //        G = 0xff,
        //        B = 0x00
        //    });
        //    var blueOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
        //        A = 255,
        //        R = 30,
        //        G = 144,
        //        B = 255
        //    });
        //    var grayOrchestrator = new Orchestrators.SolidColor((FrameInfo f) => new Color {
        //        A = 255,
        //        R = 255,
        //        G = 255,
        //        B = 255
        //    });

        //    screenOrchestrator = new Orchestrators.ScreenColor(screen);

        //    var alternatingColor = new Functions.Timing.Looping(TimeSpan.FromSeconds(60));
        //    var colors = new Color[] { Colors.Red, Colors.Green, Colors.Blue, Colors.White };
        //    ColorFunction colorBlend = (FrameInfo f) => {
        //        var progress = alternatingColor.Loop(f) * colors.Length;
        //        var colorIndex = (int)progress % colors.Length;

        //        var colorA = colors[colorIndex];
        //        var colorB = colors[(colorIndex + 1) % colors.Length];

        //        return Functions.Color.Helpers.crossfade(colorA, colorB, progress - colorIndex);
        //    };
        //    var pulsingColorOrchestrator = new Orchestrators.PulsingColors(colorBlend, 34);

        //    return new Orchestrators.Splitter(
        //        new int[] { 23, 40, 34 },
        //        new IOrchestrator[] { pulsingColorOrchestrator, blankOrchestrator, pulsingColorOrchestrator }
        //    );
        //}

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
