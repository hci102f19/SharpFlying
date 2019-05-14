using Emgu.CV;
using Emgu.CV.Structure;
using Geometry.Base;
using SDPoint = System.Drawing.Point;

namespace RenderGeometry.Base
{
    public class RenderLine
    {
        protected Line Line;

        public RenderLine(Line line)
        {
            Line = line;
        }

        public SDPoint StartPoint()
        {
            return new SDPoint((int)Line.StartPoint.X, (int)Line.StartPoint.Y);
        }

        public SDPoint EndPoint()
        {
            return new SDPoint((int)Line.EndPoint.X, (int)Line.EndPoint.Y);
        }

        public void Render(Image<Bgr, byte> frame, MCvScalar color = default(MCvScalar))
        {
            CvInvoke.Line(frame, StartPoint(), EndPoint(), color, 2);
        }

        public static explicit operator RenderLine(Line v)
        {
            return new RenderLine(v);
        }
    }
}