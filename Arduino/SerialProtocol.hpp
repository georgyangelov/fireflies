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

	Packet packet;
	uint16 lengthLeftToRead = 0;

	const byte BEGIN_MARKER = 0xff,
		       ACK = 0x4b;

public:
	void setup() {
		Serial.begin(250000);
		Serial.setTimeout(500);

		while (!Serial);
	}

	void reset() {
		reading = false;
		packetReady = false;
	}

	void loop() {
		if (!reading && Serial.available() >= sizeof(BEGIN_MARKER) + sizeof(packet.type) + sizeof(packet.length)) {
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
			// Read data
			lengthLeftToRead -= Serial.readBytes(packet.data + packet.length - lengthLeftToRead, min(Serial.available(), lengthLeftToRead));

			if (lengthLeftToRead == 0) {
				reading = false;
				packetReady = true;
			}
		}
	}

	bool hasPacketReady() {
		return packetReady;
	}

	const Packet& getPacket() {
		packetReady = false;

		return packet;
	}

	void ackPacket() {
		if (Serial.availableForWrite() >= sizeof(ACK)) {
			// Writing without checking first may block the operation if there is noone to receive the data
			Serial.write(ACK);
		}
	}
};