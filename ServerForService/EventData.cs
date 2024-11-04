using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommonForServer;

namespace ServerForService
{
    public class EventData
    {
        public TcpClient Client { get; set; }
        public SendCommandsEnum EventType { get; set; }

    }
}
