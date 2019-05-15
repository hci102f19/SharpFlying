using System.Collections.Generic;
using EdgyLib.HitBox;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.Container
{
    public class BoxContainer
    {
        public BoxContainer(int width, int height)
        {
            HitBoxes = new List<IHit>
            {
                new ReverseHitBox(width, height)
            };
        }

        public List<IHit> HitBoxes { get; protected set; }

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
    }
}