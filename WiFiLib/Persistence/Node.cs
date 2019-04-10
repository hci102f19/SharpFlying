namespace WiFiLib.Persistence
{
    public class Node
    {
        public Node(string mac, double latitude, double longitude)
        {
            Mac = mac;

            Latitude = latitude;
            Longitude = longitude;
        }

        public string Mac { get; protected set; }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
    }
}