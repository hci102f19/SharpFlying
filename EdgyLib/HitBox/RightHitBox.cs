using Aardvark.Base;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    internal class RightHitBox : HitBox
    {
        public RightHitBox(int width, int height, int? force = null, double widthTop = 0.97, double widthBottom = 0.6,
            double heightTop = 0.6) : base(force)
        {
            var width_top = width - (double) width / 2 * widthTop;
            var width_bottom = width - (double) width / 2 * widthBottom;
            var height_top = height * (1 - heightTop);

            InternalPolygon = new Polygon(
                new V2d(0, height),
                new V2d(0, height_top),
                new V2d(width_top, height_top),
                new V2d(width_top, height_top),
                new V2d(width_bottom, height),
                new V2d(0, height)
            );
        }
    }
}