using System.Collections.Generic;

namespace WiFiLib.Persistence
{
    public static class NetworkMap
    {
        public static List<Node> Nodes = new List<Node>
        {
            new Node("FC:5B:39:84:92:C2", 10, 10),
            new Node("1C:1D:86:EE:3F:62", 20, 20),
            new Node("FC:5B:39:84:91:22", 30, 30)
        };
    }
}