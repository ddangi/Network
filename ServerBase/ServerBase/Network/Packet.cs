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
        public Packet()
        {
            _header = new Header();
            _buffer = new byte[Constants.MAX_PACKET_SIZE];
        }

        public override int GetPacketSize()
        {
            return _packetSize;
        }

        public override int GetBodySize()
        {
            return _header.length - Constants.HEADER_SIZE;
        }

        public void EncodePacket()
        {
            //본문만 proto로 serialize 하고 헤더는 그냥 넣는다.
            MemoryStream stream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);


        }

        public void DecodePacket()
        {
        }

        public void Send<T>(short cmd, T protoPacket)
        {

        }
    }
}
