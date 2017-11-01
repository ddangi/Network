using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using USERPACKET;
using System.IO;
using Google.Protobuf;

namespace TcpServer
{
    public class UserSocket : SocketSessionBase
    {
        private int _id;
        public int ID { set { _id = value; } get { return _id; } }

        public UserSocket(Socket socket, ListenSocketBase listenSocket) : base(socket, listenSocket)
        {

        }

        protected override void ProcessPacket(byte[] buffer, int offset, int length)
        {
            Packet packet = new Packet(buffer, offset, length);
            TcpServerCommand command = (TcpServerCommand)packet.GetCommand();

            CodedInputStream protoStream = new CodedInputStream(buffer, Constants.HEADER_SIZE, length - Constants.HEADER_SIZE);

            OnEchoRequest(this, protoStream);
        }

        protected static int OnEchoRequest(UserSocket client, CodedInputStream stream)
        {
            CsEchoRequest proto = CsEchoRequest.Parser.ParseFrom(stream);
            string message = proto.Message;

            Console.WriteLine(message);

            CsEchoOk response = new CsEchoOk();
            response.Message = message;

            Packet packet = new Packet();
            packet.EncodePacket(TcpServerCommand.CsEchoOk, response);
            client.Send(packet);

            return 1;
        }

        public override void Disconnect()
        {
            //server 측은 shutdown 하면 graceful close 생김
            LingerOption lingerOpts = new LingerOption(true, 0);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOpts);
            base.Disconnect();
        }
    }
}
