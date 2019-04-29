using FlightLib.Enum;

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