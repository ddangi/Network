using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClient
{
    public class TcpTestClient
    {
        private TestClientSocket _testClientSocket;
        public TestClientSocket Socket { get { return _testClientSocket; } }

        SocketAsyncEventArgs _connectEventArg;

        public TcpTestClient()
        {

        }

        public IPEndPoint GetServerEndpointUsingIpAddress(string host, int port)
        {
            IPEndPoint hostEndPoint = null;
            try
            {
                IPAddress theIpAddress = IPAddress.Parse(host);
                // Instantiates the Endpoint.
                hostEndPoint = new IPEndPoint(theIpAddress, port);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (FormatException e)
            {
                Console.WriteLine("FormatException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
            return hostEndPoint;
        }

        public bool Connect(string host, int port)
        {
            _connectEventArg = new SocketAsyncEventArgs();
            _connectEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectCompleted);
            TestClientSocket testClientSocket = new TestClientSocket();
            _connectEventArg.UserToken = testClientSocket;

            _connectEventArg.RemoteEndPoint = GetServerEndpointUsingIpAddress(host, port);
            _connectEventArg.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Post the connect operation on the socket.
            //A local port is assigned by the Windows OS during connect op.

            Log.AddLog($"Try to connect - host : {host} / Port : {port}");
            bool willRaiseEvent = _connectEventArg.AcceptSocket.ConnectAsync(_connectEventArg);
            if (!willRaiseEvent)
            {
                ProcessConnect(_connectEventArg);
            }

            return true;
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                default:
                    break;
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            _testClientSocket = (TestClientSocket)e.UserToken;
            if (e.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs receiveEventArgs = new SocketAsyncEventArgs();
                receiveEventArgs.AcceptSocket = e.AcceptSocket;

                SocketAsyncEventArgs sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.AcceptSocket = e.AcceptSocket;

                _testClientSocket.SetSocketAsyncEventArg(e.AcceptSocket, sendEventArgs, receiveEventArgs);
                _testClientSocket.OnConnected();
                _testClientSocket.StartReceive();

                e.AcceptSocket = null;
            }
            else
            {
                Log.AddLog($"connect failed - Message:{e.SocketError.ToString()}");
            }
        }

        public void Close()
        {
            _testClientSocket.CloseSocket();
        }
    }
}
