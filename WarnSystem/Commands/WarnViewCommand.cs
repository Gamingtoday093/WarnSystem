using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Services;
using Steamworks;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using WarnSystem.Models;
using SDG.Unturned;

namespace WarnSystem.Commands
{
    public class WarnViewCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "WarnView";

        public string Help => "View Warnings";

        public string Syntax => "<Player> <Index>";

        public List<string> Aliases => new List<string>() { "WarnV" };

        public List<string> Permissions => new List<string>() { "WarnSystem.View" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                if (caller is ConsolePlayer)
                {
                    Logger.LogError($"[WarnSystem] {WarnSystem.Instance.Translate("WarnsConsole")}");
                    return;
                }

                if (command.Length < 1)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVInvalid"), WarnSystem.Instance.MessageColour);
                    return;
                }

                UnturnedPlayer player = (UnturnedPlayer)caller;

                if (!int.TryParse(command[0], out int index))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelFailedParse"), WarnSystem.Instance.MessageColour);
                    return;
                }

                var WarnGroup = WarnSystem.Instance.Data.FirstOrDefault(x => x.SteamID == (ulong)player.CSteamID);
                if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnsNoWarns"), WarnSystem.Instance.MessageColour);
                    return;
                }

                index -= WarnSystem.Config.IndexOffset;

                if (index > (WarnGroup.Warnings.Count - 1) | index < 0)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelOutRange"), WarnSystem.Instance.MessageColour);
                    return;
                }

                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVSuccess", index + WarnSystem.Config.IndexOffset), WarnSystem.Instance.MessageColour);
                ViewWarning(caller, WarnGroup.Warnings[index]);
                return;
            }
            else
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

                if (!int.TryParse(command[1], out int index))
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelFailedParse"), WarnSystem.Instance.MessageColour);
                    return;
                }

                var WarnGroup = WarnSystem.Instance.Data.FirstOrDefault(x => x.SteamID == (ulong)targetplayerCSteamID);
                if (WarnGroup == null || WarnGroup.Warnings.Count <= 0)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelNoWarns"), WarnSystem.Instance.MessageColour);
                    return;
                }

                index -= WarnSystem.Config.IndexOffset;

                if (index > (WarnGroup.Warnings.Count - 1) | index < 0)
                {
                    UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarndelOutRange"), WarnSystem.Instance.MessageColour);
                    return;
                }

                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVSuccessT", targetplayerCharacterName, index + WarnSystem.Config.IndexOffset), WarnSystem.Instance.MessageColour);
                ViewWarning(caller, WarnGroup.Warnings[index]);
                return;
            }
        }

        private void ViewWarning(IRocketPlayer caller, Warn warning)
        {
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVModerator", UnturnedPlayer.FromCSteamID(new CSteamID(warning.moderatorSteamID64))?.Player?.channel?.owner?.playerID?.characterName ?? warning.moderatorSteamID64.ToString()), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVDateTime", warning.dateTime, FormatedTime.FormatSeconds(DateTimeOffset.Now - warning.dateTime)), WarnSystem.Instance.MessageColour);
            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("WarnVReason", warning.reason), WarnSystem.Instance.MessageColour);
        }
    }
}
