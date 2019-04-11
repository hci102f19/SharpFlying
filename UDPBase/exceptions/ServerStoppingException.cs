using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase.exceptions
{
    public class ServerStoppingException : Exception
    {
        public ServerStoppingException(string message) : base(message) { }
    }
}
