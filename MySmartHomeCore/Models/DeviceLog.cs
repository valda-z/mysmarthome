using System;

namespace MySmartHomeCore.Models
{
    public class DeviceLog
    {
        public int Id { get; set; }

        public Guid DeviceId { get; set; }
        public DateTime Created { get; set; }

        public bool DogHouseHeatingOn { get; set; }
        public decimal DogHouseTemperature { get; set; }
        public bool WaterOn { get; set; }
        public bool IsWet { get; set; }
        public decimal Temperature { get; set; }
    }
}