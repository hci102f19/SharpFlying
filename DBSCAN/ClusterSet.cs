using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Geometry.Extended;

namespace DBSCAN
{
    public class ClusterSet
    {
        public List<Cluster> Clusters { get; internal set; }
        public List<PointContainer> UnclusteredObjects { get; internal set; }

        public bool IsValid()
        {
            return Clusters.Count > 0;
        }

        public Cluster GetBestCluster()
        {
            if (!IsValid())
                throw new Exception("Clusters is not available");
            return Clusters.OrderByDescending(p => p.Points.Count).First();

        }
    }
}