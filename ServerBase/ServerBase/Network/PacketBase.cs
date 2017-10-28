using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    abstract public class PacketBase
    {
        public byte[] _buffer;
        protected IOSocket _socket;

        int _length;
        int _sequence;
        int _command;

        int _currentIndex;

        public abstract int GetPacketSize();
        public abstract int GetLength();
    }
}
