using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    public class EventListEntry
    {
        public class ConfigItems
        {
            public class ConfigItem
            {
                public int key;
                public string value;
            }
            public ConfigItem[] items;
        }

        public string Icon { get; set; }
        public string Row1 { get; set; }
        public string Row2 { get; set; }

        public static EventListEntry Deserialize(string data)
        {
            if (data == null || data.Length == 0)
            {
                return new EventListEntry();
            }
            else
            {
                return JsonConvert.DeserializeObject<EventListEntry>(data);
            }
        }
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        private static ConfigItems convData = null;

        public static string DecodeDevice(byte i)
        {
            if (convData == null)
            {
                convData = JsonConvert.DeserializeObject<ConfigItems>(ConfigurationManager.AppSettings["JABLOTRONZONES"]);
            }

            string ret = "";

            foreach(ConfigItems.ConfigItem x in convData.items)
            {
                if (x.key == i)
                {
                    ret = x.value;
                    break;
                }
            }

            return ret;
        }
    }
}