using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MySmartHomeCore.Models
{
    public class SmartHomeDBContext
    {
        private static SmartHomeDBContext ctx = new SmartHomeDBContext();
        private SmartHomeDBContext()
        {
            this.Devices = new List<Device>();
            this.DeviceLogs = new List<DeviceLog>();
            this.EventLists = new List<EventList>();
            this.Jablotrons = new List<Jablotron>();
        }

        private string getHash(string text)
        {
            var enc = Encoding.GetEncoding(0);
            byte[] buffer = enc.GetBytes(text);
            var sha1 = SHA1.Create();
            var hash = BitConverter.ToString(sha1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        private string hashDevices = "";
        private string hashDeviceLogs = "";
        private string hashEventLists = "";
        private string hashJbalotrons = "";

        public void TestInit()
        {
            if (Devices.Count == 0)
            {
                var u1 = new Device();
                u1.Config = "{\"dogontemp\":5.0,\"irrigationOn\":false,\"water\":{\"from\":\"0001-01-01T00:00:00\",\"to\":\"0001-01-01T00:00:00\"},\"wateritems\":[{\"starthour\":\"08:00\",\"intervalsec\":600},{\"starthour\":\"20:00\",\"intervalsec\":420}]}";
                u1.Contacted = DateTime.Now;
                u1.DeviceId = new Guid(EnvSettings.UNIT1);
                u1.DogHouseHeatingOn = false;
                u1.DogHouseTemperature = 10;
                u1.IsWet = false;
                u1.Note = "";
                u1.Temperature = 11;
                u1.WaterOn = false;

                var u2 = new Device();
                u2.Config = "";
                u2.Contacted = DateTime.Now;
                u2.DeviceId = new Guid(EnvSettings.JABLOTRON);
                u2.DogHouseHeatingOn = false;
                u2.DogHouseTemperature = 10;
                u2.IsWet = false;
                u2.Note = "";
                u2.Temperature = 11;
                u2.WaterOn = false;

                Devices.Add(u1);
                Devices.Add(u2);
            }

            if (Jablotrons.Count == 0)
            {

                var j = new Jablotron();
                j.CommandToExecute = "";
                j.Contacted = DateTime.Now;
                j.DeviceId = new Guid(EnvSettings.JABLOTRON);
                j.State = "IDLE";
                j.Note = "";

                Jablotrons.Add(j);
            }
        }

        public void SaveChanges()
        {
            hashDevices = saveData(hashDevices, JsonConvert.SerializeObject(Devices), "devices");
            hashDeviceLogs = saveData(hashDeviceLogs, JsonConvert.SerializeObject(DeviceLogs), "devicelogs");
            hashEventLists = saveData(hashEventLists, JsonConvert.SerializeObject(EventLists), "eventlists");
            hashJbalotrons = saveData(hashJbalotrons, JsonConvert.SerializeObject(Jablotrons), "jablotrons");
        }

        public string saveData(string origHash, string data, string obj)
        {
            string tmpHash = getHash(data);
            if (tmpHash != origHash)
            {
                File.WriteAllText(EnvSettings.WORKDIR+"/__"+obj+".json", data);
            }
            return tmpHash;
        }

        public void Init()
        {
            try
            {
                Devices = JsonConvert.DeserializeObject<List<Device>>(loadData(out hashDevices, "devices"));
            }
            catch { }
            try
            {
                DeviceLogs = JsonConvert.DeserializeObject<List<DeviceLog>>(loadData(out hashDeviceLogs, "devicelogs"));
            }
            catch { }
            try
            {
                EventLists = JsonConvert.DeserializeObject<List<EventList>>(loadData(out hashEventLists, "eventlists"));
            }
            catch { }
            try
            {
                Jablotrons = JsonConvert.DeserializeObject<List<Jablotron>>(loadData(out hashJbalotrons, "jablotrons"));
            }
            catch { }
        }

        private string loadData(out string origHash, string obj)
        {
            string data = File.ReadAllText(EnvSettings.WORKDIR + "/__" + obj + ".json");
            origHash = getHash(data);
            return data;
        }

        public static SmartHomeDBContext Create()
        {
            return ctx;
        }

        public List<Device> Devices { get; set; }
        public List<DeviceLog> DeviceLogs { get; set; }
        public List<Jablotron> Jablotrons { get; set; }
        public List<EventList> EventLists { get; set; }
    }
}