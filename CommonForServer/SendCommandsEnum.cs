using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonForServer
{
  public enum SendCommandsEnum
  {
    ack,
    get,
    exc,
    eventFromServerAdd,
    eventFromServerRemove,
    eventFromServerClear,
    exit
  }

}
