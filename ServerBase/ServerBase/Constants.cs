using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public static class Constants
    {
        public const int BACKLOG = 1000;
        public const int MAX_CONNECTION = 5000;

        public const int MAX_PACKET_SIZE = 8192;
    }
}
