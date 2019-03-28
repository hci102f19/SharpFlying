using Geometry.Base;
using Geometry.Extended;

namespace DBSCAN
{
    public class PointInfo
    {
        public PointInfo(PointContainer item) => this.Item = item;

        public PointContainer Item { get; }
        public Cluster Cluster { get; set; }
        public bool Visited { get; set; }

        public ref readonly Point Point => ref Item.Point;

        public double X => Item.X;
        public double Y => Item.Y;
    }
}