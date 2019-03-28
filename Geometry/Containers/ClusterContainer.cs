using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry.Base;
using Geometry.Extended;

namespace Geometry.Containers
{
    public class ClustersClusterContainer
    {
        protected List<Cluster> Clusters = new List<Cluster>();

        public Cluster GetCluster(int index)
        {
            return Clusters[index] ?? (Clusters[index] = new Cluster());
        }

        public Point BestClusterAsPoint()
        {
            return Clusters.OrderBy(n => n.Density()).Reverse().FirstOrDefault().GetMean();
        }
    }
}