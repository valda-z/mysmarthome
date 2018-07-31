using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    [Table("DeviceLog")]
    public class DeviceLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index("IX_DeviceIdCreated", 1)]
        public Guid DeviceId { get; set; }
        [Index("IX_DeviceIdCreated", 2)]
        public DateTime Created { get; set; }

        public bool DogHouseHeatingOn { get; set; }
        public decimal DogHouseTemperature { get; set; }
        public bool WaterOn { get; set; }
        public bool IsWet { get; set; }
        public decimal Temperature { get; set; }
    }
}