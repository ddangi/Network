using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public abstract class ClientSocketBase : IOSocket
    {
        public ClientSocketBase() : base(null)
        {
        }

        public ClientSocketBase(Socket socket) : base(socket)
        {
        }

        public void SetSocketAsyncEventArg(Socket socket, SocketAsyncEventArgs sendArgs, SocketAsyncEventArgs recvArgs)
        {
            _socket = socket;

            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendArgs.SetBuffer(_sendBuffer, 0, Constants.MAX_PACKET_SIZE);
            sendArgs.UserToken = this;
            _sendEventArgs = sendArgs;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            recvArgs.SetBuffer(_recvBuffer, 0, Constants.MAX_PACKET_SIZE);
            recvArgs.UserToken = this;

            _receiveEventArgs = recvArgs;
        }

        public virtual void OnConnected()
        {
            _socketState = SocketState.CONNECTED;
        }

        public override void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            base.Disconnect();
        }
    }
}
