using Aardvark.Base;
using Emgu.CV;
using Emgu.CV.Structure;
using Geometry.Base;
using SDPoint = System.Drawing.Point;

namespace RenderGeometry.Base
{
    public class PolyRender
    {
        protected Polygon Polygon;

        public PolyRender(Polygon polygon)
        {
            Polygon = polygon;
        }

        protected SDPoint AsPoint(V2d point)
        {
            return new SDPoint((int) point.X, (int) point.Y);
        }

        public void Render(Image<Bgr, byte> frame, MCvScalar color = default(MCvScalar))
        {
            foreach (var line in Polygon.InternalPolygon.EdgeLines)
                CvInvoke.Line(frame, AsPoint(line.P0), AsPoint(line.P1), color);
        }

        public static explicit operator PolyRender(Polygon v)
        {
            return new PolyRender(v);
        }
    }
}