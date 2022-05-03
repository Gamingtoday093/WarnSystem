﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using WarnSystem.Database;
using WarnSystem.Services;
using Rocket.API.Collections;
using Steamworks;
using WarnSystem.Commands;
using Rocket.Unturned.Player;
using Rocket.API;
using Rocket.Core;
using SDG.Unturned;
using WarnSystem.Models;
using Logger = Rocket.Core.Logging.Logger;
using UnityEngine;

namespace WarnSystem
{
    public class WarnSystem : RocketPlugin<WarnSystemConfiguration>
    {
        public static WarnSystem Instance { get; private set; }
        public static WarnSystemConfiguration Config { get; private set; }
        public Color MessageColour { get; private set; }
        public JsonDatabase JsonDatabase { get; private set; }
        public SQLDatabase SQLDatabase { get; private set; }
        public List<WarnGroup> Data =>
            (DatabaseSystem == EDatabase.MYSQL) ? SQLDatabase.Data :
            (DatabaseSystem == EDatabase.JSON) ? JsonDatabase.Data :
            null;
        public WarnService WarnService { get; private set; }
        public static EDatabase DatabaseSystem { get; private set; }
        protected override void Load()
        {
            Instance = this;
            Config = Configuration.Instance;
            MessageColour = UnturnedChat.GetColorFromName(Config.MessageColour, Color.green);
            DatabaseSystem = Enum.TryParse(Config.DatabaseSystem.ToUpper(), out EDatabase databaseSystem) ? databaseSystem : EDatabase.JSON;
            Logger.Log($"Using a {DatabaseSystem} Database");

            WarnCommand.OnWarn += OnWarned;

            JsonDatabase = new JsonDatabase();
            if (DatabaseSystem == EDatabase.JSON) JsonDatabase.Reload();

            SQLDatabase = new SQLDatabase();
            if (DatabaseSystem == EDatabase.MYSQL) SQLDatabase.Reload();

            WarnService = gameObject.AddComponent<WarnService>();

            Logger.Log($"{Name} {Assembly.GetName().Version} by Gamingtoday093 has been Loaded");
        }

        protected override void Unload()
        {
            WarnCommand.OnWarn -= OnWarned;

            Destroy(WarnService);

            SaveDatabases();

            Logger.Log($"{Name} has been Unloaded");
        }

        private void SaveDatabases()
        {
            if (DatabaseSystem == EDatabase.JSON) JsonDatabase.SaveData();
            if (DatabaseSystem == EDatabase.MYSQL) SQLDatabase.SaveData();
        }

        private void OnWarned(CSteamID targetSteamID, CSteamID moderatorSteamID, string reason)
        {
            var WarnGroup = Data.FirstOrDefault(x => x.SteamID == (ulong)targetSteamID);
            if (WarnGroup == null)
            {
                Logger.LogError("[WarnSystem] Warned Player does not Exist?");
                return;
            }

            Punishment punishment = null;
            if (Config.ShouldRepeatHighestPunishmentIfAbove && Config.Punishments.LastOrDefault()?.WarnThreshold < WarnGroup.Warnings.Count)
            {
                punishment = Config.Punishments.LastOrDefault();
            }
            else
            {
                punishment = Config.Punishments.FirstOrDefault(x => x.WarnThreshold == WarnGroup.Warnings.Count);
            }
            if (punishment == null) return;

            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(targetSteamID);

            switch (punishment.Type.ToLower())
            {
                case "kick":
                    player.Kick(Translate("WarnPunishReason", punishment.WarnThreshold, reason));
                    break;
                case "ban":
                    double duration = FormatedTime.Parse(punishment.Duration);
                    player.Ban(Translate("WarnPunishReason", punishment.WarnThreshold, reason), duration > uint.MaxValue ? uint.MaxValue : (uint)duration);
                    break;
                default:
                    Logger.LogError("[WarnSystem] Warn Punishment Type Does not Exist! Either use: kick or ban");
                    break;
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "WarnInvalid", "You must Specify a Player and Reason!" },
            { "WarnNotFound", "Player not Found / Invalid SteamID64" },
            { "WarnNotYourself", "You can't Warn yourself!" },
            { "WarnSuccess", "You have Successfully Warned {0} for {1}!" },
            { "WarnSuccessTarget", "You have Been Warned by {0} for {1}! Read our Rules! Type /Rules" },

            { "WarndelInvalid", "You must Specify Player and Warn Index!" },
            { "WarndelNotYourself", "You can't Delete your Own Warnings!" },
            { "WarndelNoWarns", "That Player has no Warnings!" },
            { "WarndelFailedParse", "Failed to Parse Index" },
            { "WarndelOutRange", "Index Value Out of Range" },
            { "WarndelSuccess", "Successfully Removed Warning {0} from {1}!" },
            { "WarndelSuccessTarget", "{0} Removed a Warning from you!" },

            { "WarnCInvalid", "You must Specify a Player!" },
            { "WarnCSuccess", "Successfully Cleared All Warnings from {0}!" },
            { "WarnCSuccessTarget", "All of your Warnings have been Cleared by {0}!" },

            { "WarnsNoWarns", "You have no Warnings!" },
            { "WarnsConsole", "The Console has no Warnings!" },
            { "WarnsList", "Warnings({0}): {1}" },
            { "WarnsListT", "{0}'s Warnings({1}): {2}" },

            { "WarnVInvalid", "You must Specify Warn Index!" },
            { "WarnVSuccess", "Viewing Your Warning {0}:" },
            { "WarnVSuccessT", "Viewing {0}'s Warning {1}:" },
            { "WarnVModerator", "Moderator: {0}" },
            { "WarnVDateTime", "Time: {0} ({1} Ago)" },
            { "WarnVReason", "Reason: {0}" },

            { "WarnPunishReason", "Warn Threshold Reached ({0})! ({1})" },

            { "MigrateInvalid", "You must Specify Old Database and New Database Type!" },
            { "MigrateFailedParse", "Failed to Parse Database Types" },
            { "MigrateSameDB", "Old Database and New Database can't be the Same!" },
            { "MigrateNotActive", "New Database must be the same as the Current Database Type!" },
            { "MigrateNotLoaded", "Database hasn't been Loaded!" },
            { "MigrateReading", "Reading from {0} Database.." },
            { "MigrateSaving", "Saving to {0} Database.." },
            { "MigrateSuccess", "Successfully Migrated from {0} to {1}!" }
        };
    }
}
