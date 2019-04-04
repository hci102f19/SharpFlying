using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WiFiLib.Persistence
{
    public class Node
    {
        public string Mac { get; protected set; }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }

        public Node(string mac, double latitude, double longitude)
        {
            Mac = mac;

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}