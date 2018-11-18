using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHomeCore
{
    public class JablotronData
    {
        public DateTime timestamp { get; set; }
        public string state { get; set; }
        public string armedzone { get; set; }
        public byte deviceid { get; set; }
        public bool led_a { get; set; }
        public bool led_b { get; set; }
        public bool led_c { get; set; }
        public bool led_warning { get; set; }
        public bool led_backlight { get; set; }
        public bool connected { get; set; }
        public bool isalive { get; set; }
        public string commandexecuted { get; set; }
    }
}
