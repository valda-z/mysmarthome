using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    public class SmartHomeDBContext : DbContext
    {
        public SmartHomeDBContext()
            : base("DefaultConnection")
        {
        }

        public static SmartHomeDBContext Create()
        {
            return new SmartHomeDBContext();
        }

        public System.Data.Entity.DbSet<Device> Devices { get; set; }
        public System.Data.Entity.DbSet<DeviceLog> DeviceLogs { get; set; }
        public System.Data.Entity.DbSet<Jablotron> Jablotrons { get; set; }
        public System.Data.Entity.DbSet<EventList> EventLists { get; set; }
    }
}