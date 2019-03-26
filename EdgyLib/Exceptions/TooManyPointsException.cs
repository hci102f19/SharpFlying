using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgyLib.Exceptions
{
    public class TooManyPointsException : Exception
    {
        public TooManyPointsException(string message = "TooManyPointsException") : base(message)
        {
        }
    }
}