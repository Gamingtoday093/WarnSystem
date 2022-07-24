using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WarnSystem.Models
{
    public class Punishment
    {
        [XmlAttribute("WarnThreshold")]
        public int WarnThreshold { get; set; }
        [XmlAttribute("Type")]
        public string Type { get; set; }
        public string Duration { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
    }

    public class PunishmentComparer : IComparer<Punishment>
    {
        public int Compare(Punishment x, Punishment y)
        {
            if (CompareType(x, y.Type)) return 0;
            if (!ShouldBeLast(x) && !ShouldBeLast(y)) return 0;
            if (ShouldBeLast(x) && !ShouldBeLast(y)) return 1;
            if (!ShouldBeLast(x) && ShouldBeLast(y)) return -1;
            if (CompareType(x, "ban")) return -1;
            if (CompareType(y, "ban")) return 1;
            return 0;
        }

        public bool ShouldBeLast(Punishment punishment)
        {
            return CompareType(punishment, "kick") || CompareType(punishment, "ban");
        }

        public bool CompareType(Punishment punishment, string toType)
        {
            return punishment.Type.Equals(toType, StringComparison.OrdinalIgnoreCase);
        }
    }
}
