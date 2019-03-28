using System;
using System.Collections.Generic;
using System.Text;
using Geometry.Base;
using Geometry.Extended;
using Geometry.Interfaces;

namespace DBSCAN
{
    public class ClusterSet<T> where T : IPointData
    {
        public IList<Cluster<T>> Clusters { get; internal set; }
        public IList<T> UnclusteredObjects { get; internal set; }
    }
}