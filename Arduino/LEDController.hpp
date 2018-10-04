#define PIXEL_COUNT 97
#define MODEL WS2812B
#define OUTPUT_PIN 4 // D2
#define COLOR_ORDER BRG

class LEDController {
	Timeout receiveDataTimeout;
	Timeout receivePacketTimeout;
	SerialProtocol protocol;
	CRGB leds[PIXEL_COUNT];

public:
	LEDController() : receiveDataTimeout(1000), protocol(receiveDataTimeout), receivePacketTimeout(1000) {
	}

	void setup() {
		protocol.setup();

		FastLED.addLeds<MODEL, OUTPUT_PIN, COLOR_ORDER>(leds, PIXEL_COUNT); // .setCorrection(TypicalSMD5050);
		// FastLED.setTemperature(Tungsten100W);
		// FastLED.setBrightness(128);

		FastLED.show();
	}

	void loop() {
		protocol.loop();

		if (protocol.hasPacketReady()) {
			receivePacketTimeout.reset();

			const Packet* packet = &protocol.getPacket();

			if (packet->type == 1) {
				for (uint16 i = 0; i < packet->length / 3; i++) {
					leds[i] = CRGB(packet->data[i * 3], packet->data[i * 3 + 1], packet->data[i * 3 + 2]);
				}

				FastLED.show();
			} else {
				showStaticColor(CRGB(0, 0, 128));
				BlinkSignal::signal(5);
			}

			protocol.ackPacket();
		} else if (protocol.hasTimedOut() || receivePacketTimeout.timedOut()) {
			receivePacketTimeout.reset();
			turnOffLeds();
		}
	}

private:

	void showStaticColor(CRGB color) {
		for (uint16 i = 0; i < PIXEL_COUNT; i++) {
			leds[i] = color;
		}

		FastLED.show();
	}

	void turnOffLeds() {
		auto black = CRGB(0, 0, 0);

		showStaticColor(black);
	}
};