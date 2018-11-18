using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SmartHomeCore
{
    public class MySmartHomeAppConfig: MySmartHomeBase
    {
        private static string fName = "./appsettings.json";
        private MySmartHomeAppConfig()
        {
            fileName = fName;
        }

        public string MySmartHomeURL { get; set; }
        public string DeviceId { get; set; }
        public string OutDeviceId { get; set; }
        public int SendInterval { get; set; }
        public string PIN { get; set; }
        public string JablotronTTY { get; set; }

        public static MySmartHomeAppConfig ReadFromFile()
        {
            try
            {
                return JsonConvert.DeserializeObject<MySmartHomeAppConfig>(
                    File.ReadAllText(fName));
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadFromFile() error: {0}", ex.Message);

                return new MySmartHomeAppConfig();
            }
        }

    }
}
