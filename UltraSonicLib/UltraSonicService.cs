using System;
using FlightLib;
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
        protected Response Response;

        protected const int MinDistanceToWall = 20;

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

            if (Sensors != null)
                Response = new Response(true, CalculatePosition());
        }

        protected int Difference(float f1, float f2)
        {
            if (Math.Abs(f1 - f2) < 0.01)
                return 0;

            float max = Math.Max(f1, f2);
            float min = Math.Min(f1, f2);

            return (int)((max - min) / min * 100);
        }

        private Vector CalculatePosition()
        {
            Vector movement = new Vector();

            foreach (Tuple<UltrasonicSensor, Vector> sensor in Sensors.GetSensors)
            {
                if (sensor.Item1.Value < 0)
                    continue;
                if (sensor.Item1.Value < MinDistanceToWall)
                    movement.Add(sensor.Item2.TimesConstant(-1));
            }

            if (!movement.IsNull())
                return movement;

            // Calculate side-to-side movements

            int diff = Difference(Sensors.Left.Value, Sensors.Right.Value);

            if (diff > 10)
            {
                if (Sensors.Left.Value > Sensors.Right.Value)
                    movement.Roll = -diff;
                if (Sensors.Left.Value < Sensors.Right.Value)
                    movement.Roll = diff;
            }

            return movement;
        }

        public override Response GetLatestResult()
        {
            return Response;
        }
    }
}