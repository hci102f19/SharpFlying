using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightLib;
using Geometry.Base;

namespace EdgyLib.HitBox
{
    public interface IHit
    {
        bool Hit(Point point, Vector vector);
    }
}
