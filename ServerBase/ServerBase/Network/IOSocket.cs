using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using USERPACKET;

namespace ServerBase
{
    public class IOSocket
    {
        protected Socket _socket;
        protected SocketAsyncEventArgs _sendEventArgs;
        protected SocketAsyncEventArgs _receiveEventArgs;

        protected object _sendQueueLock = new object();
        protected Queue<Packet> _sendQueue = new Queue<Packet>();

        protected int _remainBytes;
        protected int _currentPosition;
        protected int _positionToRead;
        protected byte[] _buffer;

        public IOSocket()
        {
            _buffer = new byte[Constants.MAX_PACKET_SIZE];
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
            _remainBytes = bytesTransferred;

            // 원본 버퍼의 포지션값.
            // 패킷이 여러개 뭉쳐 올 경우 원본 버퍼의 포지션은 계속 앞으로 가야 하는데 그 처리를 위한 변수이다.
            int srcPosition = offset;

            short cmd = 0;
            // 남은 데이터가 있다면 계속 반복한다.
            while (_remainBytes > 0)
            {
                bool completed = false;

                // 헤더만큼 못읽은 경우 헤더를 먼저 읽는다.
                if (_currentPosition < Constants.HEADER_SIZE)
                {
                    // 목표 지점 설정(헤더 위치까지 도달하도록 설정).
                    _positionToRead = Constants.HEADER_SIZE;

                    completed = ReadUntil(buffer, ref srcPosition, offset, bytesTransferred);
                    if (!completed)
                    {
                        // 아직 다 못읽었으므로 다음 receive를 기다린다.
                        return;
                    }

                    // 헤더 하나를 온전히 읽어왔으므로 메시지 사이즈를 구한다.
                    _positionToRead = BitConverter.ToInt16(buffer, 0);
                    cmd = BitConverter.ToInt16(buffer, 2);
                }

                // 메시지를 읽는다.
                completed = ReadUntil(buffer, ref srcPosition, offset, bytesTransferred);

                if (completed)
                {
                    // 패킷 하나를 완성 했다.
                    ProcessPacket(cmd, buffer);

                    ClearBuffer();
                }
            }

            //process packet
        }

        protected bool ReadUntil(byte[] buffer, ref int srcPosition, int offset, int bytesTransferred)
        {
            if (offset + bytesTransferred <= _currentPosition)
            {
                // 들어온 데이터 만큼 다 읽은 상태이므로 더이상 읽을 데이터가 없다.
                return false;
            }

            // 읽어와야 할 바이트.
            // 데이터가 분리되어 올 경우 이전에 읽어놓은 값을 빼줘서 부족한 만큼 읽어올 수 있도록 계산해 준다.
            int copySize = _positionToRead - _currentPosition;

            // 남은 데이터가 더 적다면 가능한 만큼만 복사한다.
            if (_remainBytes < copySize)
            {
                copySize = _remainBytes;
            }

            // 버퍼에 복사.
            Array.Copy(buffer, srcPosition, _buffer, _currentPosition, copySize);


            // 원본 버퍼 포지션 이동.
            srcPosition += copySize;

            // 타겟 버퍼 포지션도 이동.
            _currentPosition += copySize;

            // 남은 바이트 수.
            _remainBytes -= copySize;

            // 목표지점에 도달 못했으면 false
            if (_currentPosition < _positionToRead)
            {
                return false;
            }

            return true;
        }

        protected virtual void ClearBuffer()
        {
            _buffer = new byte[Constants.MAX_PACKET_SIZE];
            _remainBytes = 0;
            _currentPosition = 0;
            _positionToRead = 0;
        }

        protected virtual void ProcessPacket(short cmd, byte[] buffer)
        {
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
                int length = msg.GetPacketSize();
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
