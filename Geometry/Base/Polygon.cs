using Aardvark.Base;

namespace Geometry.Base
{
    public class Polygon
    {
        public Polygon(params V2d[] points)
        {
            InternalPolygon = new Polygon2d(points);
        }

        public Polygon2d InternalPolygon { get; protected set; }

        public bool Contains(Point point)
        {
            return InternalPolygon.BoundingBox2d.Contains(point.InternalPoint);
        }

        public Point Center()
        {
            return new Point(InternalPolygon.ComputeCentroid());
        }
    }
}