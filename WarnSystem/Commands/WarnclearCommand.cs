using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Connections;
using WarnSystem.Services;

namespace WarnSystem.Commands
{
    class WarnclearCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

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

            UnturnedPlayer player = (UnturnedPlayer)caller;
            UnturnedPlayer targetplayer = UnturnedPlayer.FromName(command[0]);
            CSteamID validCSteamID = getValidCSteamIDService.getValidCSteamID(command[0]);

            if (targetplayer == null && validCSteamID == CSteamID.Nil)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnNotFound"), WarnSystem.Instance.MessageColour);
                return;
            }

            var targetplayerCharacterName = targetplayer?.CharacterName ?? validCSteamID.ToString();
            var targetplayerCSteamID = targetplayer?.CSteamID ?? validCSteamID;

            if (targetplayerCSteamID == player.CSteamID && player.IsAdmin == false)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNotYourself"), WarnSystem.Instance.MessageColour);
                return;
            }

            var WarnGroup = WarnSystem.Instance.Data.FirstOrDefault(x => x.CSteamID64 == (ulong)targetplayerCSteamID);
            if (WarnGroup == null)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                return;
            }

            WarnSystem.Instance.WarnService.ClearWarns((ulong)targetplayerCSteamID);
            if (targetplayer != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarnCSuccessTarget", player.CharacterName), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnCSuccess", targetplayerCharacterName), WarnSystem.Instance.MessageColour);
            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{player.CharacterName} Cleared All Warnings from {targetplayerCharacterName}");
            }
            if (WarnSystem.Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
            {
                var task = DiscordWebhook.SendDiscordWebhook(WarnSystem.Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook(
                    "Warn System",
                    "https://unturnedstore.com/api/images/894",
                    "Player Warnings Cleared",
                    7127038,
                    "Warn System by Gamingtoday093",
                    "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png",
                    targetplayerCharacterName,
                    targetplayerCSteamID.ToString(),
                    player.DisplayName,
                    string.Empty,
                    SteamGameServer.GetPublicIP().ToString()
                    ));
                Task.Run(async () => await task);
            }
        }
    }
}
