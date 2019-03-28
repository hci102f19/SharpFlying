using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry.Base;
using Geometry.Extended;
using Geometry.Interfaces;

namespace Geometry.Containers
{
    public class ClustersClusterContainer<T> where T : IPointData
    {
        protected List<Cluster<T>> Clusters = new List<Cluster<T>>();

        public Cluster<T> GetCluster(int index)
        {
            return Clusters[index] ?? (Clusters[index] = new Cluster<T>());
        }

        public Point BestClusterAsPoint()
        {
            return Clusters.OrderBy(n => n.Density()).Reverse().FirstOrDefault().GetMean();
        }
    }
}