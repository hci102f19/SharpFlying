using System;

namespace UDPBase.exceptions
{
    public class NoAcknowledgementException : Exception
    {
        public NoAcknowledgementException(string message) : base(message)
        {
        }
    }
}