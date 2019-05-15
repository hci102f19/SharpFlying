using Aardvark.Base;

namespace Geometry.Base
{
    public class Box
    {
        protected Polygon2d InternalBox;

        public Box(double x1, double y1, double x2, double y2)
        {
            InternalBox = new Polygon2d(
                new V2d(x1, y1),
                new V2d(x2, y1),
                new V2d(x2, y2),
                new V2d(x1, y2),
                new V2d(x1, y1)
            );
        }

        public double Area => InternalBox.ComputeArea();
        public Point Center => new Point(InternalBox.ComputeCentroid());
    }
}