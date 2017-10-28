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

            bool isPending = _listenSocket.AcceptAsync(acceptEventArg);
            if (true == isPending)
            {
                //Console.WriteLine("AcceptAsync is pending");

            }
        }

        protected void LoopToStartAccept()
        {
            if(_bShutdown != false)
                StartAccept();
        }

        protected void OnAcceptEventArgCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        protected void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                LoopToStartAccept();
                Console.WriteLine($"SocketError, message - {acceptEventArgs.SocketError.ToString()}");
                return;
            }

            NewClientAccepted(acceptEventArgs);

            acceptEventArgs.AcceptSocket = null;
            LoopToStartAccept();
        }

        protected abstract void NewClientAccepted(SocketAsyncEventArgs acceptedArgs);

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
