using System;
using System.Threading;

namespace SmartHomeCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(">>> Starting MySmartHome CORE agent ...");

            Console.WriteLine("    Communication URL: " + MySmartHomeAppConfig.ReadFromFile().MySmartHomeURL);
            Console.WriteLine("    Device ID: " + MySmartHomeAppConfig.ReadFromFile().DeviceId);
            Console.WriteLine("    Device ID (out): " + MySmartHomeAppConfig.ReadFromFile().OutDeviceId);

            var thr = new WorkThread();
            thr.Start();

            var http = new MyHttpServerControler(11080);

            while (true)
            {
                Thread.Sleep(100);
            }

            //thr.Stop();
        }
    }
}
