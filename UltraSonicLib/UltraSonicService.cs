using System;
using FlightLib;
using ServiceLib;
using UDPBase;
using UDPBase.Exception;

namespace UltraSonicLib
{
    public class UltraSonicService : Service
    {
        protected const int MinDistanceToWall = 30, FlyValue = 50;
        protected readonly UDPClient Client = new UDPClient("192.168.4.1", 20002);
        protected Direction CurrentDirection = Direction.None;

        protected double? Left, Right;

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

            return (int) ((Math.Max(f1, f2) - sideValue) * 2);
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
                    Vector vec = sensor.Item2.Copy();
                    movement.Add(vec.TimesConstant(-1));
                }
            }

            Vector postEmergencyVector = PostCalculatePosition();
            SetLastReading();
            return movement.IsNull() ? postEmergencyVector : movement;
        }


        protected void SetLastReading()
        {
            Left = Sensors.Left.Distance;
            Right = Sensors.Right.Distance;
        }


        protected Vector PostCalculatePosition()
        {
            Vector movement = new Vector();


            // Await first reading
            if (Left == null || Right == null)
            {
                return movement;
            }


            // We are trying to center us!
            if (CurrentDirection != Direction.Center || CurrentDirection != Direction.None)
            {
                if (CurrentDirection == Direction.Left)
                {
                    movement.Roll = CorrectionValue(Sensors.Left.Distance, Sensors.Right.Distance);
                }

                if (CurrentDirection == Direction.Right)
                {
                    movement.Roll = -CorrectionValue(Sensors.Right.Distance, Sensors.Left.Distance);
                }
            }

            if (!movement.IsNull())
            {
                return movement;
            }


            int diff = Difference(Sensors.Left.Distance, Sensors.Right.Distance);
            if (diff > 10)
            {
                // Til venstre
                if (Sensors.Left.Distance > Sensors.Right.Distance)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    movement.Roll = CalculateMovement(Direction.Left, Sensors.Left.Distance, (double) Left);
                    CurrentDirection = Direction.Left;
                }
                // Til højre
                else
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    movement.Roll = -CalculateMovement(Direction.Right, Sensors.Right.Distance, (double) Right);
                    CurrentDirection = Direction.Right;
                }
            }
            else
            {
                CurrentDirection = Direction.Center;
            }

            Console.WriteLine("Left: {0}, Right: {1}", Sensors.Left.Distance, Sensors.Right.Distance);
            return movement;
        }

        protected int CalculateMovement(Direction currentDirection, double directionalValve, double previousValue)
        {
            // Har vi sendt den første pakke?
            if (CurrentDirection == currentDirection)
            {
                // We are not moving left yet?
                if (directionalValve > previousValue)
                {
                    return -FlyValue;
                }
            }
            else
            {
                return -FlyValue;
            }

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="d1">First arbitrary double value</param>
        /// <param name="d2">Secondary arbitrary double value</param>
        /// <returns></returns>
        protected int CorrectionValue(double d1, double d2)
        {
            // Check if the values are within the range of center
            if (d1 < d2)
            {
                CurrentDirection = Direction.Center;
                // If we are at center, we need to stop the current drift with an opposite value
                return FlyValue;
            }

            return 0;
        }


        public override Response GetLatestResult()
        {
            return Response;
        }

        protected enum Direction
        {
            None,
            Center,
            Left,
            Right
        }
    }
}