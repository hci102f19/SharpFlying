using Flight.Enums;

namespace FlightLib
{
    public interface IFly
    {
        ConnectionStatus Connect();
        void TakeOff();
        void Landing();
        void Move(Vector flightVector);
        bool IsAlive();

    }
}