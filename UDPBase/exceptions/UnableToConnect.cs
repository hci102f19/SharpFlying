using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase.exceptions
{
    public class UnableToConnect : Exception
    {
        public UnableToConnect() : base() { }
        public UnableToConnect(string message) : base(message) { }
    }
}
