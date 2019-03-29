using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            HitBoxes = new List<IHit>()
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
                if (hitBox.Hit(point, vector))
                    return vector;

            return vector;
        }
    }
}