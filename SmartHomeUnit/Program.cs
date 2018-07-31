using System;
using System.Reflection;
using System.Threading;

namespace SMartHomeUnit
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(">>> Starting MySmartHome agent ...");
//            Console.WriteLine("    version: " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

            Console.WriteLine("    Communication URL: " + MySmartHomeAppConfig.ReadFromFile().MySmartHomeURL);
            Console.WriteLine("    Device ID: " + MySmartHomeAppConfig.ReadFromFile().DeviceId);

            var thr = new WorkThread();
            thr.Start();


            while (true)
            {
                Thread.Sleep(100);
            }

            thr.Stop();
        }
    }
}