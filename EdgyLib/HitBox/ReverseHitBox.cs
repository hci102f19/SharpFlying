using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    class ReverseHitBox : IHit
    {
        public ReverseHitBox(int width, int height)
        {
        }

        public bool Hit(Point point, Vector vector)
        {
            throw new NotImplementedException();
        }
    }
}