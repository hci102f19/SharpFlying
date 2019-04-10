using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServiceLib;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected readonly UdpClient Client = new UdpClient();

        protected IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.102"), 20001);

        protected string HELOMessage = "HELO";

        public UltraSonicService()
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
            var sendBuffer = Encoding.ASCII.GetBytes(HELOMessage);
            Client.Send(sendBuffer, sendBuffer.Length);
        }

        protected override void Run()
        {
            Connect();

            while (IsRunning)
            {
                var receivedData = Client.Receive(ref EndPoint);
                Deserialize(Encoding.ASCII.GetString(receivedData));

                CalculatePosition();
            }
        }

        protected void CalculatePosition()
        {
            Console.WriteLine("TEST");
        }

        protected void Deserialize(string data)
        {
            Console.WriteLine(data);
            // Network = JsonConvert.DeserializeObject<Network>(data);
        }

        public override Response GetLatestResult()
        {
            return null;
        }

    }
}