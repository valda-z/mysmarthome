using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SMartHomeUnit
{
    class WorkThread : ThreadSimple
    {
        protected MySmartHomeAppConfig conf;
        protected MySmartHomeConfig homeConf;

        protected SmartHomeDataList data;

        protected DogHomeProcess doghouseProc;
        protected WaterProcess waterProc;

        public WorkThread()
        {
            conf = MySmartHomeAppConfig.ReadFromFile();
            homeConf = MySmartHomeConfig.ReadFromFile();
            waterProc = new WaterProcess(homeConf);
            doghouseProc = new DogHomeProcess(homeConf, conf.DogHouseTTY);
            data = SmartHomeDataList.ReadFromFile();
        }

        protected override void Run(object obj)
        {
            for (;;)
            {
                if (!isRunning)
                {
                    break;
                }

                var homeData = new SmartHomeData();
                // each X sec
                Thread.Sleep(conf.SendInterval * 1000);

                // collect telemetry data
                waterProc.CollectData();
                doghouseProc.CollectData();

                homeData.timestamp = DateTime.Now;
                homeData.waterswitchon = waterProc.IsWaterOn();
                homeData.iswet = waterProc.IsWet();
                homeData.doghouseheating = doghouseProc.Heating;
                homeData.doghousetemp = doghouseProc.Temp;
                homeData.outsidetemp = Thermometer.GetTemp();

                data.Add(homeData);

                // send to server and get response (new config)
                try
                {
                    var newConf = sendData();
                    homeConf.Save(newConf);
                    data.Reset();
                    Console.WriteLine("sendData() .. ok");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("sendData() error: {0}", ex.Message);
                    // there is exception, we have to persist change
                    data.Save();
                }

                // process water switch on/off
                waterProc.Process();
            }
        }

        protected MySmartHomeConfig sendData()
        {
            var client = new RestClient(conf.MySmartHomeURL
                    + "/api/Device/Log?id="
                    + conf.DeviceId,
                    HttpVerb.POST,
                    JsonConvert.SerializeObject(data));

            return JsonConvert.DeserializeObject<MySmartHomeConfig>(client.MakeRequest());
        }
    }
}
