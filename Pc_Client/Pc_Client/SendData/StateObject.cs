using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pc_Client.SendData
{
    class StateObject
    {
        public Socket WorkSocket = null;

        public const int BufferSize = 5242880;

        public byte[] Buffer = new byte[BufferSize];

        public bool Connected = false;
    }
}
