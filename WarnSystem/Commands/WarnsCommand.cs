using Rocket.API;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Services;
using Steamworks;
using Logger = Rocket.Core.Logging.Logger;
using System.Threading;
using Rocket.Core.Utils;
using WarnSystem.Models;

namespace WarnSystem.Commands
{
    public class WarnsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "Warnings";

        public string Help => "View a List of Warnings";

        public string Syntax => "<Player>";

        public List<string> Aliases => new List<string>() { "Warns" };

        public List<string> Permissions => new List<string>() { "WarnSystem.View" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                if (caller is ConsolePlayer)
                {
                    Logger.LogError($"[WarnSystem] {WarnSystem.Instance.Translate("WarnsConsole")}");
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;

                if (WarnSystem.DatabaseSystem == EDatabase.MYSQL && !WarnSystem.Config.ShouldCacheMySQLData)
                {
                    ThreadPool.QueueUserWorkItem(async (_) =>
                    {
                        var WarnGroup = await WarnSystem.Instance.SQLDatabase.GetWarnGroupAsync((ulong)player.CSteamID);
                        TaskDispatcher.QueueOnMainThread(() =>
                        {
                            if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                            {
                                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsNoWarns"), WarnSystem.Instance.MessageColour);
                                return;
                            }

                            DisplayWarnings(caller, WarnGroup);
                        });
                    });
                }
                else
                {
                    var WarnGroup = WarnSystem.Instance.GetWarnGroupFromData(player.CSteamID.m_SteamID);
                    if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                    {
                        UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsNoWarns"), WarnSystem.Instance.MessageColour);
                        return;
                    }

                    DisplayWarnings(caller, WarnGroup);
                }
            } else
            {
                UnturnedPlayer targetplayer = UnturnedPlayer.FromName(command[0]);
                CSteamID validCSteamID = getValidCSteamIDService.getValidCSteamID(command[0]);

                if (targetplayer == null && validCSteamID == CSteamID.Nil)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnNotFound"), WarnSystem.Instance.MessageColour);
                    return;
                }

                var targetplayerCharacterName = targetplayer?.CharacterName ?? validCSteamID.ToString();
                var targetplayerCSteamID = targetplayer?.CSteamID ?? validCSteamID;

                if (WarnSystem.DatabaseSystem == EDatabase.MYSQL && !WarnSystem.Config.ShouldCacheMySQLData)
                {
                    ThreadPool.QueueUserWorkItem(async (_) =>
                    {
                        var WarnGroup = await WarnSystem.Instance.SQLDatabase.GetWarnGroupAsync(targetplayerCSteamID.m_SteamID);
                        TaskDispatcher.QueueOnMainThread(() =>
                        {
                            if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                            {
                                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                                return;
                            }

                            DisplayWarnings(caller, WarnGroup, targetplayerCharacterName);
                        });
                    });
                }
                else
                {
                    var WarnGroup = WarnSystem.Instance.GetWarnGroupFromData(targetplayerCSteamID.m_SteamID);
                    if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                    {
                        UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                        return;
                    }

                    DisplayWarnings(caller, WarnGroup, targetplayerCharacterName);
                }
            }
        }

        private void DisplayWarnings(IRocketPlayer caller, WarnGroup WarnGroup, string TargetPlayerCharacterName = null)
        {
            if (WarnSystem.Config.DisplayWarningsInline)
            {
                if (string.IsNullOrEmpty(TargetPlayerCharacterName))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsList", WarnGroup.Warnings.Count, string.Join(", ", WarnGroup.Warnings.Select(w => w.reason))), WarnSystem.Instance.MessageColour);
                }
                else
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsListT", TargetPlayerCharacterName, WarnGroup.Warnings.Count, string.Join(", ", WarnGroup.Warnings.Select(w => w.reason))), WarnSystem.Instance.MessageColour);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(TargetPlayerCharacterName))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsList", WarnGroup.Warnings.Count, string.Empty), WarnSystem.Instance.MessageColour);
                }
                else
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsListT", TargetPlayerCharacterName, WarnGroup.Warnings.Count, string.Empty), WarnSystem.Instance.MessageColour);
                }
                foreach (string WarnReason in WarnGroup.Warnings.Select(w => w.reason))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsListItem", WarnReason), WarnSystem.Instance.MessageColour);
                }
            }
        }
    }
}
