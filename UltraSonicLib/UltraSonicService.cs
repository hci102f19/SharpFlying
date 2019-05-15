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

            if (movement.IsNull())
            {
                //Console.WriteLine("Left: {0} - Right {1}", Sensors.Left.Distance, Sensors.Right.Distance);
                //Console.WriteLine("Vi er null");
                return PostCalculatePosition2();
            }
            else
            {
                //Console.WriteLine("Not null!");
                return movement;
            }

            //return movement.IsNull() ? PostCalculatePosition() : movement;
        }

        protected double Left = 0, Right = 0;

        protected Vector PostCalculatePosition()
        {
            Vector movement = new Vector();
            //double deg = Math.Abs(Bebop.AttitudeChanged.RollChanged);
            //Console.WriteLine("Degree: {0} - Left: {1} - Right {2}",deg, Sensors.Left.Distance, Sensors.Right.Distance);
            // Calculate side-to-side movements
            int diff = Difference(Sensors.Left.Distance, Sensors.Right.Distance);


            if (diff > 10)
            {
                // Vi skal til venstre!
                if (Sensors.Left.Distance > Sensors.Right.Distance)
                {
                    movement.Roll = CalculateDirection(diff, Sensors.Left.Distance, Sensors.Right.Distance, Sensors.Left.Distance < Left, -0.4);
                    SetLastReading();
                    return movement;
                }

                // Vi skal til højre!
                if (Sensors.Right.Distance > Sensors.Left.Distance)
                {
                    movement.Roll = -CalculateDirection(diff, Sensors.Right.Distance, Sensors.Left.Distance, Sensors.Right.Distance < Right, 0.4);
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

        protected enum Direction
        {
            None,
            Center,
            Left,
            Right
        }

        protected Direction CurrentDirection = Direction.None;
        protected const int FlyValue = 50, MovementDistance = 1;

        protected Vector PostCalculatePosition2()
        {
            Vector movement = new Vector();


            // Await first reading
            if (Left == 0 && Right == 0)
            {
                SetLastReading();
                return movement;
            }
                

            // We are trying to center us!
            if (CurrentDirection != Direction.Center || CurrentDirection != Direction.None)
            {
                if (CurrentDirection == Direction.Left)
                {
                    movement.Roll = CorrectionValue(Sensors.Left.Distance, Sensors.Right.Distance, Left);
                }

                if (CurrentDirection == Direction.Right)
                {
                    movement.Roll = -CorrectionValue(Sensors.Right.Distance, Sensors.Left.Distance, Right);
                }
            }
           // Console.WriteLine("CTRL");
            if (!movement.IsNull())
            {
                SetLastReading();
                return movement;
            }
               
            
            int diff = Difference(Sensors.Left.Distance, Sensors.Right.Distance);
            if (diff > 10)
            {
                // Til venstre
                if (Sensors.Left.Distance > Sensors.Right.Distance)
                {
                    movement.Roll = CalculateMovement(Direction.Left, Sensors.Left.Distance, Left);
                    CurrentDirection = Direction.Left;
                }
                // Til højre
                else
                {
                    movement.Roll = -CalculateMovement(Direction.Right, Sensors.Right.Distance, Right);
                    CurrentDirection = Direction.Right;
                }
            }
            else
            {
                Console.WriteLine("FUK YA, WE SENDER BOIS!");
                Console.WriteLine("Diff: {0} - Left: {1} - Right {2}",diff, Sensors.Left.Distance, Sensors.Right.Distance);
                CurrentDirection = Direction.Center;
            }
            SetLastReading();
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

        protected int CorrectionValue(double f1, double f2, double previousValue)
        {
            // We there bois!
            if (f1 < f2)
            {
                CurrentDirection = Direction.Center;
                return FlyValue;
            }
            // If we did not move enough, give it a bit more
            else if (f1 - previousValue < MovementDistance)
            {
                //return -(FlyValue / 2);
            }

            //Console.WriteLine("FAK");
            return 0;
        }

        protected int CalculateDirection(int diff, double f1, double f2, bool wrongWay, double maxDegree)
        {
            //Calc default val
            int movementValue = Map(diff, 10, (float) (f1 + f2), 50, 100);

            //Udregner forskellen i procent af total distance
            double diffTotal = Math.Abs((f1 - (f2 + f1)) / (f1 + f2));

            if (wrongWay)
            {
                if (diffTotal < 0.25)
                {
                    double deg = Bebop.AttitudeChanged.RollChanged;
                    //Console.WriteLine("Hældning: {0} ",deg);
                    if (deg > maxDegree)
                    {
                        //Vi skal rette op! -20 ---> test værdi!
                        Console.WriteLine("Håndter angle");
                        return -100;
                    }
                }

                //Console.WriteLine("Regular movement: {0}",movementValue);
                return movementValue;
            }

            //Console.WriteLine("Negative movement: {0}", -movementValue);
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