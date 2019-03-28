using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry.Base;

namespace Geometry.Extended
{
    public class PointContainer
    {
        private readonly Point _point;

        public PointContainer(Point point) => _point = point;

        public ref readonly Point Point => ref _point;

        public double X => _point.X;
        public double Y => _point.Y;
    }
}