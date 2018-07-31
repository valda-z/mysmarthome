using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    [Table("Device")]
    public class Device
    {
        [Key]
        public Guid DeviceId { get; set; }

        [StringLength(50)]
        public string Note { get; set; }

        [StringLength(8000)]
        public string Config { get; set; }

        public bool DogHouseHeatingOn { get; set; }
        public decimal DogHouseTemperature { get; set; }
        public bool WaterOn { get; set; }
        public bool IsWet { get; set; }
        public decimal Temperature { get; set; }
        public DateTime Contacted { get; set; }
    }
}