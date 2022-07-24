using System.Xml.Serialization;

namespace WarnSystem.Models
{
    public class ExpireWarnings
    {
        [XmlAttribute]
        public bool Enabled { get; set; }
        public string DurationUntilExpiring { get; set; }
    }
}
