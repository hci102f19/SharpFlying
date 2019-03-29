using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Geometry.Base;
using Vector = FlightLib.Vector;

namespace EdgyLib.HitBox
{
    class ReverseHitBox : IHit
    {
        protected Polygon InternalPolygon;
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public ReverseHitBox(int width, int height, double centerWidth = 0.065, double centerHeight = 0.2,
            double centerHeightOffset = 0.25)
        {
            Width = width;
            Height = height;

            double x1 = width * (1 - ((1 - centerWidth) / 2));
            double y1 = height * (((1 - centerHeight) / 2) - centerHeightOffset);
            double x2 = width * ((1 - centerWidth) / 2);
            double y2 = height * (1 - ((1 - centerHeight) / 2) - centerHeightOffset);

            InternalPolygon = new Polygon(
                new V2d(x1, y1),
                new V2d(x2, y1),
                new V2d(x2, y2),
                new V2d(x1, y2),
                new V2d(x1, y1)
            );
        }

        public bool Hit(Point point, Vector vector)
        {
            if (InternalPolygon.Contains(point))
                return false;

            if (0 <= point.X && point.X <= Width)
                vector.Yaw = HorizontalMovement(point.X) * 2;

            return true;
        }

        protected int HorizontalMovement(double movement)
        {
            return ((int)(((double)Width / 2 - movement) / movement) * 100).Clamp(-100, 100);
        }
    }
}