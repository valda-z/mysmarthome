using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MySmartHomeCore.Extensions
{
    public static class DateTimeUtil
    {
        public static string GetZonedDate(DateTime dt, string format)
        {
            DateTimeZone tz = DateTimeZoneProviders.Tzdb["Europe/Prague"]; // Get the system's time zone
            var zdt = new ZonedDateTime(Instant.FromDateTimeUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc)), tz);
            return zdt.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
