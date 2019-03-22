using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry.Exceptions
{
    public class InvalidLineException : Exception
    {
        public InvalidLineException(string message) : base(message)
        {
        }
    }
}
