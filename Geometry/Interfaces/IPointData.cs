using Geometry.Base;

namespace Geometry.Interfaces
{
    public interface IPointData
    {
        ref readonly Point Point { get; }
        double X { get; }
        double Y { get; }
    }
}