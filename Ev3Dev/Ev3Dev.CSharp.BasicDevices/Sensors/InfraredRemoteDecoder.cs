using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.BasicDevices.Sensors
{
    public static class InfraredRemoteDecoder
    {
        public static bool BlueDownPressed( this InfraredSensor sensor )
        {
            return sensor.Mode == InfraredSensorMode.IrRemoteControlAlternative
                   && ( sensor.GetValue( ) & 0x80 ) != 0;
        }

        public static bool BlueUpPressed( this InfraredSensor sensor )
        {
            return sensor.Mode == InfraredSensorMode.IrRemoteControlAlternative
                   && ( sensor.GetValue( ) & 0x40 ) != 0;
        }

        public static bool RedDownPressed( this InfraredSensor sensor )
        {
            return sensor.Mode == InfraredSensorMode.IrRemoteControlAlternative
                   && ( sensor.GetValue( ) & 0x20 ) != 0;
        }

        public static bool RedUpPressed( this InfraredSensor sensor )
        {
            return sensor.Mode == InfraredSensorMode.IrRemoteControlAlternative
                   && ( sensor.GetValue( ) & 0x10 ) != 0;
        }
    }
}
