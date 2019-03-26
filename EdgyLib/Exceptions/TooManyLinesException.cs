using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgyLib.Exceptions
{
    public class TooManyLinesException : Exception
    {
        public TooManyLinesException(string message = "TooManyLinesException") : base(message)
        {
        }
    }
}