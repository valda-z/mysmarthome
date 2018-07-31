using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMartHomeUnit
{
    class SmartHomeDataList: MySmartHomeBase
    {
        private static string fName = "./data.json";
        private bool persis = true;
        public SmartHomeData[] data { get; set; }

        public SmartHomeDataList()
        {
            fileName = fName;
            data = new SmartHomeData[0];
        }

        public void Add(SmartHomeData obj)
        {
            var newarr = new SmartHomeData[data.Length + 1];
            for(int i = 0; i < data.Length; i++)
            {
                newarr[i] = data[i];
            }
            newarr[data.Length] = obj;
            data = newarr;
        }

        public void Reset()
        {
            data = new SmartHomeData[0];
            if (persis)
            {
                persis = false;
                this.StoreData();
            }
        }

        public void Save()
        {
            persis = true;
            this.StoreData();
        }

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

        public static SmartHomeDataList ReadFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<SmartHomeDataList>(
                    File.ReadAllText(fName));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadFromFile() error: {0}", ex.Message);

                return new SmartHomeDataList();
            }
        }
    }
}
