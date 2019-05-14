using System;
using FlightLib;
using ServiceLib;
using UDPBase;
using UDPBase.Exception;
using BebopFlying;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected const int MinDistanceToWall = 30;
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

        protected int Difference(double f1, double f2)
        {
            double totalDistance = f1 + f2;
            double sideValue = totalDistance / 2;

            return (int) (Math.Max(f1, f2) - sideValue);
        }

        protected Vector CalculatePosition()
        {
            Vector movement = new Vector();

            foreach (Tuple<UltrasonicSensor, Vector> sensor in Sensors.GetSensors)
            {
                if (sensor.Item1.Value < 0)
                {
                    continue;
                }

                if (sensor.Item1.Distance < MinDistanceToWall)
                {
                    movement.Add(sensor.Item2.TimesConstant(-1));
                }
            }

            return movement.IsNull() ? PostCalculatePosition() : movement;
        }

        protected double Left = 0, Right = 0;

        protected Vector PostCalculatePosition()
        {
            Vector movement = new Vector();

            // Calculate side-to-side movements
            int diff = Difference(Sensors.Left.Distance, Sensors.Right.Distance);

            if (diff > 10)
            {
                // Vi skal til venstre!
                if (Sensors.Left.Distance > Sensors.Right.Distance)
                {
                    movement.Roll = CalculateDirection(diff, Sensors.Left.Distance, Sensors.Right.Distance, Sensors.Left.Distance < Left);
                    SetLastReading();
                    return movement;
                }

                // Vi skal til højre!
                if (Sensors.Right.Distance > Sensors.Left.Distance)
                {
                    movement.Roll = -CalculateDirection(diff, Sensors.Right.Distance, Sensors.Left.Distance, Sensors.Right.Distance < Right);
                    SetLastReading();
                    return movement;
                }
            }

            return movement;
        }

        protected void SetLastReading()
        {
            Left = Sensors.Left.Distance;
            Right = Sensors.Right.Distance;
        }

        protected int CalculateDirection(int diff, double f1, double f2, bool wrongWay)
        {
            //Calc default val
            int movementValue = Map(diff, 1, (float) (f1 + f2), 1, 50);

            //Udregner forskellen i procent af total distance
            double diffTotal = Math.Abs((f1 - (f2 + f1)) / (f1 + f2));

            if (wrongWay)
            {
                if (diffTotal < 0.35)
                {
                    double deg = Bebop.AttitudeChanged.RollChanged;

                    if (deg > 3)
                    {
                        //Vi skal rette op! -20 ---> test værdi!
                        return -10;
                    }
                }

                return movementValue;
            }

            return -movementValue;
        }

        /// <summary>
        /// Accepts a value and a range of the value together with another range
        /// Then it maps the value to the second range
        /// </summary>
        /// <param name="inputVal">Value to map to new range</param>
        /// <param name="inputMin">Input range minimum</param>
        /// <param name="inputMax">Input range maximum</param>
        /// <param name="outputMin">Output range minimum</param>
        /// <param name="outputMax">Output range maximum</param>
        /// <returns></returns>
        public static int Map(float inputVal, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return (int) ((inputVal - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin);
        }

        public override Response GetLatestResult()
        {
            return Response;
        }
    }
}