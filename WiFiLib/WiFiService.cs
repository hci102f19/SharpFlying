using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using ServiceLib;
using UDPBase;
using UDPBase.exceptions;
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
                try
                {
                    var data = Client.ReceiveData();
                    if (data == null)
                        continue;

                    Deserialize(data);
                }
                catch (ServerStoppedResponding)
                {
                    Console.WriteLine("Lost connection, trying to reconnect");
                    Client.ReConnect();
                }
                catch (ServerStopping)
                {
                    Console.WriteLine("Server closing, trying to reconnect");
                    Client.ReConnect();
                }
                catch (NoAcknowledgement)
                {
                    Console.WriteLine("Server did not acknowledge client, trying to reconnect");
                    Client.ReConnect();
                }
            }
        }

        protected void CalculatePosition()
        {
            Console.WriteLine("TEST");
        }

        protected void Deserialize(string data)
        {
            Console.WriteLine("WiFly: " + data);
            //Network = JsonConvert.DeserializeObject<Network>(data);
            //CalculatePosition();
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}