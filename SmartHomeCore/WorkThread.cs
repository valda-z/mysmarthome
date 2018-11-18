using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartHomeCore
{
    class WorkThread : ThreadSimple
    {
        protected MySmartHomeAppConfig conf;

        protected JablotronDataList data;

        protected JaLib.JaInterface jab = new JaLib.JaInterface();

        public WorkThread()
        {
            conf = MySmartHomeAppConfig.ReadFromFile();
            data = JablotronDataList.ReadFromFile();
            jab.AlarmStateChanged += Jab_AlarmStateChanged;
            jab.AlarmDataChanged += Jab_AlarmDataChanged;
        }

        private void Jab_AlarmDataChanged(JaLib.JaInterface sender)
        {
            StaticJablotronState.IsMovementThere = true;
        }

        private void Jab_AlarmStateChanged(JaLib.JaInterface sender, JaLib.AlarmState state, byte deviceId)
        {
            BreakDynamicSleep = true;
        }

        public bool BreakDynamicSleep { get; set; }

        private void dynamicSleep()
        {
            int inter = conf.SendInterval * 10;
            for(int i = 0; i < inter; i++)
            {
                Thread.Sleep(100);
                if (BreakDynamicSleep)
                {
                    BreakDynamicSleep = false;
                    break;
                }
            }
        }

        protected override void Run(object obj)
        {
            for (;;)
            {
                if (!isRunning)
                {
                    break;
                }

                var homeData = new JablotronData();
                // each X sec
                dynamicSleep();

                // collect telemetry data
                try
                {
                    //is port open?
                    if (!jab.IsOpen || !jab.jablotronAlive)
                    {
                        Console.WriteLine("opening com port: " + conf.JablotronTTY);
                        jab.Open(conf.JablotronTTY);
                        jab.PIN = conf.PIN;
                    }

                    homeData.timestamp = DateTime.Now;
                    homeData.armedzone = jab.Zones.ToString();
                    homeData.commandexecuted = "";
                    homeData.connected = jab.IsOpen;
                    homeData.deviceid = jab.DeviceId;
                    homeData.led_a = jab.LED_A;
                    homeData.led_b = jab.LED_B;
                    homeData.led_backlight = jab.LED_Backlight;
                    homeData.led_c = jab.LED_C;
                    homeData.led_warning = jab.LED_Warning;
                    homeData.state = jab.State.ToString();
                    homeData.isalive = jab.jablotronAlive;

                    data.Add(homeData);

                    // send to server and get response (new config)
                    try
                    {
                        var resp = sendData();
                        data.Reset();
                        Console.WriteLine("sendData() .. ok");
                        var cmd = resp.commandtoexecute;
                        if (resp.commandtoexecute != "")
                        {
                            // process command from backend
                            switch (cmd)
                            {
                                case "ARM":
                                    if(jab.State== JaLib.AlarmState.IDLE)
                                    {
                                        jab.SetArmed();
                                    }
                                    break;
                                case "DISARM":
                                    if (jab.State != JaLib.AlarmState.IDLE)
                                    {
                                        jab.SetDisArmed();
                                    }
                                    break;
                                case "ARM_A":
                                    jab.SetArmA();
                                    break;
                                case "ARM_B":
                                    jab.SetArmB();
                                    break;
                                case "ARM_ABC":
                                    jab.SetArmABC();
                                    break;
                            }
                            Thread.Sleep(500);
                            homeData.timestamp = DateTime.Now;
                            homeData.armedzone = jab.Zones.ToString();
                            homeData.commandexecuted = cmd;
                            homeData.connected = jab.IsOpen;
                            homeData.deviceid = jab.DeviceId;
                            homeData.led_a = jab.LED_A;
                            homeData.led_b = jab.LED_B;
                            homeData.led_backlight = jab.LED_Backlight;
                            homeData.led_c = jab.LED_C;
                            homeData.led_warning = jab.LED_Warning;
                            homeData.state = jab.State.ToString();
                            homeData.isalive = jab.jablotronAlive;
                            data.Add(homeData);
                            resp = sendData();
                            data.Reset();
                            Console.WriteLine("sendData() .. ok - command [" + cmd + "] executed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("sendData() error: {0}", ex.Message);
                        // there is exception, we have to persist change
                        data.Save();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("sendData() error: {0}", ex.Message);
                }

            }
        }

        protected JablotronResponse sendData()
        {
            var client = new RestClient(conf.MySmartHomeURL
                    + "/api/Jablotron/Log?id="
                    + conf.DeviceId,
                    HttpVerb.POST,
                    JsonConvert.SerializeObject(data));

            return JsonConvert.DeserializeObject<JablotronResponse>(client.MakeRequest());
        }
    }
}
