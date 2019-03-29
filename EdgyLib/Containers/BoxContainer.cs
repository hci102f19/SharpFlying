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
        List<IHit> HitBoxes = new List<IHit>()
        {
            new LeftHitBox(604, 480),
            new RightHitBox(604, 480),
            new ReverseHitBox(604, 480)
        };

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