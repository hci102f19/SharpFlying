using Aardvark.Base;

namespace Geometry.Base
{
    public class Polygon
    {
        public Polygon2d InternalPolygon { get; protected set; }

        public Polygon(params V2d[] points)
        {
            InternalPolygon = new Polygon2d(points);
        }

        public bool Contains(Point point)
        {
            return InternalPolygon.Contains(point.InternalPoint);
        }

        public Point Center()
        {
            return new Point(InternalPolygon.ComputeCentroid());
        }
    }
}