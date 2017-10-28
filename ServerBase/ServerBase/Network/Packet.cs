using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class Packet : PacketBase
    {
        public override int GetLength()
        {
            throw new NotImplementedException();
        }

        public override int GetPacketSize()
        {
            throw new NotImplementedException();
        }
    }
}
