using System.Collections.Generic;
using EdgyLib.HitBox;
using FlightLib;
using Geometry.Base;

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
    }
}