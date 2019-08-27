using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySmartHomeCore.Models
{
    public enum AlarmState
    {
        IDLE,
        ARMED,
        OUTCOMMINGDELAY,
        INCOMMINGDELAY,
        ALARM,
        DISCONNECTED
    }


    [Table("Jablotron")]
    public class Jablotron
    {
        [Key]
        public Guid DeviceId { get; set; }

        [StringLength(1000)]
        public string Note { get; set; }

        [StringLength(100)]
        public string CommandToExecute { get; set; }

        public bool LED_A { get; set; }
        public bool LED_B { get; set; }
        public bool LED_C { get; set; }
        public bool LED_Warning { get; set; }
        [StringLength(50)]
        public string State { get; set; }
        public DateTime Contacted { get; set; }
    }
}