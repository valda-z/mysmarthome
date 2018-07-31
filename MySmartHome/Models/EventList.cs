using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MySmartHome.Models
{
    [Table("EventList")]
    public class EventList
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid DeviceId { get; set; }
        [Index("IX_Created")]
        public DateTime Created { get; set; }

        [StringLength(50)]
        public string EventCode { get; set; }
        [StringLength(100)]
        public string EventText { get; set; }
    }
}