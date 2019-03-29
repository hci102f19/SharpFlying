using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    internal abstract class HitBox : IHit
    {
        protected Polygon InternalPolygon;
        public int Force { get; protected set; }

        protected HitBox(int? force)
        {
            Force = force ?? 100;
        }


        public bool Hit(Point point, Vector vector)
        {
            if (InternalPolygon.Contains(point))
            {
                vector.Roll = Force;
                return true;
            }

            return false;
        }

        public Point Center() => InternalPolygon.Center();
    }
}