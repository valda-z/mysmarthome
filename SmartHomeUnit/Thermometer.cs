using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMartHomeUnit
{
    public static class Thermometer
    {
        public static double GetTemp()
        {
            return PiOverlay.GetTemp();
        }
    }
}
