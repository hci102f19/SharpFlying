using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    internal abstract class HitBox : IHit
    {
        protected Polygon InternalPolygon;

        protected HitBox(int? force)
        {
            Force = force ?? 100;
        }

        public int Force { get; protected set; }


        public bool Hit(Point point, Vector vector)
        {
            if (InternalPolygon.Contains(point))
            {
                vector.Roll = Force;
                return true;
            }

            return false;
        }

        public Point Center()
        {
            return InternalPolygon.Center();
        }
    }
}