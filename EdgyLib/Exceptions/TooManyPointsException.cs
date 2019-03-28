using System;

namespace EdgyLib.Exceptions
{
    public class TooManyPointsException : Exception
    {
        public TooManyPointsException(string message = "TooManyPointsException") : base(message)
        {
        }
    }
}