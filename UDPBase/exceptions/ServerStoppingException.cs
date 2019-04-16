using System;

namespace UDPBase.exceptions
{
    public class ServerStoppingException : Exception
    {
        public ServerStoppingException(string message) : base(message)
        {
        }
    }
}