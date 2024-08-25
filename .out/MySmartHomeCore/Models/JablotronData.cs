using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySmartHomeCore.Models
{
    public enum AlarmStateEx
    {
        IDLE,
        ARMED,
        ALARM
    }

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

        public bool IsEqual(JablotronData data)
        {
            return (
                this.state == data.state &&
                this.armedzone == data.armedzone &&
                this.led_a == data.led_a &&
                this.led_b == data.led_b &&
                this.led_c == data.led_c &&
                this.led_backlight == data.led_backlight &&
                this.connected == data.connected &&
                this.isalive == data.isalive &&
                this.commandexecuted == data.commandexecuted &&
                this.deviceid == deviceid
                );
        }

        public AlarmStateEx GetArmStateEx()
        {
            var ret = AlarmStateEx.IDLE;
            switch (this.state)
            {
                case "IDLE":
                    ret = AlarmStateEx.IDLE;
                    break;
                case "INCOMMINGDELAY":
                case "OUTCOMMINGDELAY":
                case "ARMED":
                    ret = AlarmStateEx.ARMED;
                    break;
                case "ALARM":
                    ret = AlarmStateEx.ALARM;
                    break;
            }
            return ret;
        }
    }

    public class JablotronDataList
    {
        public JablotronData[] data { get; set; }

        public static JablotronDataList Create(string data)
        {
            if (data == null || data.Length == 0)
            {
                return new JablotronDataList();
            }
            else
            {
                return JsonConvert.DeserializeObject<JablotronDataList>(data);
            }
        }
    }

}
