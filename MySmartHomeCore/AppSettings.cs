using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MySmartHomeCore
{
    public class AppSettings
    {
        public string JABLOTRONZONES { get; set; }
    }
    public class EnvSettings
    {
        static public string WORKDIR { get { return Environment.GetEnvironmentVariable("WORKDIR"); } }
        static public string UNIT1 { get { return Environment.GetEnvironmentVariable("UNIT1"); } }
        static public string JABLOTRON { get { return Environment.GetEnvironmentVariable("JABLOTRON"); } }
    }
}
