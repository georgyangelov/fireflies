// #include <NeoPixelBus.h>
#include <FastLED.h>

#define INTERNAL_LED_PORT 2

#include "Timeout.hpp"
#include "BlinkSignal.hpp"
#include "SerialProtocol.hpp"
#include "LEDController.hpp"

LEDController led;

void setup() {
	BlinkSignal::setup();

	led.setup();
}

void loop() {
	led.loop();
}