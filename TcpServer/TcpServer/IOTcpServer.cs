using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServer
{
    public class IOTcpServer : ListenSocketBase
    {
        private object _userSocketDictLock = new object();
        private Dictionary<int, SocketSessionBase> _userSocketDict = new Dictionary<int, SocketSessionBase>();

        public IOTcpServer() : base()
        {

        }

        protected override void NewClientAccepted(Socket socket)
        {
            SocketAsyncEventArgs receiveArgs = _receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = _sendEventArgsPool.Pop();

            UserSocket client = new UserSocket(socket, this);
            client.SetSocketAsyncEventArg(sendArgs, receiveArgs);
            client.ID = Interlocked.Increment(ref _socketIdSeq);
            client.State = SocketState.CONNECTED;
            
            Monitor.Enter(_userSocketDictLock);
            _userSocketDict[client.ID] = client;
            Monitor.Exit(_userSocketDictLock);

            client.StartReceive();
        }

        public override void Stop()
        {
            base.Stop();

            Monitor.Enter(_userSocketDictLock);
            foreach (UserSocket user in _userSocketDict.Values)
            {
                user.CloseSocket();
            }
            _userSocketDict.Clear();
            Monitor.Exit(_userSocketDictLock);
        }
    }
}
