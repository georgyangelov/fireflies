// #include <NeoPixelBus.h>
#include <ESP8266WiFi.h>
#include <FastLED.h>

#define INTERNAL_LED_PORT 2

#include "Timeout.hpp"
#include "BlinkSignal.hpp"
#include "SerialProtocol.hpp"
#include "LEDController.hpp"

LEDController led;

void setup() {
	WiFi.mode(WIFI_OFF);

	BlinkSignal::setup();

	led.setup();
}

void loop() {
	led.loop();
}