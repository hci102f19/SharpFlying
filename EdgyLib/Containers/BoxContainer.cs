using System.Collections.Generic;
using System.Xml.Serialization;
using EdgyLib.HitBox;
using Emgu.CV;
using Emgu.CV.Structure;
using FlightLib;
using Geometry.Base;
using RenderGeometry.Base;

namespace EdgyLib.Containers
{
    public class BoxContainer
    {
        protected List<IHit> HitBoxes;

        public BoxContainer(int width, int height)
        {
            HitBoxes = new List<IHit>
            {
                new LeftHitBox(width, height),
                new RightHitBox(width, height),
                new ReverseHitBox(width, height)
            };
        }

        public Vector Hit(Point point)
        {
            var vector = new Vector();

            foreach (var hitBox in HitBoxes)
                if (hitBox.Hit(point, vector))
                    return vector;

            return vector;
        }

        public void Render(Image<Bgr, byte> frame)
        {
            foreach (var hitBox in HitBoxes)
                hitBox.Render(frame);
        }
    }
}