using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Models;
using System.Xml.Serialization;

namespace WarnSystem
{
    public class WarnSystemConfiguration : IRocketPluginConfiguration
    {
        public string MessageColour { get; set; }
        public string DatabaseSystem { get; set; }
        public string MySQLConnectionString { get; set; }
        public bool ShouldCacheMySQLData { get; set; }
        public bool ReplicateSharedServersPunishments { get; set; }
        public bool ShouldRepeatHighestPunishmentIfAbove { get; set; }
        public int IndexOffset { get; set; }
        public bool DisplayWarningsInline { get; set; }
        public bool ShouldLogConsole { get; set; }
        public string DiscordWebhookURL { get; set; }
        public bool DiscordWebhookHideServerIP { get; set; }
        public ExpireWarnings ExpireWarnings { get; set; }
        [XmlArrayItem("Punishment")]
        public Punishment[] Punishments { get; set; }
        public void LoadDefaults()
        {
            MessageColour = "yellow";
            DatabaseSystem = "Json";
            MySQLConnectionString = "SERVER=127.0.0.1;DATABASE=unturned;UID=root;PASSWORD=123;PORT=3306;TABLENAME=warnsystem;";
            ShouldCacheMySQLData = true;
            ReplicateSharedServersPunishments = false;
            ShouldRepeatHighestPunishmentIfAbove = true;
            IndexOffset = 1;
            DisplayWarningsInline = true;
            ShouldLogConsole = true;
            DiscordWebhookURL = "Webhook";
            DiscordWebhookHideServerIP = false;
            ExpireWarnings = new ExpireWarnings()
            {
                Enabled = false,
                DurationUntilExpiring = "6m"
            };
            Punishments = new Punishment[]
            {
                new Punishment()
                {
                    WarnThreshold = 1,
                    Type = "tell",
                    Text = "Hey don't Break our Rules! {Reason}? Not Cool. Now see what you've done"
                },
                new Punishment()
                {
                    WarnThreshold = 1,
                    Type = "command",
                    Command = "say \"Because {TargetName} Broke a Rule the Weather will now be set to Raining ;(\""
                },
                new Punishment()
                {
                    WarnThreshold = 1,
                    Type = "command",
                    Command = "weather Storm"
                },
                new Punishment()
                {
                    WarnThreshold = 2,
                    Type = "kick"
                },
                new Punishment()
                {
                    WarnThreshold = 3,
                    Type = "ban",
                    Duration = "300"
                },
                new Punishment()
                {
                    WarnThreshold = 4,
                    Type = "ban",
                    Duration = "30min"
                },
                new Punishment()
                {
                    WarnThreshold = 5,
                    Type = "ban",
                    Duration = "1d"
                },
                new Punishment()
                {
                    WarnThreshold = 6,
                    Type = "ban",
                    Duration = "1w"
                },
                new Punishment()
                {
                    WarnThreshold = 7,
                    Type = "ban",
                    Duration = "15778463"
                }
            };
        }
    }
}
