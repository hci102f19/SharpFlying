using Aardvark.Base;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    internal class LeftHitBox : HitBox
    {
        public LeftHitBox(int width, int height, int? force = null, double widthTop = 0.97, double widthBottom = 0.6,
            double heightTop = 0.6) : base(force)
        {
            double width_top = (double) width / 2 * widthTop;
            double width_bottom = (double) width / 2 * widthBottom;
            double height_top = height * (1 - heightTop);

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