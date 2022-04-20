using Rocket.API;
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
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public string Name => "WSMigrateDB";

        public string Help => "Move Database Data to a new DatabaseSystem";

        public string Syntax => "<Old Database> <New Database>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 2)
            {
                Logger.LogError("[WarnSystem] You must Specify Old Database and New Database Type!");
                return;
            }

            if (!Enum.TryParse(command[0].ToUpper(), out EDatabase oldDatabase) | !Enum.TryParse(command[1].ToUpper(), out EDatabase newDatabase))
            {
                Logger.LogError("[WarnSystem] Failed to Parse Database Types");
                return;
            }

            if (oldDatabase == newDatabase)
            {
                Logger.LogError("[WarnSystem] Old Database and New Database can't be the Same!");
                return;
            }

            if (newDatabase != WarnSystem.DatabaseSystem)
            {
                Logger.LogError("[WarnSystem] New Database must be the same as the Current Database Type!");
                return;
            }

            if (WarnSystem.Instance.Data == null)
            {
                Logger.LogError("[WarnSystem] Database hasn't been Loaded!");
                return;
            }

            Logger.Log($"Reading from {oldDatabase} Database..");

            List<WarnGroup> Data = new List<WarnGroup>();
            if (oldDatabase == EDatabase.JSON)
            {
                var jsonData = WarnSystem.Instance.JsonDatabase.ReadData();
                if (jsonData != null)
                {
                    Data = jsonData;
                }
                
            } else if (oldDatabase == EDatabase.MYSQL)
            {
                var sqlData = WarnSystem.Instance.SQLDatabase.ReadData();
                if (sqlData != null)
                {
                    Data = sqlData;
                }
            }

            Logger.Log($"Saving to {newDatabase} Database..");

            if (newDatabase == EDatabase.JSON)
            {
                WarnSystem.Instance.JsonDatabase.Reload();
                WarnSystem.Instance.JsonDatabase.SetData(Data);
                WarnSystem.Instance.JsonDatabase.SaveData();
            } else if (newDatabase == EDatabase.MYSQL)
            {
                WarnSystem.Instance.SQLDatabase.ReloadNonAsync();
                WarnSystem.Instance.SQLDatabase.SetData(Data);
                WarnSystem.Instance.SQLDatabase.SaveData();
            }

            Logger.Log($"Successfully Migrated from {oldDatabase} to {newDatabase}!");
        }
    }
}
