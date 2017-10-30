using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public abstract class ListenSocketBase : IOSocket
    {
        protected Socket _listenSocket;
        protected int _port;
        protected bool _bShutdown;

        public bool Start(int port)
        {
            _bShutdown = false;
            _port = port;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            IPEndPoint localEndPoint = null;
            foreach (IPAddress ipAddress in ipHostInfo.AddressList)
            {
                if (true == ipAddress.IsIPv6LinkLocal || true == ipAddress.IsIPv6Multicast)
                    continue;

                localEndPoint = new IPEndPoint(ipAddress, _port);
                break;
            }

            // Create a TCP/IP socket.
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bool result = true;
            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _listenSocket.Bind(localEndPoint);
                _listenSocket.Listen(Constants.BACKLOG);

                StartAccept();
            }
            catch (Exception e)
            {
                result = false;
                Console.WriteLine(e.ToString());
            }

            return result;
        }

        protected void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptEventArgCompleted);

            //AcceptAsync : Returns false if the I / O operation completed synchronously.
            //The SocketAsyncEventArgs.Completed event on the e parameter will not be raised
            // and the e object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation
            bool isPending = _listenSocket.AcceptAsync(acceptEventArg);
            if (false == isPending)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        protected void LoopToStartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if(_bShutdown != false)
            {
                bool isPending = _listenSocket.AcceptAsync(acceptEventArg);
                if (false == isPending)
                {
                    ProcessAccept(acceptEventArg);
                }
            }
        }

        protected void OnAcceptEventArgCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        protected void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                Console.WriteLine($"SocketError, message - {acceptEventArgs.SocketError.ToString()}");
                LoopToStartAccept(acceptEventArgs);                
                return;
            }
            
            Socket clientSocket = acceptEventArgs.AcceptSocket;
            if(clientSocket != null)
                NewClientAccepted(clientSocket);

            acceptEventArgs.AcceptSocket = null;
            LoopToStartAccept(acceptEventArgs);
            
            //성능에 문제가 있다면 LoopToStartAccept 로직을 쪼개서
            //accetpAsync 한 다음 
            //NewClientAccepted 하고
            //ProcessAccept 호출하는 형태로 수정하자
        }

        protected abstract void NewClientAccepted(Socket socket);

        public virtual void Stop()
        {
            _bShutdown = true;

            if (_listenSocket != null)
            {
                _listenSocket.Close();
                _listenSocket = null;
            }
        }
    }
}
