using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    abstract public class PacketBase
    {
        protected byte[] _buffer;
        protected IOSocket _socket;

        public abstract int GetPacketSize();
        public abstract int GetLength();
    }
}
