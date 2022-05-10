using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Database;
using WarnSystem.Models;
using Logger = Rocket.Core.Logging.Logger;

namespace WarnSystem.Commands
{
    public class MigrateDBCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "WSMigrateDB";

        public string Help => "Move Database Data to a new DatabaseSystem";

        public string Syntax => "<Old Database> <New Database>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateInvalid"), WarnSystem.Instance.MessageColour);
                return;
            }

            if (!Enum.TryParse(command[0].ToUpper(), out EDatabase oldDatabase) | !Enum.TryParse(command[1].ToUpper(), out EDatabase newDatabase))
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateFailedParse"), WarnSystem.Instance.MessageColour);
                return;
            }

            if (oldDatabase == newDatabase)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateSameDB"), WarnSystem.Instance.MessageColour);
                return;
            }

            if (newDatabase != WarnSystem.DatabaseSystem)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateNotActive"), WarnSystem.Instance.MessageColour);
                return;
            }

            if (WarnSystem.Instance.Data == null && WarnSystem.Config.ShouldCacheMySQLData && WarnSystem.DatabaseSystem == EDatabase.MYSQL)
            {
                UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateNotLoaded"), WarnSystem.Instance.MessageColour);
                return;
            }

            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateReading", oldDatabase), WarnSystem.Instance.MessageColour);

            List<WarnGroup> Data = WarnSystem.Instance.Data ?? new List<WarnGroup>();
            if (oldDatabase == EDatabase.JSON) Data = WarnSystem.Instance.JsonDatabase.ReadData() ?? Data;
            else if (oldDatabase == EDatabase.MYSQL) Data = WarnSystem.Instance.SQLDatabase.ReadData() ?? Data;

            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateSaving", newDatabase), WarnSystem.Instance.MessageColour);

            if (newDatabase == EDatabase.JSON) WarnSystem.Instance.JsonDatabase.SetSaveData(Data);
            else if (newDatabase == EDatabase.MYSQL) WarnSystem.Instance.SQLDatabase.SetSaveData(Data);

            UnturnedChat.Say(caller, WarnSystem.Instance.Translate("MigrateSuccess", oldDatabase, newDatabase), WarnSystem.Instance.MessageColour);
        }
    }
}
