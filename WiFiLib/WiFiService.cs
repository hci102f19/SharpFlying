using System;
using Newtonsoft.Json;
using ServiceLib;
using UDPBase;
using WiFiLib.Data;

namespace WiFiLib
{
    public class WiFiService : Service
    {
        protected readonly UDPClient Client = new UDPClient("192.168.1.102", 20001);

        protected Network Network = new Network();

        public WiFiService()
        {
            IgnoreInput = true;
        }


        protected override void Run()
        {
            Client.Connect();

            while (IsRunning)
            {
                var data = Client.ReceiveData();
                Deserialize(data);
            }
        }

        protected void CalculatePosition()
        {
            Console.WriteLine("TEST");
        }

        protected void Deserialize(string data)
        {
            Network = JsonConvert.DeserializeObject<Network>(data);
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}