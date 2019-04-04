using Emgu.CV;
using Emgu.CV.Structure;
using FlightLib;
using Geometry.Base;
using RenderGeometry.Base;

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

        public void Render(Image<Bgr, byte> frame)
        {
            ((PolyRender) InternalPolygon).Render(frame);
        }

        public Point Center()
        {
            return InternalPolygon.Center();
        }
    }
}