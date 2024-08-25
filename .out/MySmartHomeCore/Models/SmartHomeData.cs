using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySmartHomeCore.Models
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

    public class SmartHomeDataList
    {
        public SmartHomeData[] data { get; set; }

        public static SmartHomeDataList Create(string data)
        {
            if (data == null || data.Length == 0)
            {
                return new SmartHomeDataList();
            }
            else
            {
                return JsonConvert.DeserializeObject<SmartHomeDataList>(data);
            }
        }
    }
}