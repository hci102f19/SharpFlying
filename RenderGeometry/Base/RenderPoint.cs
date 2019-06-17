using Emgu.CV;
using Emgu.CV.Structure;
using Geometry.Base;
using SDPoint = System.Drawing.Point;

namespace RenderGeometry.Base
{
    public class RenderPoint
    {
        protected Point Point;

        public RenderPoint(Point point)
        {
            Point = point;
        }

        public SDPoint AsPoint()
        {
            return new SDPoint((int) Point.X, (int) Point.Y);
        }

        public void Render(Image<Bgr, byte> frame, MCvScalar color = default(MCvScalar), int size = 2)
        {
            CvInvoke.Circle(frame, AsPoint(), size, color, -1);
        }

        public static explicit operator RenderPoint(Point v)
        {
            return new RenderPoint(v);
        }
    }
}