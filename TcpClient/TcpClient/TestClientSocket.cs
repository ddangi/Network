using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServerBase;
using USERPACKET;
using Google.Protobuf;

namespace TcpClient
{
    public class TestClientSocket : ClientSocketBase
    {
        public TestClientSocket() : base(null)
        {
        }

        public TestClientSocket(Socket socket) : base(socket)
        {
        }

        public override void OnConnected()
        {
            base.OnConnected();

            Log.AddLog("Connected");
        }

        private static IDictionary<TcpServerCommand, Func<TestClientSocket, CodedInputStream, bool>> s_PacketHandlers = new Dictionary<TcpServerCommand, Func<TestClientSocket, CodedInputStream, bool>>();

        public static bool InitializePacketHandler()
        {
            System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            Array commands = Enum.GetValues(typeof(TcpServerCommand));
            foreach (Enum cmd in commands)
            {
                string packetFunction = cmd.ToString();
                if (false == packetFunction.EndsWith("Ok"))
                    continue;

                packetFunction = packetFunction.Replace("Cs", "On");
                //Type packetType = Type.GetType("USERPACKET." + packetProto + ",serverBase");

                System.Reflection.MethodInfo method = typeof(TestClientSocket).GetMethod(packetFunction,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                null, new Type[] { typeof(TestClientSocket), typeof(CodedInputStream) }, null);

                if (method == null)
                {
                    Log.AddLog($"{packetFunction} method is missing");
                    return false;
                }

                s_PacketHandlers.Add((TcpServerCommand)cmd, (Func<TestClientSocket, CodedInputStream, bool>)Delegate.CreateDelegate(typeof(Func<TestClientSocket, CodedInputStream, bool>), method));
            }

            return true;
        }

        protected override void ProcessPacket(byte[] buffer, int offset, int length)
        {
            Packet packet = new Packet(buffer, offset, length);
            TcpServerCommand command = (TcpServerCommand)packet.GetCommand();

            if (false == s_PacketHandlers.ContainsKey(command))
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

        protected static bool OnEchoOk(TestClientSocket socket, CodedInputStream stream)
        {
            CsEchoOk proto = CsEchoOk.Parser.ParseFrom(stream);
            string message = proto.Message;

            Log.AddLog($"Received from server : {message}");

            return true;
        }

        protected static bool OnPingOk(TestClientSocket socket, CodedInputStream stream)
        {
            return true;
        }
    }
}
