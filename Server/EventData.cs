using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
namespace Server
{
    public class EventData
    {
        public TcpClient Client { get; set; }
        public SendCommandsEnum EventType { get; set; }
    }

}
