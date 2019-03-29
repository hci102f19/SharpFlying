using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flight;

namespace ServiceLib
{
    public class Response
    {
        public bool IsValid { get; protected set; }
        public Vector Vector { get; protected set; }
        public int Priority { get; protected set; }

        public Response(bool isValid, Vector vector, int priority = 0)
        {
            IsValid = isValid;
            Vector = vector;
            Priority = priority;
        }
    }
}