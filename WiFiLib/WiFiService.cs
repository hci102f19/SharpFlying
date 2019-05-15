using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLib;
using UDPBase;
using UDPBase.Exception;
using WiFiLib.Data;
using WiFiLib.Persistence;

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
                    {
                        continue;
                    }

                    Network = data;
                    CalculatePosition();
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
            List<Tuple<AccessPoint, Node>> ListAccessPoints = new List<Tuple<AccessPoint, Node>>();

            foreach (AccessPoint accessPoint in Network.AccessPoints)
            {
                foreach (Node node in NetworkMap.Nodes)
                {
                    if (accessPoint.Mac == node.Mac)
                    {
                        ListAccessPoints.Add(new Tuple<AccessPoint, Node>(accessPoint, node));
                    }
                }
            }


            Tuple<AccessPoint, Node> NearestAP = ListAccessPoints.OrderBy(p => p.Item1.Area()).First();

            // Do map stuff
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}