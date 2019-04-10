using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase.exceptions
{
    public class NoAcknowledgement : Exception
    {
        public NoAcknowledgement(string message) : base(message) { }
    }
}