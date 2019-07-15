using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SMartHomeUnit
{
    public static class PiOverlay
    {
        public static double GetTemp()
        {
            try
            {
                string test = File.ReadAllText("./devices/ds18b20/w1_slave");
                var regex = new Regex(".*crc=.*YES\\s*.* t=(?<temp>(-*\\d+)?)", RegexOptions.Multiline);
                var match = regex.Match(test);
                if (match.Success)
                {
                    return double.Parse(match.Groups["temp"].Value) / 1000.0;
                }
                else
                {
                    return 99.9;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetTemp() error: {0}", ex.Message);
                return 99.9;
            }
        }

        private static int waterSensor = 0;

        public static bool GetWaterSensor()
        {
            try
            {
                string test = File.ReadAllText("./devices/watersensor/value");
                if(test.Length>0 && test.StartsWith("1"))
                {
                    // is not wet
                    waterSensor = 6;
                }
                else
                {
                    // is wet
                    if (waterSensor > 0)
                    {
                        waterSensor--;
                    }
                }

                // return true if is wet
                return waterSensor == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetWaterSensor() error: {0}", ex.Message);
                return false;
            }
        }
        public static void SetWaterOn(bool on)
        {
            try
            {
                string txt = (on ? "1" : "0") + "\n";
                File.WriteAllText("./devices/wateron/value", txt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetWaterOn() error: {0}", ex.Message);
            }
        }
        public static bool GetWaterOn()
        {
            try
            {
                string test = File.ReadAllText("./devices/wateron/value");
                if (test.Length > 0 && test.StartsWith("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetWaterOn() error: {0}", ex.Message);
                return false;
            }
        }
    }
}
