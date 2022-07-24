using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarnSystem.Connections;
using WarnSystem.Models;
using WarnSystem.Services;

namespace WarnSystem.Commands
{
    class WarnclearCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "Warnclear";

        public string Help => "Clear Warnings";

        public string Syntax => "<Player>";

        public List<string> Aliases => new List<string>() { "Warnclr" };

        public List<string> Permissions => new List<string>() { "WarnSystem.Warn" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnCInvalid"), WarnSystem.Instance.MessageColour);
                return;
            }

            bool isConsole = caller is ConsolePlayer;
            UnturnedPlayer player = isConsole ? null : (UnturnedPlayer)caller;
            UnturnedPlayer targetplayer = UnturnedPlayer.FromName(command[0]);
            CSteamID validCSteamID = getValidCSteamIDService.getValidCSteamID(command[0]);

            if (targetplayer?.Player == null && validCSteamID == CSteamID.Nil)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnNotFound"), WarnSystem.Instance.MessageColour);
                return;
            }

            var targetplayerCharacterName = targetplayer?.CharacterName ?? validCSteamID.ToString();
            var targetplayerCSteamID = targetplayer?.CSteamID ?? validCSteamID;

            if (!isConsole && targetplayerCSteamID == player.CSteamID && !player.IsAdmin)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNotYourself"), WarnSystem.Instance.MessageColour);
                return;
            }

            WarnGroup WarnGroup;
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL && !WarnSystem.Config.ShouldCacheMySQLData)
            {
                WarnGroup = Task.Run(async () => await WarnSystem.Instance.SQLDatabase.GetWarnGroupAsync(targetplayerCSteamID.m_SteamID)).Result;
            }
            else
            {
                WarnGroup = WarnSystem.Instance.GetWarnGroupFromData(targetplayerCSteamID.m_SteamID);
            }

            if (WarnGroup == null)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                return;
            }

            WarnSystem.Instance.WarnService.ClearWarns(WarnGroup);

            string playerCharacterName = isConsole ? "CONSOLE" : (player.CharacterName == "CONSOLE" ? "CONSOLE (Player)" : player.CharacterName);
            if (targetplayer?.Player != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarnCSuccessTarget", playerCharacterName), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnCSuccess", targetplayerCharacterName), WarnSystem.Instance.MessageColour);
            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{playerCharacterName} Cleared All Warnings from {targetplayerCharacterName}");
            }
            if (WarnSystem.Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
            {
                var task = DiscordWebhook.SendDiscordWebhook(WarnSystem.Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhookWarnClear(
                    "Warn System",
                    "https://unturnedstore.com/api/images/896",
                    "Player Warnings Cleared",
                    9140721,
                    "Warn System by Gamingtoday093",
                    "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png",
                    targetplayerCharacterName,
                    targetplayerCSteamID.ToString(),
                    playerCharacterName,
                    Provider.serverName,
                    SteamGameServer.GetPublicIP().ToString()
                    ));
                Task.Run(async () => await task);
            }
        }
    }
}
