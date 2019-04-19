using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopFlying.Bebop_Classes
{
    public class BebopSensors
    {
        public BebopSensors()
        {
            //Initialize sensor dict
            SensorDictionary = new Dictionary<string, bool>();

            //Default to 100 battery
            Battery = 100;
        }
        //Actual sensor data
        public Dictionary<string,bool> SensorDictionary { get; set; }
        public string FlyState { get; set; }
        public int Battery { get; set; }

        public bool RelativeMoveEnded { get; set; }
        public bool CameraMoveEnded_tilt { get; set; }
        public bool CameraMoveEnded_pan { get; set; }

        public bool flat_trim_changed { get; set; }

        public bool max_altitude_changed { get; set; }

        public bool max_distance_changed { get; set; }

        public bool no_fly_over_max_distance { get; set; }

        public bool max_tilt_changed { get; set; }

        public bool max_pitch_roll_rotation_speed_changed { get; set; }

        public bool max_vertical_speed { get; set; }

        public bool max_rotation_speed { get; set; }

        public bool hull_protection_changed { get; set; }

        public bool outdoor_mode_changed { get; set; }

        public bool picture_format_changed { get; set; }

        public bool auto_white_balance_changed { get; set; }

        public bool exposition_changed { get; set; }

        public bool saturation_changed { get; set; }

        public bool timelapse_changed { get; set; }

        public bool video_stabilization_changed { get; set; }

        public bool video_recording_changed { get; set; }

        public bool video_framerate_changed { get; set; }
        public bool video_resolutions_changed { get; set; }
    }
}
