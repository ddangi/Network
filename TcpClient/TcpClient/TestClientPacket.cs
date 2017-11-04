using ServerBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USERPACKET;

namespace TcpClient
{
    public class TestClientPacket
    {
        public static void SendEchoRequest(TestClientSocket client, string message)
        {
            CsEchoRequest proto = new CsEchoRequest();
            proto.Message = message;

            Packet packet = new Packet();
            packet.EncodePacket(TcpServerCommand.CsEchoRequest, proto);
            client.Send(packet);
        }
    }
}
