#include "KeyboardController.h"
#include <iostream>

int main() {
	KeyboardController* controller = create_controller();

	auto devices = controller->enumerateConnectedDevices();

	if (devices.empty()) {
		std::cerr << "No connected devices\n";
		return 1;
	}

	if (devices.size() > 1) {
		std::cerr << "More than one supported device is connected. Cannot choose.\n";
		return 2;
	}

	auto device = devices[0];

	controller->openDevice(&device);

	CTColour* colors = controller->getPixelBuffer();
	for (int i = 0; i < controller->getPixelCount(); i++) {
		colors[i] = CTColour{255, 0, 0, 255};
	}

	controller->enableTimerSending();

	std::cout << "Press enter to stop...";
	std::cin.get();

	controller->disableTimerSending();
	controller->closeDevice();

	delete controller;

	return 0;
}