using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeJablotron
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(">>> Starting MySmartHome agent ...");

            Console.WriteLine("    Communication URL: " + MySmartHomeAppConfig.ReadFromFile().MySmartHomeURL);
            Console.WriteLine("    Device ID: " + MySmartHomeAppConfig.ReadFromFile().DeviceId);

            var thr = new WorkThread();
            thr.Start();

            var http = new MyHttpServerHeatingControler(11080);

            while (true)
            {
                Thread.Sleep(100);
            }

            thr.Stop();
        }
    }
}
