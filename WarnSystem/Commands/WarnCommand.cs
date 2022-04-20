using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Core.Logging;
using WarnSystem.Events;
using WarnSystem.Connections;
using Steamworks;
using WarnSystem.Services;

namespace WarnSystem.Commands
{
    public class WarnCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Warn";

        public string Help => "Warn Rulebreakers";

        public string Syntax => "<Player> <Reason>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "WarnSystem.Warn" };

        public static event OnWarnHandler OnWarn;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnInvalid"), WarnSystem.Instance.MessageColour);
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
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnNotYourself"), WarnSystem.Instance.MessageColour);
                return;
            }

            List<string> reasonArray = new List<string>(command);
            reasonArray.RemoveAt(0);
            string reason = string.Join(" ", reasonArray);

            WarnSystem.Instance.WarnService.RegisterWarn((ulong)targetplayerCSteamID, (ulong)player.CSteamID, reason);
            if (targetplayer != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarnSuccessTarget", player.CharacterName, reason), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnSuccess", targetplayerCharacterName, reason), WarnSystem.Instance.MessageColour);
            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{player.CharacterName} Warned {targetplayerCharacterName} for {reason}");
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
                    reason,
                    SteamGameServer.GetPublicIP().ToString()
                    ));
                
                Task.Run(async() => await task);
            }
            OnWarn.Invoke(targetplayerCSteamID);
        }
    }
}
