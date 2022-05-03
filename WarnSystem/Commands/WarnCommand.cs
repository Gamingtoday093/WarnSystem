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
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

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
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnNotYourself"), WarnSystem.Instance.MessageColour);
                return;
            }

            string reason = string.Join(" ", command.Skip(1));

            WarnSystem.Instance.WarnService.RegisterWarn((ulong)targetplayerCSteamID, (ulong)(isConsole ? CSteamID.Nil : player.CSteamID), reason);

            string playerCharacterName = isConsole ? "CONSOLE" : (player.CharacterName == "CONSOLE" ? "CONSOLE (Player)" : player.CharacterName);
            if (targetplayer?.Player != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarnSuccessTarget", playerCharacterName, reason), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnSuccess", targetplayerCharacterName, reason), WarnSystem.Instance.MessageColour);

            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{playerCharacterName} Warned {targetplayerCharacterName} for {reason}");
            }
            if (WarnSystem.Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
            {
                Logger.Log("Sending Discord Webhook");
                var task = DiscordWebhook.SendDiscordWebhook(WarnSystem.Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook(
                    "Warn System",
                    "https://unturnedstore.com/api/images/896",
                    "Player Warned",
                    9140721,
                    "Warn System by Gamingtoday093",
                    "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png",
                    targetplayerCharacterName,
                    targetplayerCSteamID.ToString(),
                    playerCharacterName,
                    reason,
                    SteamGameServer.GetPublicIP().ToString()
                    ));
                
                Task.Run(async() => await task);
            }
            OnWarn.Invoke(targetplayerCSteamID, isConsole ? CSteamID.Nil : player.CSteamID, reason);
        }
    }
}
