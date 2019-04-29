using FlightLib;

namespace ServiceLib
{
    public class Response
    {
        public Response(bool isValid, Vector vector, double confidence = 1)
        {
            IsValid = isValid;
            Vector = vector;
            Confidence = confidence;
        }

        public bool IsValid { get; protected set; }
        public Vector Vector { get; protected set; }
        public double Confidence { get; protected set; }
    }
}