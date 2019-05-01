using System;
using ServiceLib;
using UDPBase;
using UDPBase.Exception;
using WiFiLib.Data;

namespace WiFiLib
{
    public class WiFiService : Service
    {
        protected readonly UDPClient Client = new UDPClient("192.168.4.1", 20001);

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
                    Network data = Client.ReceiveData<Network>();
                    if (data == null)
                        continue;

                    Network = data;
                }
                catch (ServerStoppedRespondingException)
                {
                    Console.WriteLine("Lost connection, trying to reconnect");
                    if (!Client.ReConnect())
                    {
                        Stop();
                        return;
                    }
                }
                catch (ServerStoppingException)
                {
                    Console.WriteLine("Server closing, trying to reconnect");
                    if (!Client.ReConnect())
                    {
                        Stop();
                        return;
                    }
                }
                catch (NoAcknowledgementException)
                {
                    Console.WriteLine("Server did not acknowledge client, trying to reconnect");
                    if (!Client.ReConnect())
                    {
                        Stop();
                        return;
                    }
                }
            }
        }

        protected void CalculatePosition()
        {
            Console.WriteLine("TEST");
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}