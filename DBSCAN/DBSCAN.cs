using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geometry.Extended;

namespace DBSCAN
{
    public static class DBSCAN
    {
        public static ClusterSet CalculateClusters(
            IList<PointContainer> data,
            double epsilon,
            int minimumPointsPerCluster)
        {
            var pointInfos = data
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
            var points = index.Search().ToList();

            var clusters = new List<Cluster>();

            foreach (var p in points)
            {
                if (p.Visited) continue;

                p.Visited = true;
                var candidates = index.Search(p.Point, epsilon);

                if (candidates.Count >= minimumPointsPerCluster)
                {
                    clusters.Add(
                        BuildCluster(
                            index,
                            p,
                            candidates,
                            epsilon,
                            minimumPointsPerCluster));
                }
            }

            return new ClusterSet
            {
                Clusters = clusters,
                UnclusteredObjects = points
                    .Where(p => p.Cluster == null)
                    .Select(p => p.Item)
                    .ToList(),
            };
        }

        private static Cluster BuildCluster(ListSpatialIndex index, PointInfo point, IReadOnlyList<PointInfo> neighborhood,
            double epsilon, int minimumPointsPerCluster)
        {
            var points = new List<PointContainer>() { point.Item };
            var cluster = new Cluster() { Points = points };
            point.Cluster = cluster;

            var queue = new Queue<PointInfo>(neighborhood);
            while (queue.Any())
            {
                var newPoint = queue.Dequeue();
                if (!newPoint.Visited)
                {
                    newPoint.Visited = true;
                    var newNeighbors = index.Search(newPoint.Point, epsilon);
                    if (newNeighbors.Count >= minimumPointsPerCluster)
                        foreach (var p in newNeighbors)
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