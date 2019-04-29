using FlightLib.Enums;

namespace FlightLib
{
    public interface IFly
    {
        ConnectionStatus Connect();
        void TakeOff();
        void Land();
        void Move(Vector flightVector);
        bool IsAlive();
    }
}