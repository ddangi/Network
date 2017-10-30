using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public short length;    // 2
        public short cmd;       // 2
        
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Constants.HEADER_SIZE];
            int offset = 0;
            
            byte[] ar = BitConverter.GetBytes(length);
            Buffer.BlockCopy(ar, 0, buffer, offset, sizeof(short));
            offset += sizeof(short);
            
            byte[] cmdBy = BitConverter.GetBytes(length);
            Buffer.BlockCopy(cmdBy, 0, buffer, offset, sizeof(short));
            offset += sizeof(short);
        }
    }

    abstract public class PacketBase
    {
        protected IOSocket _socket;

        protected Header _header;
        public byte[] _buffer;
        public int _currentIndex;
        protected int _packetSize;

        public abstract int GetPacketSize();
        public abstract int GetBodySize();
    }
}
