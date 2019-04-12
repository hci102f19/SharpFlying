using System.Collections.Generic;
using System.Linq;
using Geometry.Extended;

namespace DBSCANLib
{
    public static class DBSCAN
    {
        public static ClusterSet CalculateClusters(
            IList<PointContainer> data,
            double epsilon,
            int minimumPointsPerCluster)
        {
            List<PointInfo> pointInfos = data
                .Select(p => new PointInfo(p))
                .ToList();

            return CalculateClusters(
                new ListSpatialIndex(pointInfos),
                epsilon,
                minimumPointsPerCluster
            );
        }

        public static ClusterSet CalculateClusters(
            ListSpatialIndex index,
            double epsilon,
            int minimumPointsPerCluster)
        {
            List<PointInfo> points = index.Search().ToList();

            List<Cluster> clusters = new List<Cluster>();

            foreach (PointInfo p in points)
            {
                if (p.Visited) continue;

                p.Visited = true;
                IReadOnlyList<PointInfo> candidates = index.Search(p.Point, epsilon);

                if (candidates.Count >= minimumPointsPerCluster)
                    clusters.Add(
                        BuildCluster(
                            index,
                            p,
                            candidates,
                            epsilon,
                            minimumPointsPerCluster));
            }

            return new ClusterSet
            {
                Clusters = clusters,
                UnclusteredObjects = points
                    .Where(p => p.Cluster == null)
                    .Select(p => p.Item)
                    .ToList()
            };
        }

        private static Cluster BuildCluster(ListSpatialIndex index, PointInfo point,
            IReadOnlyList<PointInfo> neighborhood,
            double epsilon, int minimumPointsPerCluster)
        {
            List<PointContainer> points = new List<PointContainer> { point.Item };
            Cluster cluster = new Cluster { Points = points };
            point.Cluster = cluster;

            Queue<PointInfo> queue = new Queue<PointInfo>(neighborhood);
            while (queue.Any())
            {
                PointInfo newPoint = queue.Dequeue();
                if (!newPoint.Visited)
                {
                    newPoint.Visited = true;
                    IReadOnlyList<PointInfo> newNeighbors = index.Search(newPoint.Point, epsilon);
                    if (newNeighbors.Count >= minimumPointsPerCluster)
                        foreach (PointInfo p in newNeighbors)
                            queue.Enqueue(p);
                }

                if (newPoint.Cluster == null)
                {
                    newPoint.Cluster = cluster;
                    points.Add(newPoint.Item);
                }
            }

            return cluster;
        }
    }
}