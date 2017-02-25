using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Transport {
    public struct Packet {
        public byte type;
        public UInt16 length;

        public byte[] data;
    }

    public class SerialProtocol {
        private SerialPort serial;
        private bool receivedAck = true;

        public SerialProtocol(SerialPort serial) {
            this.serial = serial;

            serial.BaudRate = 250000;
            serial.Open();

            while (!serial.IsOpen);

            serial.DataReceived += dataReceived;
        }

        private void dataReceived(object sender, SerialDataReceivedEventArgs e) {
            while (serial.BytesToRead > 0) {
                int response = serial.ReadByte();

                if (response == 0x4b) {
                    receivedAck = true;
                }
            }
        }
        
        public bool availableForSending() {
            return receivedAck;
        }

        public void sendPacket(Packet p) {
            byte[] lengthBuffer = BitConverter.GetBytes(p.length);

            serial.BaseStream.WriteByte(0xff);
            serial.BaseStream.WriteByte(p.type);
            serial.BaseStream.Write(lengthBuffer, 0, sizeof(UInt16));
            serial.BaseStream.Write(p.data, 0, p.length);
            serial.BaseStream.WriteByte(0xf0);

            receivedAck = false;
        }
    }
}
