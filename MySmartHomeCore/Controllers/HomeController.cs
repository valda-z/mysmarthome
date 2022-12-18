using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using MySmartHomeCore.Models;
using MySmartHomeCore.Security.Cryptography;

namespace MySmartHomeCore.Controllers
{
    [Authorize(Roles = "MYHOME")]
    public class HomeController : Controller
    {
        private AppSettings AppSettings { get; set; }

        private ICompositeViewEngine _viewEngine;

        public HomeController(ICompositeViewEngine viewEngine, IOptions<AppSettings> settings)
        {
            _viewEngine = viewEngine;
            AppSettings = settings.Value;
        }

        private string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult =
                    _viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                var t = viewResult.View.RenderAsync(viewContext);
                t.Wait();

                return writer.GetStringBuilder().ToString();
            }
        }

        public class HomeIndexData
        {
            public Device unit1 { get; set; }
            public Jablotron jablotron { get; set; }
            public bool unit1Offline
            {
                get
                {
                    return (unit1.Contacted < DateTime.Now.AddMinutes(-1));
                }
            }

            public bool jablotronOffline
            {
                get
                {
                    return (jablotron.Contacted < DateTime.Now.AddMinutes(-1));
                }
            }

            public string checksum()
            {
                var sb = new StringBuilder();

                sb.Append(unit1.DogHouseHeatingOn);
                sb.Append("|");
                sb.Append(unit1.DogHouseTemperature);
                sb.Append("|");
                sb.Append(unit1.IsWet);
                sb.Append("|");
                sb.Append(unit1.Temperature);
                sb.Append("|");
                sb.Append(unit1.WaterOn);
                sb.Append("|");
                sb.Append(jablotron.State);
                sb.Append("|");
                sb.Append(jablotron.CommandToExecute);

                Crc32 crc32 = new Crc32();
                String hash = String.Empty;

                foreach (byte b in crc32.ComputeHash(Encoding.ASCII.GetBytes(sb.ToString())))
                {
                    hash += b.ToString("x2").ToLower();
                }

                return hash;
            }
        }

        public ActionResult IndexHash()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            var model = new HomeIndexData();
            Guid gid = new Guid(AppSettings.UNIT1);
            model.unit1 = cx.Devices.Single(e => e.DeviceId == gid);
            gid = new Guid(AppSettings.JABLOTRON);
            model.jablotron = cx.Jablotrons.Single(e => e.DeviceId == gid);

            return Content(model.checksum());
        }

        public ActionResult IndexPartial()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            var model = new HomeIndexData();
            Guid gid = new Guid(AppSettings.UNIT1);
            model.unit1 = cx.Devices.Single(e => e.DeviceId == gid);
            gid = new Guid(AppSettings.JABLOTRON);
            model.jablotron = cx.Jablotrons.Single(e => e.DeviceId == gid);

            return Content(RenderPartialViewToString("_HomeData", model));
        }

        public ActionResult Index(string cmd)
        {
            if (cmd == null)
            {
                cmd = "";
            }

            var cx = SmartHomeDBContext.Create(AppSettings);
            var model = new HomeIndexData();
            Guid gid = new Guid(AppSettings.UNIT1);
            model.unit1 = cx.Devices.Single(e => e.DeviceId == gid);
            gid = new Guid(AppSettings.JABLOTRON);
            model.jablotron = cx.Jablotrons.Single(e => e.DeviceId == gid);

            switch (cmd)
            {
                case "switchwater":
                    {
                        var conf = SmartHomeConfig.Deserialize(model.unit1.Config);
                        conf.water = new SmartHomeConfig.MySmartHomeConfigWaterHardOn();
                        conf.irrigationOn = !conf.irrigationOn;
                        model.unit1.Config = conf.Serialize();
                        cx.SaveChanges();
                        return RedirectToAction("Index");
                    }
                //break;
                case "switchheating":
                    {
                        var conf = SmartHomeConfig.Deserialize(model.unit1.Config);
                        conf.dogontemp = conf.dogontemp<-99.0 ? 10 : -100.0;
                        model.unit1.Config = conf.Serialize();
                        cx.SaveChanges();
                        return RedirectToAction("Index");
                    }
                //break;
                case "switchalarm":
                    {
                        if (model.jablotron.CommandToExecute == "")
                        {
                            model.jablotron.CommandToExecute = model.jablotron.State == "IDLE" ? "ARM" : "DISARM";
                        }
                        else
                        {
                            model.jablotron.CommandToExecute = model.jablotron.CommandToExecute == "ARM" ? "DISARM" : "ARM";
                        }
                        cx.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    //break;
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Timeline()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            var model = cx.EventLists.AsQueryable().OrderByDescending(e => e.Created).Take(25);
            return View(model);
        }

        [HttpGet]
        public ActionResult Setting()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            Guid gid = new Guid(AppSettings.UNIT1);
            var model = SmartHomeConfig.Deserialize(cx.Devices.Single(e => e.DeviceId == gid).Config);
            return View(model);
        }

        [HttpPost]
        public ActionResult Setting(IFormCollection form)
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            Guid gid = new Guid(AppSettings.UNIT1);
            var model = cx.Devices.Single(e => e.DeviceId == gid);
            var conf = SmartHomeConfig.Deserialize(model.Config);
            conf.dogontemp = int.Parse(form["dogontemp"]);
            conf.irrigationOn = (form["irrigationOn"].Contains("true"));
            conf.christmasOn = (form["christmasOn"].Contains("true"));
            conf.ClearWaterItems();
            for (int i = 0; i < 20; i++)
            {
                string f = "irrigation" + i.ToString();
                if (form.ContainsKey(f))
                {
                    var itm = new SmartHomeConfig.MySmartHomeConfigWaterItem();
                    itm.starthour = form[f];
                    itm.intervalsec = int.Parse(form[f + "m"]) * 60;
                    conf.AddWaterItem(itm);
                }
            }
            model.Config = conf.Serialize();
            cx.SaveChanges();
            return View(conf);
        }

        [HttpGet]
        public ActionResult SettingHeating()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            Guid gid = new Guid(AppSettings.JABLOTRON);
            var model = SmartHomeConfig.Deserialize(cx.Devices.Single(e => e.DeviceId == gid).Config);
            return View(model);
        }

        [HttpPost]
        public ActionResult SettingHeating(IFormCollection form)
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            Guid gid = new Guid(AppSettings.JABLOTRON);
            var model = cx.Devices.Single(e => e.DeviceId == gid);
            var conf = SmartHomeConfig.Deserialize(model.Config);
            conf.homeheatingoutofhometemp = int.Parse(form["homeheatingoutofhometemp"]);
            conf.homeheatingoutofhome = (form["homeheatingoutofhome"].Contains("true"));
            List<SmartHomeConfig.MySmartHomeHeatingItem> data = new List<SmartHomeConfig.MySmartHomeHeatingItem>();
            foreach (var i in form.Keys.Where(e => e.StartsWith("ht_")))
            {
                var iarr = i.Split('_');
                DayOfWeek _d = (DayOfWeek)int.Parse(iarr[1]);
                int _h = int.Parse(iarr[2]);
                decimal _t = decimal.Parse(form[i]);
                var k = new SmartHomeConfig.MySmartHomeHeatingItem();
                k.d = _d;
                k.h = _h;
                k.t = _t;
                data.Add(k);
            }
            conf.heatingitems = data.ToArray();
            model.Config = conf.Serialize();
            cx.SaveChanges();
            return View(conf);
        }

        public class StatObject
        {
            public List<StatItem> items = new List<StatItem>();
            public decimal min = 1000;
            public decimal max = -1000;
        }

        public class StatItem
        {
            public string time { get; set; }
            public decimal temp { get; set; }
            public decimal dogtemp { get; set; }
            public bool iswet { get; set; }
            public bool wateron { get; set; }
            public bool dogheating { get; set; }
        }

        public ActionResult Stats()
        {
            Guid gid = new Guid(AppSettings.UNIT1);
            return stats(gid);
        }

        public ActionResult StatsHome()
        {
            Guid gid = new Guid(AppSettings.JABLOTRON);
            return stats(gid);
        }

        private ActionResult stats(Guid gid)
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            var fromD = DateTime.Now.AddDays(-1);
            var toD = DateTime.Now;
            var firstlist = from s in cx.DeviceLogs
                            where s.DeviceId == gid && s.Created > fromD && s.Created <= toD
                            select s;
            var list = from s in firstlist.ToList()
                       let groupKey = new DateTime(s.Created.Year, s.Created.Month, s.Created.Day, s.Created.Hour, (s.Created.Minute / 10) * 10, 0)
                       group s by groupKey into g
                       select new
                       {
                           Timestamp = g.Key,
                           DogHouseHeat = g.Max(e => e.DogHouseHeatingOn),
                           DogHouseTemp = g.Average(e => e.DogHouseTemperature),
                           IsWet = g.Max(e => e.IsWet),
                           WaterOn = g.Max(e => e.WaterOn),
                           Temp = g.Average(e => e.Temperature)
                       };
            var finallist = list.OrderBy(e => e.Timestamp);

            var ret = new StatObject();

            foreach (var i in finallist)
            {
                var itm = new StatItem();
                itm.dogheating = i.DogHouseHeat;
                itm.dogtemp = i.DogHouseTemp;
                itm.iswet = i.IsWet;
                itm.temp = i.Temp;
                itm.time = MySmartHomeCore.Extensions.DateTimeUtil.GetZonedDate(i.Timestamp, "HH:mm");
                itm.wateron = i.WaterOn;
                if (ret.min > itm.dogtemp)
                {
                    ret.min = itm.dogtemp;
                }
                if (ret.min > itm.temp)
                {
                    ret.min = itm.temp;
                }
                if (ret.max < itm.dogtemp)
                {
                    ret.max = itm.dogtemp;
                }
                if (ret.max < i.Temp)
                {
                    ret.max = itm.temp;
                }
                ret.items.Add(itm);
            }

            return View(ret);
        }

        [HttpGet]
        public ActionResult Water()
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            ViewBag.Saved = "";
            Guid gid = new Guid(AppSettings.UNIT1);
            var model = SmartHomeConfig.Deserialize(cx.Devices.Single(e => e.DeviceId == gid).Config);
            return View(model);
        }

        [HttpPost]
        public ActionResult Water(IFormCollection form)
        {
            var cx = SmartHomeDBContext.Create(AppSettings);
            ViewBag.Saved = "YES";
            Guid gid = new Guid(AppSettings.UNIT1);
            var model = cx.Devices.Single(e => e.DeviceId == gid);
            var conf = SmartHomeConfig.Deserialize(model.Config);
            conf.water = new SmartHomeConfig.MySmartHomeConfigWaterHardOn();
            conf.water.from = DateTime.Now;
            conf.water.to = DateTime.Now.AddMinutes(1);
            model.Config = conf.Serialize();
            cx.SaveChanges();
            return View(conf);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
