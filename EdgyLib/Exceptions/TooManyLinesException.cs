using System;

namespace EdgyLib.Exceptions
{
    public class TooManyLinesException : Exception
    {
        public TooManyLinesException(string message = "TooManyLinesException") : base(message)
        {
        }
    }
}