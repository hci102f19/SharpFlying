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
        public bool IsValid { get; }
        public Vector Vector { get; }

        public Response(bool isValid, Vector vector)
        {
            IsValid = isValid;
            Vector = vector;
        }
    }
}