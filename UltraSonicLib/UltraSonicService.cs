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
        protected string BYEMessage = "K-BYE";
        protected bool IsConnected = false;

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
            var sendBuffer = Encoding.UTF8.GetBytes(HELOMessage);
            Client.Send(sendBuffer, sendBuffer.Length);
        }

        protected override void Run()
        {
            var packetLoss = 0;
            Connect();
            Client.Client.ReceiveTimeout = 3000;

            while (IsRunning)
            {
                try
                {
                    var receivedData = Encoding.UTF8.GetString(Client.Receive(ref EndPoint));
                    packetLoss = 0;
                    if (!IsConnected)
                    {
                        if (receivedData == HELOMessage)
                        {
                            Console.WriteLine("Sever acknowledge me");
                            IsConnected = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (receivedData == BYEMessage)
                    {
                        Console.WriteLine("Server is stopping, so am i");
                        return;
                    }

                    Deserialize(receivedData);

                    // CalculatePosition();
                }
                catch (SocketException e)
                {
                    if (packetLoss > 3)
                    {
                        Console.WriteLine("Server stopped responding.");
                        return;
                    }
                    packetLoss++;
                }
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