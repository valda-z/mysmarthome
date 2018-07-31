using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    public class SmartHomeConfig
    {
        public class MySmartHomeConfigWaterItem
        {
            public string starthour { get; set; }
            public int intervalsec { get; set; }
        }

        public class MySmartHomeConfigWaterHardOn
        {
            public DateTime from { get; set; }
            public DateTime to { get; set; }
        }

        public class MySmartHomeHeatingItem
        {
            public int h { get; set; }
            public DayOfWeek d { get; set; }
            public decimal t { get; set; }
        }

        private SmartHomeConfig()
        {
            dogontemp = 5.0;
            water = new MySmartHomeConfigWaterHardOn();
            irrigationOn = true;
            christmasOn = false;
            wateritems = new MySmartHomeConfigWaterItem[0];
        }

        public double dogontemp { get; set; }
        public bool irrigationOn { get; set; }
        public bool christmasOn { get; set; }
        public MySmartHomeConfigWaterHardOn water { get; set; }
        public MySmartHomeConfigWaterItem[] wateritems { get; set; }
        public MySmartHomeHeatingItem[] heatingitems { get; set; }
        public bool homeheatingoutofhome { get; set; }
        public decimal homeheatingoutofhometemp { get; set; }

        public static SmartHomeConfig Deserialize(string data)
        {
            if (data == null || data.Length == 0)
            {
                return new SmartHomeConfig();
            }
            else
            {
                return JsonConvert.DeserializeObject<SmartHomeConfig>(data);
            }
        }

        public void ClearWaterItems()
        {
            wateritems = new MySmartHomeConfigWaterItem[0];
        }

        public void AddWaterItem(MySmartHomeConfigWaterItem obj)
        {
            var newarr = new MySmartHomeConfigWaterItem[wateritems.Length + 1];
            for (int i = 0; i < wateritems.Length; i++)
            {
                newarr[i] = wateritems[i];
            }
            newarr[wateritems.Length] = obj;
            wateritems = newarr;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}