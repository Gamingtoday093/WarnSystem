using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Core.Plugins;
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
using Rocket.Unturned;
using System.Threading;
using Rocket.Core.Utils;

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
            U.Events.OnPlayerConnected += PunishPlayerOnJoin;

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
            U.Events.OnPlayerConnected -= PunishPlayerOnJoin;

            Destroy(WarnService);

            SaveDatabases();

            Logger.Log($"{Name} has been Unloaded");
        }

        private void SaveDatabases()
        {
            if (DatabaseSystem == EDatabase.JSON) JsonDatabase.SaveData();
            if (DatabaseSystem == EDatabase.MYSQL) SQLDatabase.SaveData(Data);
        }

        private void PunishPlayerOnJoin(UnturnedPlayer player)
        {
            if (!Config.ReplicateSharedServersPunishments) return;
            OnWarned(player.CSteamID, CSteamID.Nil, null);
        }

        public void OnWarned(CSteamID targetSteamID, CSteamID moderatorSteamID, string reason)
        {
            if (DatabaseSystem == EDatabase.MYSQL && !Config.ShouldCacheMySQLData)
            {
                ThreadPool.QueueUserWorkItem(async (_) =>
                {
                    var WarnGroup = await SQLDatabase.GetWarnGroupAsync(targetSteamID.m_SteamID);
                    TaskDispatcher.QueueOnMainThread(() =>
                    {
                        if (WarnGroup == null) return;

                        PunishPlayer(WarnGroup, reason, string.IsNullOrEmpty(reason));
                    });
                });
            }
            else
            {
                var WarnGroup = GetWarnGroupFromData(targetSteamID.m_SteamID);
                if (WarnGroup == null) return;

                PunishPlayer(WarnGroup, reason, string.IsNullOrEmpty(reason));
            }
        }

        private void PunishPlayer(WarnGroup WarnGroup, string Reason, bool ReplicatedPunishment = false)
        {
            IEnumerable<Punishment> punishments = null;
            if (Config.ShouldRepeatHighestPunishmentIfAbove && Config.Punishments.LastOrDefault()?.WarnThreshold < WarnGroup.Warnings.Count)
            {
                var lastPunishment = Config.Punishments.LastOrDefault();
                if (lastPunishment != null)
                    punishments = Config.Punishments.Where(p => p.WarnThreshold == lastPunishment.WarnThreshold);
            }
            else
            {
                punishments = Config.Punishments.Where(p => p.WarnThreshold == WarnGroup.Warnings.Count);
            }
            if (punishments == null) return;

            CSteamID PlayerToBan = new CSteamID(WarnGroup.SteamID);
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(PlayerToBan);

            Warn LatestWarn = WarnGroup.Warnings.OrderByDescending(w => w.dateTime).First();
            Reason = ReplicatedPunishment ? LatestWarn.reason : Reason;

            foreach (var punishment in punishments.OrderBy(p => p, new PunishmentComparer()))
            {
                switch (punishment.Type.ToLower())
                {
                    case "kick":
                        if (ReplicatedPunishment || player?.Player == null) break;
                        player.Kick(Translate("WarnPunishReason", punishment.WarnThreshold, Reason));
                        return;
                    case "ban":
                        double duration = ReplicatedPunishment ? FormatedTime.Parse(punishment.Duration) - (DateTimeOffset.Now - LatestWarn.dateTime).TotalSeconds : FormatedTime.Parse(punishment.Duration);
                        if (duration <= uint.MinValue) break;
                        Provider.requestBanPlayer(CSteamID.Nil,
                            PlayerToBan,
                            SDG.Unturned.SteamGameServerNetworkingUtils.getIPv4AddressOrZero(PlayerToBan),
                            PlayerTool.getSteamPlayer(PlayerToBan)?.playerID?.GetHwids(),
                            Translate("WarnPunishReason", punishment.WarnThreshold, Reason),
                            duration > uint.MaxValue ? uint.MaxValue : (uint)duration);
                        return;
                    case "command":
                        if (ReplicatedPunishment) break;
                        R.Commands.Execute(new ConsolePlayer(), FormatPunishmentText(punishment.Command, PlayerToBan, player, Reason));
                        break;
                    case "tell":
                        if (ReplicatedPunishment) break;
                        UnturnedChat.Say(PlayerToBan, FormatPunishmentText(punishment.Text, PlayerToBan, player, Reason), MessageColour);
                        break;
                    default:
                        Logger.LogError("[WarnSystem] Warn Punishment Type Does not Exist! Either use: kick, ban, command or tell");
                        break;
                }
            }
        }

        private string FormatPunishmentText(string punishmentCommand, CSteamID TargetSteamId, UnturnedPlayer player, string Reason)
        {
            string Command = punishmentCommand;

            Command = Command.Replace("{TargetSteamId}", TargetSteamId.ToString());
            Command = Command.Replace("{TargetName}", player?.Player != null ? player.CharacterName : TargetSteamId.ToString());
            Command = Command.Replace("{Reason}", Reason);

            return Command;
        }

        public bool TryExpireWarnings(WarnGroup warnGroup, out List<Warn> newWarnings, out List<Warn> expiredWarnings)
        {
            expiredWarnings = null;
            newWarnings = null;
            if (!Config.ExpireWarnings.Enabled) return false;

            int WarningsCount = warnGroup.Warnings.Count;
            newWarnings = warnGroup.Warnings.Where(w => (DateTimeOffset.Now - w.dateTime).TotalSeconds < FormatedTime.Parse(Config.ExpireWarnings.DurationUntilExpiring)).ToList();
            expiredWarnings = warnGroup.Warnings.Where(w => (DateTimeOffset.Now - w.dateTime).TotalSeconds > FormatedTime.Parse(Config.ExpireWarnings.DurationUntilExpiring)).ToList();
            return newWarnings.Count != WarningsCount;
        }

        public WarnGroup GetWarnGroupFromData(ulong SteamId)
        {
            WarnGroup WarnGroup = Data.FirstOrDefault(x => x.SteamID == SteamId);

            if (TryExpireWarnings(WarnGroup, out List<Warn> newWarnings, out List<Warn> _))
            {
                WarnGroup.Warnings = newWarnings;
            }

            return WarnGroup;
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
            { "WarnsListItem", "- {0}" },

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
