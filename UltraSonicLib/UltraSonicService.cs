﻿using System;
using Newtonsoft.Json;
using ServiceLib;
using UDPBase;
using UDPBase.exceptions;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected readonly UDPClient Client = new UDPClient("192.168.1.102", 20002);
        protected Sensors Sensors;

        public UltraSonicService()
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

        protected void Deserialize(string data)
        {
            Sensors = JsonConvert.DeserializeObject<Sensors>(data);
            CalculatePosition();
        }

        private void CalculatePosition()
        {
            if (Sensors == null)
                return;
            Console.WriteLine("Front: " + Sensors.Front.Distance + "cm.");
            Console.WriteLine("Right: " + Sensors.Right.Distance + "cm.");
            Console.WriteLine("Back: " + Sensors.Back.Distance + "cm.");
            Console.WriteLine("Left: " + Sensors.Left.Distance + "cm.");
        }

        public override Response GetLatestResult()
        {
            return null;
        }
    }
}