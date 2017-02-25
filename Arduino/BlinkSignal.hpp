class BlinkSignal {
public:
	static void setup() {
		pinMode(INTERNAL_LED_PORT, OUTPUT);
		digitalWrite(INTERNAL_LED_PORT, HIGH);
	}

	static void signal(int blinks, int count = 3) {
		for (int i = 0; i < count; i++) {
			for (int j = 0; j < blinks; j++) {
				digitalWrite(INTERNAL_LED_PORT, LOW);
				delay(200);
				digitalWrite(INTERNAL_LED_PORT, HIGH);
				delay(200);
			}

			delay(1000);
		}
	}
};