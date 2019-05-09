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

        protected int Difference(double f1, double f2)
        {
            double totalDistance = f1 + f2;
            double sideValue = totalDistance / 2;

            return (int) (Math.Max(f1, f2) - sideValue);
        }

        private double _lastKnownDistanceUsedForCalcLeft = 0;
        private double _lastKnownDistanceUsedForCalcRight = 0;

        protected Vector CalculatePosition()
        {
            Vector movement = new Vector();

            foreach (Tuple<UltrasonicSensor, Vector> sensor in Sensors.GetSensors)
            {
                if (sensor.Item1.Value < 0)
                    continue;

                if (sensor.Item1.Distance < MinDistanceToWall)
                {
                    movement.Add(sensor.Item2.TimesConstant(-1));
                }
            }

            return (movement.IsNull()) ? PostCalculatePosition() : movement;
            return (movement.IsNull()) ? PostCalculatePosition2() : movement;
        }


        protected Vector PostCalculatePosition2()
        {
            Vector movement = new Vector();
            int diff = Difference(Sensors.Left.Distance, Sensors.Right.Distance);

            // - Goes towards Left
            // + Goes towards Right
            double roll = Bebop.AttitudeChanged.RollChanged;

            int rollMax = 1, bebopMaxRoll = 5;
            double rollTolerance = 0.15;

            // Should be 10 cm more to one side for us to react
            if (diff > 10)
            {
                if (Sensors.Left.Value > Sensors.Right.Value)
                {
                    // 20 = 1 Deg
                    // Go right
                    if (Math.Abs(roll - rollMax) < rollTolerance)
                    {
                        // Right power: 20
                        movement.Roll = 20;
                    }
                    else if (roll < rollMax)
                    {
                        // Too little power: 100
                        movement.Roll = 100;
                    }
                    else
                    {
                        // Too much power
                        // 0 - -100
                        double rollPower = -(((roll - rollMax) / (bebopMaxRoll - rollMax)) * 100);
                        movement.Roll = (int) Math.Round(rollPower, 0);
                    }

                    Console.WriteLine("WE GO RIGHT: {0}", movement.Roll);
                }
                else
                {
                    // 20 = 1 Deg
                    // Go left
                    if (Math.Abs(roll - rollMax) < rollTolerance)
                    {
                        // Left power: -20
                        movement.Roll = -20;
                    }
                    else if (roll > rollMax)
                    {
                        // Too little power: -100
                        movement.Roll = -100;
                    }
                    else
                    {
                        // Too much power
                        // 0 - 100
                        double rollPower = (((roll - rollMax) / (bebopMaxRoll - rollMax)) * 100);
                        movement.Roll = (int) Math.Round(rollPower, 0);
                    }

                    Console.WriteLine("WE GO LEFT: {0}", movement.Roll);
                }
            }

            return movement;
        }

        private Vector PostCalculatePosition()
        {
            Vector movement = new Vector();

            // Calculate side-to-side movements
            int diff = Difference(Sensors.Left.Value, Sensors.Right.Value);

            if (diff > 10)
            {
                //Calc default val
                int movementValue = (int) ((Math.Abs(Sensors.Left.Distance - Sensors.Right.Distance) / 200) * 100);
                //Vi skal til venstre!
                if (Sensors.Left.Distance > Sensors.Right.Distance)
                {
                    //Er det første gang vi flyver til venstre?
                    if (Math.Abs(_lastKnownDistanceUsedForCalcLeft) < 0.01)
                    {
                        //"Standard movement"
                        movement.Roll = movementValue;
                        //Gem sidste distance
                        _lastKnownDistanceUsedForCalcLeft = Sensors.Left.Distance;
                        return movement;
                    }

                    //Er vi længere væk end sidst?
                    if (Sensors.Left.Distance < _lastKnownDistanceUsedForCalcLeft)
                    {
                        //Udregner forskellen i procent af total distance
                        var diffTotal = Math.Abs((Sensors.Left.Distance - (Sensors.Right.Distance + Sensors.Left.Distance)) / (Sensors.Left.Distance + Sensors.Right.Distance));
                        //Hvis vi er over 15% til den ene side skal vi begynde at rette op (0.5 i midten)
                        if (diffTotal < 0.35)
                        {
                            var deg = Bebop.AttitudeChanged.RollChanged;
                            //Venstre -> Negativ
                            if (deg > 3)
                            {
                                //Vi skal rette op! -20 ---> test værdi!
                                movement.Roll = -20;
                                _lastKnownDistanceUsedForCalcLeft = Sensors.Left.Distance;
                                return movement;
                            }
                        }
                        else
                        {
                            // Vi er relativt langt væk fra midten; Lav alm movement
                            movement.Roll = movementValue;
                            //Gem sidste distance
                            _lastKnownDistanceUsedForCalcLeft = Sensors.Left.Distance;
                            return movement;
                        }
                    }

                    movement.Roll = -movementValue;
                    _lastKnownDistanceUsedForCalcLeft = Sensors.Left.Distance;
                    return movement;
                }

                //Vi skal til højre!
                if (Sensors.Right.Distance > Sensors.Left.Distance)
                {
                    //Er det første gang vi flyver til venstre?
                    if (Math.Abs(_lastKnownDistanceUsedForCalcRight) < 0.01)
                    {
                        //"Standard movement"
                        movement.Roll = movementValue;
                        //Gem sidste distance
                        _lastKnownDistanceUsedForCalcRight = Sensors.Left.Distance;
                        return movement;
                    }

                    //Er vi længere væk end sidst?
                    if (Sensors.Left.Distance < _lastKnownDistanceUsedForCalcRight)
                    {
                        //Udregner forskellen i procent af total distance
                        var diffTotal = Difference(Sensors.Right.Distance, Sensors.Left.Distance + Sensors.Right.Distance) / (Sensors.Left.Distance + Sensors.Right.Distance);
                        //Hvis vi er over 15% til den ene side skal vi begynde at rette op (0.5 i midten)
                        if (diffTotal < 0.35)
                        {
                            var deg = Bebop.AttitudeChanged.RollChanged;
                            //Venstre -> Negativ
                            if (deg > 3)
                            {
                                //Vi skal rette op! -20 ---> test værdi!
                                movement.Roll = +20;
                                _lastKnownDistanceUsedForCalcRight = Sensors.Right.Distance;
                                return movement;
                            }
                        }
                        else
                        {
                            // Vi er relativt langt væk fra midten; Lav alm movement
                            movement.Roll = -movementValue;
                            //Gem sidste distance
                            _lastKnownDistanceUsedForCalcRight = Sensors.Right.Distance;
                            return movement;
                        }
                    }

                    movement.Roll = movementValue;
                    _lastKnownDistanceUsedForCalcRight = Sensors.Right.Distance;
                    return movement;
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