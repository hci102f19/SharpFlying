using FlightLib;

namespace ServiceLib
{
    public class Response
    {
        public Response(bool isValid, Vector vector, int priority = 0)
        {
            IsValid = isValid;
            Vector = vector;
            Priority = priority;
        }

        public bool IsValid { get; protected set; }
        public Vector Vector { get; protected set; }
        public int Priority { get; protected set; }
    }
}