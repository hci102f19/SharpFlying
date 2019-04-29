using System.Collections.Generic;
using EdgyLib.HitBox;
using Emgu.CV;
using Emgu.CV.Structure;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.Container
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
            Vector vector = new Vector();

            foreach (IHit hitBox in HitBoxes)
            {
                if (hitBox.Hit(point, vector))
                {
                    return vector;
                }
            }

            return vector;
        }

        public void Render(Image<Bgr, byte> frame)
        {
            foreach (IHit hitBox in HitBoxes)
            {
                hitBox.Render(frame);
            }
        }
    }
}