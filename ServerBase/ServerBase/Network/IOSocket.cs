using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using USERPACKET;

namespace ServerBase
{
    public enum SocketState
    {
        CONNECTED = 0,
        CLOSED
    }

    public abstract class IOSocket
    {
        protected Socket _socket;
        protected SocketState _socketState = SocketState.CLOSED;
        public SocketState State { get { return _socketState; } set { _socketState = value; } }

        protected SocketAsyncEventArgs _sendEventArgs;
        protected SocketAsyncEventArgs _receiveEventArgs;

        protected object _sendQueueLock = new object();
        protected Queue<PacketBase> _sendQueue = new Queue<PacketBase>();

        protected byte[] _recvBuffer;
        protected byte[] _sendBuffer;

        public IOSocket(Socket socket)
        {
            _socket = socket;

            _recvBuffer = new byte[Constants.MAX_PACKET_SIZE];
            _sendBuffer = new byte[Constants.MAX_PACKET_SIZE];
        }

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

        public void StartReceive()
        {
            //socket state check. shuld not be SocketState.CLOSED
            if (State != SocketState.CONNECTED)
            {
                //to do - log
                return;
            }

            try
            {
                bool pending = _socket.ReceiveAsync(_receiveEventArgs);
                if (!pending)
                {
                    ProcessReceive();
                }
            }
            catch
            {
                //to do - log
                CloseSocket();
            }
        }

        public void ProcessReceive()
        {
            if (_receiveEventArgs.BytesTransferred > 0 && _receiveEventArgs.SocketError == SocketError.Success)
            {
                OnReceive(_receiveEventArgs);
                StartReceive();
            }
            else
            {
                Console.WriteLine(string.Format($"error - {_receiveEventArgs.SocketError},  transferred : {_receiveEventArgs.BytesTransferred}"));
                CloseSocket();
                return;
            }

            StartReceive();
        }

        protected void OnReceive(SocketAsyncEventArgs arg)
        {
            //parse packet
            int totalRecvSize = arg.Offset + arg.BytesTransferred;
            int parsedSize = 0;
            ParsePacket(arg.Buffer, totalRecvSize, out parsedSize);

            //if(false == )
            //{
            //    Console.WriteLine(string.Format($"ParsePacket failed -  receivedSize : {receivedOffset}"));
            //}

            //edit buffer offset
            int remainDataSize = totalRecvSize - parsedSize;
            if(0 < remainDataSize && 0 < parsedSize)
            {
                Buffer.BlockCopy(_recvBuffer, parsedSize, _recvBuffer, 0, remainDataSize);
                arg.SetBuffer(remainDataSize, Constants.MAX_PACKET_SIZE - remainDataSize);
            }
            else if(parsedSize == 0)
            {
                arg.SetBuffer(remainDataSize, Constants.MAX_PACKET_SIZE - remainDataSize);
            }
            else if(0 < arg.Offset )
            {
                arg.SetBuffer(0, Constants.MAX_PACKET_SIZE);
            }
        }

        protected void ParsePacket(byte[] buffer, int totalRecvSize, out int parsedSize)
        {
            parsedSize = 0;

            while (true)
            {
                int remainLength = totalRecvSize - parsedSize;
                // 헤더만큼 못읽은 경우 헤더를 먼저 읽는다.
                if (remainLength < Constants.PACKET_LENGTH_SIZE)
                    return;

                // first 2 bytes is "packetLength"
                int packetLength = BitConverter.ToInt16(buffer, parsedSize);
                if (remainLength < packetLength)
                    return;

                if(packetLength < Constants.HEADER_SIZE)
                {
                    //to do - log
                    return;
                }

                ProcessPacket(buffer, parsedSize, packetLength);
                parsedSize += packetLength;
            }
        }

        protected abstract void ProcessPacket(byte[] buffer, int offset, int length);

        public void Send(PacketBase msg)
        {
            if(Constants.MAX_PACKET_SIZE < msg.GetPacketSize())
            {
                //to do - log
                return;
            }

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
                PacketBase msg = _sendQueue.Peek();

                // 헤더에 패킷 사이즈를 기록한다.
                //msg.record_size();

                // 이번에 보낼 패킷 사이즈 만큼 버퍼 크기를 설정하고
                int length = msg.GetPacketSize();
                _sendEventArgs.SetBuffer(0, length);

                // 패킷 내용을 SocketAsyncEventArgs버퍼에 복사한다.
                Buffer.BlockCopy(msg._buffer, 0, _sendEventArgs.Buffer, 0, length);
                //Array.Copy(msg._buffer, 0, _sendEventArgs.Buffer, _sendEventArgs.Offset, length);

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
            Monitor.Enter(_sendQueueLock);
            int size = _sendQueue.Peek().GetPacketSize();
            int sentSize = _sendEventArgs.Offset + _sendEventArgs.BytesTransferred;
            if (sentSize != size && size - sentSize > 0)
            {
                _sendEventArgs.SetBuffer(sentSize, size - sentSize);

                try
                {
                    _socket.SendAsync(_sendEventArgs);
                }
                catch (Exception e)
                {
                    //to do - log
                    CloseSocket();
                }
                Monitor.Exit(_sendQueueLock);
                return;
            }

            // 전송 완료된 패킷을 큐에서 제거한다.
            PacketBase packet = _sendQueue.Dequeue();
            int queueCount = _sendQueue.Count;

            Monitor.Exit(_sendQueueLock);

            // 아직 전송하지 않은 대기중인 패킷이 있다면 다시한번 전송을 요청한다.
            if (0 < queueCount)
            {
                StartSend();
            }
        }

        // CloseSocket -> OnClose -> Disconnect

        public void CloseSocket()
        {
            _socketState = SocketState.CLOSED;
            OnClose();
        }

        public virtual void OnClose()
        {
            Monitor.Enter(_sendQueueLock);
            _sendQueue.Clear();
            Monitor.Exit(_sendQueueLock);

            Disconnect();
        }

        public virtual void Disconnect()
        {
            //to do _socket.Shutdown(SocketShutdown.Both); // <- server 측은 linger timout 0 : shutdown 하면 graceful close 생김
            //client 일때는 shutdown 필요
            _socket.Close();
            _socket = null;

            _sendEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            _sendEventArgs.UserToken = null;
            _receiveEventArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            _receiveEventArgs.UserToken = null;
        }
    }
}
