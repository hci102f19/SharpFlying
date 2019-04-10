using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase.exceptions
{
    public class ServerStoppedRespondingException : Exception
    {
        public ServerStoppedRespondingException(string message) : base(message) { }
    }
}
