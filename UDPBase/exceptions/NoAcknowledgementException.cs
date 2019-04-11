using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UDPBase.exceptions
{
    public class NoAcknowledgementException : Exception
    {
        public NoAcknowledgementException(string message) : base(message) { }
    }
}