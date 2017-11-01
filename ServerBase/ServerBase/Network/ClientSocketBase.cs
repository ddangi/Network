using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class ClientSocketBase : IOSocket
    {
        public ClientSocketBase(Socket socket) : base(socket)
        {
        }

        protected override void ProcessPacket(byte[] buffer, int offset, int length)
        {
        }

        public override void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            base.Disconnect();
        }
    }
}
