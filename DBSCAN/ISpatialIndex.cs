using System;
using System.Collections.Generic;
using System.Text;
using Geometry.Base;
using Geometry.Extended;

namespace DBSCAN
{
    public interface ISpatialIndex
    {
        IReadOnlyList<PointInfo> Search();
        IReadOnlyList<PointInfo> Search(in Point p, double epsilon);
    }
}