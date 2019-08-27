using Microsoft.AspNetCore.Mvc;
using MySmartHomeCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MySmartHomeCore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        [HttpPost("Log")]
        public JToken Log(string id, JToken data)
        {
            var gid = new Guid(id);
            var obj = SmartHomeDataList.Create(data.ToString());
            var cx = SmartHomeDBContext.Create();
            // get device detail, if there is no device we will throw exception
            var device = cx.Devices.Single(e => e.DeviceId == gid);
            for(int i = 0; i < obj.data.Length; i++)
            {
                var itm = obj.data[i];
                var tmstmp = new DateTime(itm.timestamp.Year, itm.timestamp.Month, itm.timestamp.Day, itm.timestamp.Hour, (itm.timestamp.Minute / 10) * 10, 0);
                var curr = cx.DeviceLogs.SingleOrDefault(e => e.DeviceId == gid && e.Created == tmstmp);
                if (curr == null)
                {
                    var d = new DeviceLog();
                    d.Created = tmstmp;
                    d.DeviceId = gid;
                    d.DogHouseHeatingOn = itm.doghouseheating;
                    d.DogHouseTemperature = (decimal)itm.doghousetemp;
                    d.Temperature = (decimal)itm.outsidetemp;
                    d.WaterOn = itm.waterswitchon;
                    d.IsWet = itm.iswet;
                    cx.DeviceLogs.Add(d);
                }
                else
                {
                    var d = curr;
                    if (itm.doghouseheating) { d.DogHouseHeatingOn = itm.doghouseheating; }
                    d.DogHouseTemperature = (d.DogHouseTemperature * 9 + (decimal)itm.doghousetemp) / 10;
                    d.Temperature = (d.Temperature * 9 + (decimal)itm.outsidetemp) / 10;
                    if (itm.waterswitchon) { d.WaterOn = itm.waterswitchon; }
                    if (itm.iswet) { d.IsWet = itm.iswet; }
                }
                if (i == (obj.data.Length - 1))
                {
                    device.Contacted = itm.timestamp;
                    device.DogHouseHeatingOn = itm.doghouseheating;
                    device.DogHouseTemperature = (decimal)itm.doghousetemp;
                    device.Temperature = (decimal)itm.outsidetemp;
                    device.WaterOn = itm.waterswitchon;
                    device.IsWet = itm.iswet;
                }
                cx.SaveChanges();
            }
            var conf = SmartHomeConfig.Deserialize(device.Config);
            if (!conf.irrigationOn)
            {
                conf.ClearWaterItems();
            }
            return JObject.Parse(conf.Serialize());
        }
    }
}
