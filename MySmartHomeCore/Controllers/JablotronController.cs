using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySmartHomeCore;
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
    public class JablotronController : ControllerBase
    {
        private AppSettings AppSettings { get; set; }

        public JablotronController(IOptions<AppSettings> settings)
        {
            AppSettings = settings.Value;
        }

        [HttpPost("Log")]
        public JToken Log(string id, JToken data)
        {
            var gid = new Guid(id);
            var obj = JablotronDataList.Create(data.ToString());
            var cx = SmartHomeDBContext.Create();
            // get device detail, if there is no device we will throw exception
            var device = cx.Jablotrons.Single(e => e.DeviceId == gid);
            var prevData = new JablotronData();
            for (int i = 0; i < obj.data.Length; i++)
            {
                var itm = obj.data[i];
                string newData = JsonConvert.SerializeObject(itm);
                if (!prevData.IsEqual(itm))
                {
                    var newEntryObj = new EventListEntry();
                    newEntryObj.Icon = "ALARM-" + itm.GetArmStateEx().ToString();
                    switch (itm.GetArmStateEx())
                    {
                        case AlarmStateEx.IDLE:
                            newEntryObj.Row1 = "Alarm OFF";
                            newEntryObj.Row2 = "";
                            break;
                        case AlarmStateEx.ARMED:
                            newEntryObj.Row1 = "Alarm ARMED";
                            newEntryObj.Row2 = "Zones: " + itm.armedzone;
                            break;
                        case AlarmStateEx.ALARM:
                            newEntryObj.Row1 = "ALARM";
                            newEntryObj.Row2 = "Zone: " + EventListEntry.DecodeDevice(itm.deviceid, AppSettings.JABLOTRONZONES);
                            break;
                    }
                    if(device.Note != newEntryObj.Serialize())
                    {
                        var entry = new EventList();
                        entry.DeviceId = device.DeviceId;
                        entry.Created = DateTime.Now;
                        entry.EventCode = newEntryObj.Icon;
                        entry.EventText = newEntryObj.Serialize();
                        cx.EventLists.Add(entry);
                        cx.SaveChanges();
                        device.Note = entry.EventText;
                    }

                    device.Contacted = itm.timestamp;
                    if (itm.commandexecuted != "")
                    {
                        device.CommandToExecute = "";
                    }
                    device.LED_A = itm.led_a;
                    device.LED_B = itm.led_b;
                    device.LED_C = itm.led_c;
                    device.LED_Warning = itm.led_warning;
                    device.State = itm.state;
                }
                cx.SaveChanges();
            }
            var ret = new JablotronResponse();
            ret.status = "OK";
            ret.commandtoexecute = device.CommandToExecute;
            return JObject.Parse(JsonConvert.SerializeObject(ret));
        }

    }
}
