using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry.Exceptions
{
    public class TooManyLinesException : Exception
    {
        public TooManyLinesException(string message) : base(message)
        {
        }
    }
}
