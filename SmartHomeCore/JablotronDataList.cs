using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SmartHomeCore
{
    public class JablotronDataList : MySmartHomeBase
    {
        private static string fName = "./data.json";
        private bool persis = true;
        public JablotronData[] data { get; set; }

        public JablotronDataList()
        {
            fileName = fName;
            data = new JablotronData[0];
        }

        public void Add(JablotronData obj)
        {
            Console.WriteLine(JsonConvert.SerializeObject(obj));
            var newarr = new JablotronData[data.Length + 1];
            for(int i = 0; i < data.Length; i++)
            {
                newarr[i] = data[i];
            }
            newarr[data.Length] = obj;
            data = newarr;
        }

        public void Reset()
        {
            data = new JablotronData[0];
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

        public static JablotronDataList ReadFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<JablotronDataList>(
                    File.ReadAllText(fName));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadFromFile() error: {0}", ex.Message);

                return new JablotronDataList();
            }
        }
    }
}
