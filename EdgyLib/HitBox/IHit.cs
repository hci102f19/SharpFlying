using Emgu.CV;
using Emgu.CV.Structure;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    public interface IHit
    {
        bool Hit(Point point, Vector vector);

        void Render(Image<Bgr, byte> frame);
    }
}