using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WiFiLib.Persistence
{
    public static class NetworkMap
    {
        public static List<Node> Nodes = new List<Node>()
        {
            new Node("FC:5B:39:84:92:C2", 10, 10),
            new Node("1C:1D:86:EE:3F:62", 20, 20),
            new Node("FC:5B:39:84:91:22", 30, 30),
        };
    }
}