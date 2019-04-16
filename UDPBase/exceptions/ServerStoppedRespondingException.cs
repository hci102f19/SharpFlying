using System;

namespace UDPBase.exceptions
{
    public class ServerStoppedRespondingException : Exception
    {
        public ServerStoppedRespondingException(string message) : base(message)
        {
        }
    }
}