using System;
using System.Collections.Generic;
using System.Text;

namespace SMartHomeUnit
{
    public class SmartHomeData
    {
        public double outsidetemp { get; set; }
        public double doghousetemp { get; set; }
        public bool doghouseheating { get; set; }
        public bool waterswitchon { get; set; }
        public bool iswet { get; set; }
        public DateTime timestamp { get; set; }
    }
}
