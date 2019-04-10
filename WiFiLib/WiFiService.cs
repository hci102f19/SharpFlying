using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using ServiceLib;
using WiFiLib.Data;
using WiFiLib.Persistence;

namespace WiFiLib
{
    public class WiFiService : Service
    {
        protected readonly UdpClient Client = new UdpClient();

        protected IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.102"), 20001);

        protected string HELOMessage = "HELO";

        protected Network Network = new Network();

        public WiFiService()
        {
            IgnoreInput = true;
        }

        protected void Connect()
        {
            Client.Connect(EndPoint);
            SendHELO();
        }

        protected void SendHELO()
        {
            var sendBuffer = Encoding.UTF8.GetBytes(HELOMessage);
            Client.Send(sendBuffer, sendBuffer.Length);
        }

        protected override void Run()
        {
            Connect();

            while (IsRunning)
            {
                var receivedData = Client.Receive(ref EndPoint);
                Deserialize(Encoding.UTF8.GetString(receivedData));

                CalculatePosition();
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