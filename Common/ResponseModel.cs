using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
  public class ResponseModel
  {
    public SendCommandsEnum Command { get; set; }

    public Exception Exception { get; set; }
    public byte[] Object { get; set; }
  }
}
