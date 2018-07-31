using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SMartHomeUnit
{
    class WaterProcess
    {
        private MySmartHomeConfig conf;

        private bool iswet = false;

        public WaterProcess(MySmartHomeConfig homeConf)
        {
            conf = homeConf;
            waterSwitch(false);
        }

        public void CollectData()
        {
            iswet = PiOverlay.GetWaterSensor();
        }

        public bool IsWaterOn()
        {
            return PiOverlay.GetWaterOn();
        }

        public bool IsWet()
        {
            return iswet;
        }

        public void Process()
        {
            var curr = DateTime.Now;
            string currHour = curr.ToString("HH:mm");
            bool toSwitchOn = false;

            if (conf.water != null
                && conf.water.from <= DateTime.Now
                && conf.water.to >= DateTime.Now)
            {
                toSwitchOn = true;
            }
            else
            {
                Console.WriteLine("  ... Conf christmasOn: {0}", conf.christmasOn);
                if (!iswet || IsWaterOn() || conf.christmasOn)
                {
                    foreach (var w in conf.wateritems)
                    {
                        string hTo = curr.AddSeconds(-1 * w.intervalsec).ToString("HH:mm");
                        Console.WriteLine("     ... curr/start/to: {0}/{1}/{2}", currHour, w.starthour, hTo);
                        if (currHour.CompareTo(w.starthour) >= 0 && hTo.CompareTo(w.starthour) <= 0)
                        {
                            toSwitchOn = true;
                            break;
                        }
                    }
                }
            }

            waterSwitch(toSwitchOn);
        }

        private void waterSwitch(bool on)
        {
            PiOverlay.SetWaterOn(on);
        }

    }
}
