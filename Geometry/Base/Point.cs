using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Geometry.Extended;

namespace Geometry.Base
{
    public class Point
    {
        protected V2d InternalPoint;

        public double X => InternalPoint.X;
        public double Y => InternalPoint.Y;

        protected Cluster Cluster;

        public Point(double x, double y)
        {
            InternalPoint = new V2d(x, y);
        }

        public Point(V2d point)
        {
            InternalPoint = point;
        }


        public void SetCluster(Cluster cluster)
        {
            cluster.AddPoint(this);
            Cluster = cluster;
        }
    }
}