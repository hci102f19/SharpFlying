namespace UDPBase.Exception
{
    public class ServerStoppedRespondingException : System.Exception
    {
        public ServerStoppedRespondingException(string message) : base(message)
        {
        }
    }
}