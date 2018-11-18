using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SmartHomeCore
{
    public class MySmartHomeBase
    {
        protected string fileName;

        public void StoreData()
        {
            try
            {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(this));
            }
            catch (Exception ex)
            {
                Console.WriteLine("StoreData() error: {0}", ex.Message);
            }
        }
    }
}
