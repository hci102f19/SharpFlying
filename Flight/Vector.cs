using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight
{
    public class Vector
    {
        public int Flag { get; set; }
        public int Roll { get; set; }
        public int Pitch { get; set; }
        public int Yaw { get; set; }
        public int Gaz { get; set; }

        /// <summary>
        /// Initializes a flightvector for the drone
        /// </summary>
        /// <param name="flag">1 if the roll and pitch values should be taken in consideration. 0 otherwise</param>
        /// <param name="roll">roll angle percentage (from -100 to 100). Negative values go left, positive go right.</param>
        /// <param name="pitch">pitch angle percentage (from -100 to 100). Negative values go backward, positive go forward.</param>
        /// <param name="yaw">yaw speed percentage (calculated on the max rotation speed)(from -100 to 100). Negative values go left, positive go right.</param>
        /// <param name="gaz">gaz speed percentage (calculated on the max vertical speed)(from -100 to 100). Negative values go down, positive go up.</param>
        public Vector(int flag, int roll, int pitch, int yaw, int gaz)
        {
            Flag = flag;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Gaz = gaz;
        }

        public bool IsNull()
        {
            if (Flag == 0 && Roll == 0 && Pitch == 0 && Yaw == 0 && Gaz == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
