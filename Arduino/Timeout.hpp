#include "limits.h"

class Timeout {
	unsigned long lastResetTime, timeoutTime;

public:
	Timeout(unsigned long timeoutTimeMillis) : timeoutTime(timeoutTimeMillis), lastResetTime(millis()) {
	}

	void reset() {
		lastResetTime = millis();
	}

	bool timedOut() {
		unsigned long currentTime = millis();

		// millis() overflows (roughly every 50 days)
		if (currentTime < lastResetTime) {
			return (ULONG_MAX - lastResetTime) + currentTime > timeoutTime;
		}
		else {
			return currentTime - lastResetTime > timeoutTime;
		}
	}
};