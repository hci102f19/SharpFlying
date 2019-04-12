using FlightLib;

namespace ServiceLib
{
    public class Response
    {
        public Response(bool isValid, Vector vector, float confidence = 0)
        {
            IsValid = isValid;
            Vector = vector;
            Confidence = confidence;
        }

        public bool IsValid { get; protected set; }
        public Vector Vector { get; protected set; }
        public float Confidence { get; protected set; }
    }
}