using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    public interface IHit
    {
        bool Hit(Point point, Vector vector);
    }
}