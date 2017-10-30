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
        private SocketAsyncEventArgsPool _receiveEventArgsPool;
        private SocketAsyncEventArgsPool _sendEventArgsPool;
        private int _socketIdSeq = 0;

        private object _userSocketDictLock = new object();
        private Dictionary<int, SocketSessionBase> _userSocketDict = new Dictionary<int, SocketSessionBase>();

        public IOTcpServer() : base()
        {
            InitializeArgs(Constants.MAX_CONNECTION);
        }

        private void InitializeArgs(int capacity)
        {
            _receiveEventArgsPool = new SocketAsyncEventArgsPool(capacity);
            _sendEventArgsPool = new SocketAsyncEventArgsPool(capacity);

            for (int i = 0; i < capacity; i++)
            {
                UserSocket client = new UserSocket();
                // receive pool
                {
                    //이 로직은 ServerSocketBase로 옮
                    SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    BufferManager.Instance.SetBuffer(arg);
                    arg.UserToken = client;

                    _receiveEventArgsPool.Push(arg);
                }

                // send pool
                {
                    SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    BufferManager.Instance.SetBuffer(arg);
                    arg.UserToken = client;

                    _sendEventArgsPool.Push(arg);
                }
            }
        }

        protected override void NewClientAccepted(Socket socket)
        {
            SocketAsyncEventArgs receiveArgs = _receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = _sendEventArgsPool.Pop();

            UserSocket client = new UserSocket(socket);
            client.SetEventArgs(acceptedArgs.AcceptSocket, sendArgs, receiveArgs);
            client.ID = Interlocked.Increment(ref _socketIdSeq);

            //dict에 소켓 추가
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
