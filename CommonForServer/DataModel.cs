using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonForServer
{
    public class DataModel
    {
        public CommandsEnums Command { get; set; }
        public string Key { get; set; }
        public byte[] Object { get; set; }
    }

}
