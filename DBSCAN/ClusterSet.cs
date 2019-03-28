using System;
using System.Collections.Generic;
using System.Text;
using Geometry.Base;
using Geometry.Extended;

namespace DBSCAN
{
    public class ClusterSet
    {
        public IList<Cluster> Clusters { get; internal set; }
        public IList<PointContainer> UnclusteredObjects { get; internal set; }
    }
}