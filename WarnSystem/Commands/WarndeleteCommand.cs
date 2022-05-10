using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
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
    class WarndeleteCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

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

            int index = -1;
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL && !WarnSystem.Config.ShouldCacheMySQLData)
            {
                bool ShouldReturn = false;
                ThreadPool.QueueUserWorkItem(async (_) =>
                {
                    var WarnGroup = await WarnSystem.Instance.SQLDatabase.GetWarnGroupAsync(targetplayerCSteamID.m_SteamID);
                    TaskDispatcher.QueueOnMainThread(() =>
                    {
                        if (WarnGroup == null)
                        {
                            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                            ShouldReturn = true;
                            return;
                        }

                        if (!int.TryParse(command[1], out index))
                        {
                            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelFailedParse"), WarnSystem.Instance.MessageColour);
                            ShouldReturn = true;
                            return;
                        }

                        index -= WarnSystem.Config.IndexOffset;

                        if (index > (WarnGroup.Warnings.Count - 1) | index < 0)
                        {
                            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelOutRange"), WarnSystem.Instance.MessageColour);
                            ShouldReturn = true;
                            return;
                        }

                        WarnSystem.Instance.WarnService.RemoveWarn(index, WarnGroup);

                        index += WarnSystem.Config.IndexOffset;
                    });
                });
                if (ShouldReturn) return;
            }
            else
            {
                var WarnGroup = WarnSystem.Instance.Data.FirstOrDefault(x => x.SteamID == targetplayerCSteamID.m_SteamID);
                if (WarnGroup == null)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                    return;
                }

                if (!int.TryParse(command[1], out index))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelFailedParse"), WarnSystem.Instance.MessageColour);
                    return;
                }

                index -= WarnSystem.Config.IndexOffset;

                if (index > (WarnGroup.Warnings.Count - 1) | index < 0)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelOutRange"), WarnSystem.Instance.MessageColour);
                    return;
                }

                WarnSystem.Instance.WarnService.RemoveWarn(index, WarnGroup);

                index += WarnSystem.Config.IndexOffset;
            }

            string playerCharacterName = isConsole ? "CONSOLE" : (player.CharacterName == "CONSOLE" ? "CONSOLE (Player)" : player.CharacterName);
            if (targetplayer?.Player != null) UnturnedChat.Say(targetplayer, WarnSystem.Instance.Translate("WarndelSuccessTarget", playerCharacterName), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelSuccess", index, targetplayerCharacterName), WarnSystem.Instance.MessageColour);

            if (WarnSystem.Config.ShouldLogConsole)
            {
                Logger.Log($"{playerCharacterName} Removed Warning {index} from {targetplayerCharacterName}");
            }
            if (WarnSystem.Config.DiscordWebhookURL.StartsWith("https://discord.com/api/webhooks/"))
            {
                var task = DiscordWebhook.SendDiscordWebhook(WarnSystem.Config.DiscordWebhookURL, DiscordWebhook.FormatDiscordWebhook(
                    "Warn System",
                    "https://unturnedstore.com/api/images/896",
                    "Player Warning Removed",
                    9140721,
                    "Warn System by Gamingtoday093",
                    "https://cdn.discordapp.com/attachments/545016765885972494/907732705553317939/User.png",
                    targetplayerCharacterName,
                    targetplayerCSteamID.ToString(),
                    playerCharacterName,
                    string.Empty,
                    SteamGameServer.GetPublicIP().ToString()
                    ));

                Task.Run(async () => await task);
            }
        }
    }
}
