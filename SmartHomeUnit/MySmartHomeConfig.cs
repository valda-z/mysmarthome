using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMartHomeUnit
{
    public class MySmartHomeConfig: MySmartHomeBase
    {
        private static string fName = "./conf.json";

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


        private MySmartHomeConfig()
        {
            dogontemp = 5.0;
            water = new MySmartHomeConfigWaterHardOn();
            fileName = fName;
            wateritems = new MySmartHomeConfigWaterItem[0];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(dogontemp);
            sb.Append("|");
            if (water != null)
            {
                sb.Append(water.from.ToString("yyyyMMddHHmmss") + ";" + water.to.ToString("yyyyMMddHHmmss"));
            }
            sb.Append("|");
            foreach (MySmartHomeConfigWaterItem itm in wateritems)
            {
                sb.Append(itm.starthour);
                sb.Append("|");
                sb.Append(itm.intervalsec);
                sb.Append("|");
            }
            return sb.ToString();
        }

        public double dogontemp { get; set; }
        public bool christmasOn { get; set; }
        public MySmartHomeConfigWaterHardOn water { get; set; }
        public MySmartHomeConfigWaterItem[] wateritems { get; set; }

        public static MySmartHomeConfig Create(string data)
        {
            if (data == null || data.Length == 0)
            {
                return new MySmartHomeConfig();
            }
            else
            {
                return JsonConvert.DeserializeObject<MySmartHomeConfig>(data);
            }
        }

        public bool Save(MySmartHomeConfig obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if(this.ToString() != obj.ToString())
                {
                    // save only in case that we have new config data ...
                    obj.StoreData();
                    // copy object
                    this.wateritems = obj.wateritems;
                    this.water = obj.water;
                    this.dogontemp = obj.dogontemp;
                    return true;
                }
                return false;
            }
        }

        public static MySmartHomeConfig ReadFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<MySmartHomeConfig>(
                    File.ReadAllText(fName));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadFromFile() error: {0}", ex.Message);

                return new MySmartHomeConfig();
            }
        }

    }
}
