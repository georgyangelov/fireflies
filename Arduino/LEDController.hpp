#define PIXEL_COUNT 97
#define MODEL WS2812B
#define OUTPUT_PIN 4
#define COLOR_ORDER BRG

// NeoPixelBus<NeoBrgFeature, NeoEsp8266Uart800KbpsMethod> ledStrip(100, 5);

class LEDController {
	Timeout receiveDataTimeout;
	Timeout receivePacketTimeout;
	SerialProtocol protocol;
	CRGB leds[PIXEL_COUNT];

public:
	LEDController() : receiveDataTimeout(1000), protocol(receiveDataTimeout), receivePacketTimeout(5000) {
	}

	void setup() {
		protocol.setup();

		FastLED.addLeds<MODEL, OUTPUT_PIN, COLOR_ORDER>(leds, PIXEL_COUNT); // .setCorrection(TypicalSMD5050);
		FastLED.setTemperature(Tungsten100W);
		FastLED.setBrightness(128);

		FastLED.show();

		// ledStrip.Begin();
		// ledStrip.Show();
	}

	void loop() {
		protocol.loop();

		if (protocol.hasPacketReady()) {
			receivePacketTimeout.reset();

			const Packet* packet = &protocol.getPacket();

			if (packet->type == 1) {
				for (uint16 i = 0; i < packet->length / 3; i++) {
					leds[i] = CRGB(packet->data[i * 3], packet->data[i * 3 + 1], packet->data[i * 3 + 2]);
					// ledStrip.SetPixelColor(i, RgbColor(packet->data[i * 3], packet->data[i * 3 + 1], packet->data[i * 3 + 2]));
				}

				// ledStrip.Show();
				FastLED.show();
			} else {
				BlinkSignal::signal(5);
			}

			protocol.ackPacket();
		} else if (receivePacketTimeout.timedOut()) {
			receivePacketTimeout.reset();
			turnOffLeds();
		}
	}

private:

	void turnOffLeds() {
		auto black = CRGB(0, 0, 0);

		for (uint16 i = 0; i < PIXEL_COUNT; i++) {
			leds[i] = black;
		}

		FastLED.show();
	}
};