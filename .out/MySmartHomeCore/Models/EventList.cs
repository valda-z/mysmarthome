using System;

namespace MySmartHomeCore.Models
{
    public class EventList
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime Created { get; set; }
        public string EventCode { get; set; }
        public string EventText { get; set; }
    }
}