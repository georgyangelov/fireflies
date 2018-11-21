using Fireflies.Capture;
using Fireflies.Core;
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

        public ChoreographyFunction ScreenChoreography { get; set; } = ChoreographyFn.black();
        public ChoreographyFunction CaseChoreography { get; set; } = ChoreographyFn.black();
        public ChoreographyFunction KeyboardChoreography { get; set; } = ChoreographyFn.black();

        private ChoreographyFunction ledChoreography;
        private ChoreographyFunction actualKeyboardChoreography;

        public float Brightness { get; set; } = 1;

        private FPSCounter fps = new FPSCounter();
        private Communicator transport;

        private IFrameSource frameSource;
        private FrameStopwatch frameStopwatch = new FrameStopwatch();
        
        public double CurrentFPS { get => fps.CurrentFPS; }

        public event FrameUpdate FrameReady;

        private KeyboardController keyboard;

        public LEDController(SerialPort serialPort) {
            keyboard = new KeyboardController();
            initializeKeyboard();

            KeyboardPixels = new Color[keyboardPixelCount];
            initializePixelsTo(KeyboardPixels, Colors.Black);

            LEDPixels = new Color[pixelCount];
            initializePixelsTo(LEDPixels, Colors.Black);

            ledChoreography = ChoreographyFn.correction(
                ChoreographyFn.segment(
                    new int[] { 23, 40, 34 },
                    new ChoreographyFunction[] {
                        ChoreographyFn.dynamic(frame => CaseChoreography),
                        ChoreographyFn.black(),
                        ChoreographyFn.dynamic(frame => ScreenChoreography)
                    }
                ),
                frame => ColorCorrectionFn.scaleBrightness(Brightness)
            );

            actualKeyboardChoreography = ChoreographyFn.correction(
                ChoreographyFn.dynamic(frame => KeyboardChoreography),
                frame => ColorCorrectionFn.scaleBrightness(Brightness)
            );

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

        private void handleFrame() {
            FrameInfo frame = frameStopwatch.NewFrame();

            ledChoreography(LEDPixels, 0, LEDPixels.Length, frame);
            transport.sendFrame(LEDPixels);

            actualKeyboardChoreography(KeyboardPixels, 0, KeyboardPixels.Length, frame);
            keyboard.setPixels(KeyboardPixels, 0, KeyboardPixels.Length);

            fps.frameReady(frame);

            FrameReady?.Invoke();
        }

        private void initializePixelsTo(Color[] pixels, Color color) {
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = color;
            }
        }
    }
}
