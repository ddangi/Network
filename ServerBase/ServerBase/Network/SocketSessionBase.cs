using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public class SocketSessionBase : IOSocket
    {
        protected ListenSocketBase _listenSocket;

        public SocketSessionBase(Socket socket)
        {
            _socket = socket;
        }
        
        //set event args()
        //recvEventArg.UserToken = this;
        //sendEventArg.UserToken = this;
        //reciveWait?
        
        //Disconnect()
        //풀에 eventArg push
    }
}
