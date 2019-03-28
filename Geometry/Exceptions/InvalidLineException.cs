using System;

namespace Geometry.Exceptions
{
    public class InvalidLineException : Exception
    {
        public InvalidLineException(string message) : base(message)
        {
        }
    }
}