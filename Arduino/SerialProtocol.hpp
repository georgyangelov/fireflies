#define SERIAL_PACKET_BUFFER_SIZE 2048
#define min(a, b) ((a) < (b) ? (a) : (b))

struct Packet {
	byte type;
	uint16 length;

	byte data[SERIAL_PACKET_BUFFER_SIZE];
};

class SerialProtocol {
	bool reading = false,
		 packetReady = false;

	Timeout& receiveTimeout;

	Packet packet;
	uint16 lengthLeftToRead = 0;

	const byte BEGIN_MARKER = 0xff,
		       END_MARKER = 0xf0,
		       ACK = 0x4b;

public:
	SerialProtocol(Timeout& _receiveTimeout) : receiveTimeout(_receiveTimeout) {
	}

	void setup() {
		Serial.begin(250000);

		while (!Serial);
	}

	void loop() {
		// 4 = begin marker + type + length
		if (!reading && Serial.available() >= 4) {
			receiveTimeout.reset();

			// Read control byte and header
			if (Serial.read() != BEGIN_MARKER) {
				return;
			}

			packet.type = Serial.read();
			Serial.readBytes((byte*)&packet.length, sizeof(uint16_t));

			reading = true;
			lengthLeftToRead = packet.length;

			if (packet.length > SERIAL_PACKET_BUFFER_SIZE) {
				return;
			}
		} else if (reading && Serial.available()) {
			receiveTimeout.reset();

			// Read data
			lengthLeftToRead -= Serial.readBytes(packet.data + packet.length - lengthLeftToRead, min(Serial.available(), lengthLeftToRead));

			if (lengthLeftToRead == 0) {
				reading = false;

				// Read the final data marker
				while (Serial.available() == 0);

				if (Serial.read() == END_MARKER) {
					packetReady = true;
				}
			}
		} else if (receiveTimeout.timedOut()) {
			reading = false;
		}
	}

	bool hasPacketReady() {
		return packetReady;
	}

	const Packet& getPacket() {
		return packet;
	}

	void ackPacket() {
		packetReady = false;
		Serial.write(ACK);
	}
};