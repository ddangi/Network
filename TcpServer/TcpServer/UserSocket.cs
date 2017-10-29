using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TcpServer
{
    public class UserSocket : SocketSessionBase
    {
        private Socket _acceptedSocket;
        private SocketAsyncEventArgs _receiveArgs;
        private SocketAsyncEventArgs _sendArgs;

        private int _id;
        public int ID { set { _id = value; } get { return _id; } }

        public UserSocket()
        {
        }

        protected override void ProcessPacket(short cmd, byte[] buffer)
        {

        }
    }
}
