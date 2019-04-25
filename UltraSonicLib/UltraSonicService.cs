using System;
using FlightLib;
using ServiceLib;
using UDPBase;
using UDPBase.exceptions;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected const int MinDistanceToWall = 20;
        protected readonly UDPClient Client = new UDPClient("192.168.4.1", 20002);
        protected Response Response;
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
                    Sensors data = Client.ReceiveData<Sensors>();

                    if (data != null)
                    {
                        Sensors = data;
                        Response = new Response(true, CalculatePosition(), 75);
                    }
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

        protected int Difference(float f1, float f2)
        {
            if (Math.Abs(f1 - f2) < 0.01)
            {
                return 0;
            }

            float max = Math.Max(f1, f2);
            float min = Math.Min(f1, f2);

            return (int) ((max - min) / min * 100);
        }

        private Vector CalculatePosition()
        {
            Vector movement = new Vector();

            foreach (Tuple<UltrasonicSensor, Vector> sensor in Sensors.GetSensors)
            {
                if (sensor.Item1.Value < 0)
                {
                    continue;
                }

                if (sensor.Item1.Value < MinDistanceToWall)
                {
                    movement.Add(sensor.Item2.TimesConstant(-1));
                }
            }

            if (!movement.IsNull())
            {
                return movement;
            }

            // Calculate side-to-side movements

            int diff = Difference(Sensors.Left.Value, Sensors.Right.Value);

            if (diff > 10)
            {
                if (Sensors.Left.Value > Sensors.Right.Value)
                {
                    movement.Roll = -diff;
                }

                if (Sensors.Left.Value < Sensors.Right.Value)
                {
                    movement.Roll = diff;
                }
            }

            return movement;
        }

        public override Response GetLatestResult()
        {
            return Response;
        }
    }
}