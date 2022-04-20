using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnSystem.Models
{
    public class Warn
    {
        public ulong owner { get; set; }
        public ulong moderatorSteamID64 { get; set; }
        public DateTimeOffset dateTime { get; set; }
        public string reason { get; set; }
    }
}
