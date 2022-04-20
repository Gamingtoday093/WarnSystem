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
    class WarndeleteCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Warndelete";

        public string Help => "Delete Individual Warnings";

        public string Syntax => "<Player> <Index>";

        public List<string> Aliases => new List<string>() { "Warndel" };

        public List<string> Permissions => new List<string>() { "WarnSystem.Warn" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelInvalid"), WarnSystem.Instance.MessageColour);
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

            if (!int.TryParse(command[1], out int index))
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelFailedParse"), WarnSystem.Instance.MessageColour);
                return;
            }

            if (index > (WarnGroup.Warnings.Count - 1) | index < 0)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelOutRange"), WarnSystem.Instance.MessageColour);
                return;
            }

            WarnSystem.Instance.WarnService.RemoveWarn((ulong)targetplayerCSteamID, index);
            if (targetplayer != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarndelSuccessTarget", player.CharacterName), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelSuccess", index, targetplayerCharacterName), WarnSystem.Instance.MessageColour);
            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{player.CharacterName} Removed Warning {index} from {targetplayerCharacterName}");
            }
            if (WarnSystem.Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
            {
                var task = DiscordWebhook.SendDiscordWebhook(WarnSystem.Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook(
                    "Warn System",
                    "https://unturnedstore.com/api/images/894",
                    "Player Warning Removed",
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
