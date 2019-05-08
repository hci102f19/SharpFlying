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

            return (int) ((f1 > f2) ? (f2 - sideValue) : (f1 - sideValue));
        }

        private double _lastKnownDistanceUsedForCalcLeft = 0;
        private double _lastKnownDistanceUsedForCalcRight = 0;

        private Vector CalculatePosition()
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

            if (!movement.IsNull())
                return movement;

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