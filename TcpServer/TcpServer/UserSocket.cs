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
        private Socket _acceptedSocket;
        private SocketAsyncEventArgs _receiveArgs;
        private SocketAsyncEventArgs _sendArgs;

        private int _id;
        public int ID { set { _id = value; } get { return _id; } }

        public UserSocket(Socket socket) : base(socket)
        {
            
        }

        protected override void ProcessPacket(short cmd, byte[] buffer)
        {
            TcpServerCommand command = (TcpServerCommand)cmd;
            int length = _positionToRead - Constants.HEADER_SIZE;
            CodedInputStream protoStream = new CodedInputStream(buffer, Constants.HEADER_SIZE, length);

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
    }
}
