using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USERPACKET;

namespace ServerBase
{
    public class Packet : PacketBase
    {
        //send
        public Packet()
        {
            _header = new Header();
            _buffer = new byte[Constants.MAX_PACKET_SIZE];
            _currentIndex = 0;
            _packetSize = 0;
        }

        //recv
        public Packet(byte[] buffer, int offset, int length)
        {
            _header = new Header();
            _buffer = buffer;
            _currentIndex = offset;
            _packetSize = length;
        }

        public override int GetPacketSize()
        {
            return _packetSize;
        }

        public override int GetBodySize()
        {
            return _header.length - Constants.HEADER_SIZE;
        }

        public override short GetCommand()
        {
            short cmd = BitConverter.ToInt16(_buffer, _currentIndex + Constants.PACKET_LENGTH_SIZE);
            return cmd;
        }

        public void EncodePacket(TcpServerCommand cmd, object response)
        {
            MemoryStream outStream = new MemoryStream(Constants.MAX_PACKET_SIZE);
            outStream.Seek(Constants.PACKET_LENGTH_SIZE, SeekOrigin.Begin);
            outStream.Write(BitConverter.GetBytes((short)cmd), 0, sizeof(short));

            outStream.Seek(Constants.HEADER_SIZE, SeekOrigin.Begin);

            CodedOutputStream output = new CodedOutputStream(outStream);
            global::Google.Protobuf.IMessage msg = response as IMessage;
            if (msg != null)
            {
                msg.WriteTo(output);
                //output.WriteMessage(msg);
                output.Flush();
            }

            short length = (short)outStream.Length;

            outStream.Seek(0, SeekOrigin.Begin);
            outStream.Write(BitConverter.GetBytes(length), 0, sizeof(short));

            outStream.Seek(0, SeekOrigin.Begin);
            outStream.Read(_buffer, 0, length);

            _packetSize = length;
        }
    }
}
