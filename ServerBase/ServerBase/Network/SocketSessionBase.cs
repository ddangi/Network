using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public abstract class SocketSessionBase : IOSocket
    {
        protected ListenSocketBase _listenSocket;

        public SocketSessionBase(Socket socket, ListenSocketBase listenSocket) : base(socket)
        {
            _socket = socket;
            _listenSocket = listenSocket;
        }

        //set event args()
        public void SetSocketAsyncEventArg(SocketAsyncEventArgs sendArgs, SocketAsyncEventArgs receiveArgs)
        {
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendArgs.SetBuffer(_sendBuffer, 0, Constants.MAX_PACKET_SIZE);
            //BufferManager.Instance.SetBuffer(sendArgs);
            sendArgs.UserToken = this;

            _sendEventArgs = sendArgs;

            receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            receiveArgs.SetBuffer(_recvBuffer, 0, Constants.MAX_PACKET_SIZE);
            //BufferManager.Instance.SetBuffer(receiveArgs);
            receiveArgs.UserToken = this;

            _receiveEventArgs = receiveArgs;
        }

        public override void Disconnect()
        {
            if(_sendEventArgs != null)
                _sendEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed); ;

            if(_receiveEventArgs != null)
                _receiveEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed); ;

            _listenSocket.ReturnSocketAsyncEventArgs(_sendEventArgs, _receiveEventArgs);
        }
    }
}
