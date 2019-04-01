using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Emgu.CV;
using Emgu.CV.Structure;
using Geometry.Base;
using SDPoint = System.Drawing.Point;

namespace RenderGeometry
{
    public class RenderPoint : Point
    {
        public RenderPoint(V2d point) : base(point) { }

        public RenderPoint(double x, double y) : base(x, y) { }

        public SDPoint AsPoint()
        {
            return new SDPoint((int)X, (int)Y);
        }

        public void Render(Image<Bgr, byte> frame)
        {
            var r = new Random();
            var Color = new MCvScalar(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));

            CvInvoke.Circle(frame, AsPoint(), 2, Color, -1);
        }
    }
}