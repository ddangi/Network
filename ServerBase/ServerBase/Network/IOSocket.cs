using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBase
{
    public class IOSocket
    {
        protected Socket _socket;
        protected SocketAsyncEventArgs _sendEventArgs;
        protected SocketAsyncEventArgs _receiveEventArgs;

        protected object _sendQueueLock = new object();
        protected Queue<Packet> _sendQueue = new Queue<Packet>();

        protected void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OnReceiveCompleted(sender, e);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendCompleted(sender, e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    CloseSocket();
                    break;
                default:
                    break;
            }
        }

        protected void OnReceiveCompleted(object sender, SocketAsyncEventArgs receiveSendEventArgs)
        {
            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                ProcessReceive();
            }
            else
            {
                CloseSocket();
            }
        }

        public void SetEventArgs(Socket socket, SocketAsyncEventArgs sendArgs, SocketAsyncEventArgs receiveArgs)
        {
            _socket = socket;
            _sendEventArgs = sendArgs;
            _receiveEventArgs = receiveArgs;
        }

        public void StartReceive()
        {
            bool pending = _socket.ReceiveAsync(_receiveEventArgs);
            if (!pending)
            {
                ProcessReceive();
            }
        }

        public void ProcessReceive()
        {
            if (_receiveEventArgs.BytesTransferred > 0 && _receiveEventArgs.SocketError == SocketError.Success)
            {
                OnReceive(_receiveEventArgs.Buffer, _receiveEventArgs.Offset, _receiveEventArgs.BytesTransferred);
            }
            else
            {
                Console.WriteLine(string.Format($"error {_receiveEventArgs.SocketError},  transferred {_receiveEventArgs.BytesTransferred}"));
                CloseSocket();
                return;
            }

            StartReceive();
        }

        protected void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            //parse packet
            //process packet
        }

        public void Send(Packet msg)
        {
            Monitor.Enter(_sendQueueLock);
            {
                //if Queue is empty, need to call StartSend() directly
                if (_sendQueue.Count <= 0)
                {
                    _sendQueue.Enqueue(msg);
                    StartSend();
                    return;
                }

                // if Queue is not empty, maybe socket is sending some data.
                // after SendAsync() completed, socket will check and call SendAsync() to send another data
                _sendQueue.Enqueue(msg);
            }
            Monitor.Exit(_sendQueueLock);
        }

        public void StartSend()
        {
            Monitor.Enter(_sendQueueLock);
            {
                // 전송이 아직 완료된 상태가 아니므로 데이터만 가져오고 큐에서 제거하진 않는다.
                Packet msg = _sendQueue.Peek();

                // 헤더에 패킷 사이즈를 기록한다.
                //msg.record_size();

                // 이번에 보낼 패킷 사이즈 만큼 버퍼 크기를 설정하고
                int length = msg.GetLength();
                _sendEventArgs.SetBuffer(this._sendEventArgs.Offset, length);

                // 패킷 내용을 SocketAsyncEventArgs버퍼에 복사한다.
                Array.Copy(msg._buffer, 0, _sendEventArgs.Buffer, _sendEventArgs.Offset, length);

                // 비동기 전송 시작.
                bool pending = _socket.SendAsync(_sendEventArgs);
                if (!pending)
                {
                    ProcessSend();
                }
            }
            Monitor.Exit(_sendQueueLock);
        }

        protected void OnSendCompleted(object sender, SocketAsyncEventArgs receiveSendEventArgs)
        {
            if (receiveSendEventArgs.SocketError == SocketError.Success)
            {
                ProcessSend();
            }
            else
            {
                CloseSocket();
            }
        }

        public void ProcessSend()
        {
            //do someting

            // 전송 완료된 패킷을 큐에서 제거한다.
            Monitor.Enter(_sendQueueLock);

            Packet packet = _sendQueue.Dequeue();
            int queueCount = _sendQueue.Count;

            Monitor.Exit(_sendQueueLock);

            // 아직 전송하지 않은 대기중인 패킷이 있다면 다시한번 전송을 요청한다.
            if (0 < queueCount)
            {
                StartSend();
            }
        }

        public void CloseSocket()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;

            Monitor.Enter(_sendQueueLock);
            _sendQueue.Clear();
            Monitor.Exit(_sendQueueLock);

            OnClose();
        }

        public virtual void OnClose() { }
    }
}
