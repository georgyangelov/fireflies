using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Transport {
    public class Communicator {
        public delegate Color Correction(Color c);

        private Correction colorCorrection;
        private SerialProtocol protocol;

        public Communicator(SerialProtocol protocol, Correction colorCorrection) {
            this.protocol = protocol;
            this.colorCorrection = colorCorrection;
        }

        public void sendFrame(Color[] pixels) {
            if (!protocol.availableForSending()) {
                return;
            }

            UInt16 packetLength = Convert.ToUInt16(pixels.Length * 3);

            Packet packet = new Packet {
                type = 1,
                length = packetLength,
                data = new byte[packetLength]
            };

            for (int i = 0; i < pixels.Length; i++) {
                Color color = colorCorrection(pixels[i]);

                packet.data[i * 3 + 0] = color.R;
                packet.data[i * 3 + 1] = color.G;
                packet.data[i * 3 + 2] = color.B;
            }

            protocol.sendPacket(packet);
        }
    }
}
