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

        public override void Disconnect()
        {
            //server 측은 shutdown 하면 graceful close 생김
            LingerOption lingerOpts = new LingerOption(true, 0);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOpts);
            base.Disconnect();
        }
        private static IDictionary<TcpServerCommand, Func<UserSocket, CodedInputStream, bool>> s_PacketHandlers = new Dictionary<TcpServerCommand, Func<UserSocket, CodedInputStream, bool>>();

        public static bool InitializePacketHandler()
        {
            System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            Array commands = Enum.GetValues(typeof(TcpServerCommand));
            foreach (Enum cmd in commands)
            {
                string packetFunction = cmd.ToString();
                if (false == packetFunction.EndsWith("Request"))
                    continue;

                packetFunction = packetFunction.Replace("Cs", "On");
                //Type packetType = Type.GetType("USERPACKET." + packetProto + ",serverBase");

                System.Reflection.MethodInfo method = typeof(UserSocket).GetMethod(packetFunction,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                null, new Type[] { typeof(UserSocket), typeof(CodedInputStream) }, null);

                if (method == null)
                {
                    Log.AddLog($"{packetFunction} method is missing");
                    return false;
                }

                s_PacketHandlers.Add((TcpServerCommand)cmd, (Func<UserSocket, CodedInputStream, bool>)Delegate.CreateDelegate(typeof(Func<UserSocket, CodedInputStream, bool>), method));
            }

            return true;
        }

        protected override void ProcessPacket(byte[] buffer, int offset, int length)
        {
            Packet packet = new Packet(buffer, offset, length);
            TcpServerCommand command = (TcpServerCommand)packet.GetCommand();

            if(false == s_PacketHandlers.ContainsKey(command))
            {
                Log.AddLog($"packet handler not fouind : cmd:{command.ToString()}");
                return;
            }

            int count = length - Constants.HEADER_SIZE;
            byte[] copiedBuffer = new byte[count];
            Buffer.BlockCopy(buffer, Constants.HEADER_SIZE, copiedBuffer, 0, count);
            CodedInputStream protoStream = new CodedInputStream(copiedBuffer);

            s_PacketHandlers[command](this, protoStream);
        }

        protected static bool OnEchoRequest(UserSocket client, CodedInputStream stream)
        {
            CsEchoRequest proto = CsEchoRequest.Parser.ParseFrom(stream);
            string message = proto.Message;

            Console.WriteLine(message);

            CsEchoOk response = new CsEchoOk();
            response.Message = message;

            Packet packet = new Packet();
            packet.EncodePacket(TcpServerCommand.CsEchoOk, response);
            client.Send(packet);

            return true;
        }

        protected static bool OnPingRequest(UserSocket client, CodedInputStream stream)
        {
            return true;
        }
    }
}
