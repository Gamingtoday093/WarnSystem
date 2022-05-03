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
    }
}
