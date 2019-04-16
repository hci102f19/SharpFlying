using System;

namespace FlightLib
{
    public class Vector
    {
        /// <summary>
        ///     Initializes a flightvector for the drone
        /// </summary>
        /// <param name="flag">1 if the roll and pitch values should be taken in consideration. 0 otherwise</param>
        /// <param name="roll">roll angle percentage (from -100 to 100). Negative values go left, positive go right.</param>
        /// <param name="pitch">pitch angle percentage (from -100 to 100). Negative values go backward, positive go forward.</param>
        /// <param name="yaw">
        ///     yaw speed percentage (calculated on the max rotation speed)(from -100 to 100). Negative values go
        ///     left, positive go right.
        /// </param>
        /// <param name="gaz">
        ///     gaz speed percentage (calculated on the max vertical speed)(from -100 to 100). Negative values go
        ///     down, positive go up.
        /// </param>
        public Vector(int flag = 0, int roll = 0, int pitch = 0, int yaw = 0, int gaz = 0)
        {
            Flag = flag;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Gaz = gaz;
        }

        /// <summary>
        ///     Default 'null' constructor
        /// </summary>
        public Vector()
        {
        }

        public int Flag { get; set; }
        public int Roll { get; set; }
        public int Pitch { get; set; }
        public int Yaw { get; set; }
        public int Gaz { get; set; }

        /// <summary>
        ///     Returns whether or not a vector is null
        /// </summary>
        /// <returns>true if null, false otherwise</returns>
        public bool IsNull()
        {
            return Flag == 0 && Roll == 0 && Pitch == 0 && Yaw == 0 && Gaz == 0;
        }

        public override string ToString()
        {
            return "Flag: " + Flag + ", Roll: " + Roll + ", Pitch: " + Pitch + ", Yaw: " + Yaw + ", Gaz: " + Gaz;
        }

        public Vector Add(Vector vector)
        {
            Roll += vector.Roll;
            Pitch += vector.Pitch;
            Yaw += vector.Yaw;
            Gaz += vector.Gaz;

            return this;
        }

        public Vector TimesConstant(int constant)
        {
            Roll *= constant;
            Pitch *= constant;
            Yaw *= constant;
            Gaz *= constant;

            return this;
        }

        public Vector TimesConstant(float constant)
        {
            Roll = (int)Math.Round(Roll * constant, 0);
            Pitch = (int)Math.Round(Pitch * constant, 0);
            Yaw = (int)Math.Round(Yaw * constant, 0);
            Gaz = (int)Math.Round(Gaz * constant,0);

            return this;
        }
    }
}