﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Models;
using MySql.Data.MySqlClient;
using WarnSystem.Storage;
using Rocket.Core.Logging;
using System.Threading;

namespace WarnSystem.Database
{
    public class SQLDatabase
    {
        private const string CreateTableQuery =
            "`Id` INT NOT NULL AUTO_INCREMENT, " +
            "`SteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`ModeratorSteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`DateTime` VARCHAR(28) NOT NULL," +
            "`Reason` TEXT NOT NULL DEFAULT '', " +
            "PRIMARY KEY (Id)";

        private SQLStorage<List<Warn>> SQLStorage { get; set; }
        public List<WarnGroup> Data { get; private set; }
        public SQLDatabase()
        {
            SQLStorage = new SQLStorage<List<Warn>>(WarnSystem.Config.MySQLConnectionString);
        }

        private List<WarnGroup> ConvertData(List<Warn> warns)
        {
            List<WarnGroup> convertedData = new List<WarnGroup>();

            foreach (Warn warn in warns)
            {
                WarnGroup WarnGroup = convertedData.FirstOrDefault(d => d.CSteamID64 == warn.owner);
                if (WarnGroup == null)
                {
                    WarnGroup = new WarnGroup()
                    {
                        CSteamID64 = warn.owner,
                        Warnings = new List<Warn>() { warn }
                    };
                    convertedData.Add(WarnGroup);
                    continue;
                }

                WarnGroup.Warnings.Add(warn);
            }
            return convertedData;
        }

        public List<WarnGroup> ReadData()
        {
            return ConvertData(SQLStorage.Read());
        }

        public void SaveData()
        {
            if (Data == null) return;
            List<Warn> Warns = Data.SelectMany(w => w.Warnings).ToList();
            List<Warn> ExistingWarns = SQLStorage.Read();

            foreach (Warn warn in Warns)
            {
                if (ExistingWarns.Any(w => w.owner == warn.owner && w.moderatorSteamID64 == warn.moderatorSteamID64 && w.dateTime == warn.dateTime && w.reason == warn.reason)) continue;
                ExistingWarns.Add(warn);
                Task.Run(async () => await SQLStorage.InsertAsync(warn));
            }
            foreach (Warn warn in ExistingWarns)
            {
                if (Warns.Any(w => w.owner == warn.owner && w.moderatorSteamID64 == warn.moderatorSteamID64 && w.dateTime == warn.dateTime && w.reason == warn.reason)) continue;
                Task.Run(async () => await SQLStorage.DeleteAsync(warn));
            }
        }

        public void Reload()
        {
            SQLStorage.CreateDatabase();
            SQLStorage.CreateTable(CreateTableQuery);
            Logger.Log("Loading MySQL Database..");
            ThreadPool.QueueUserWorkItem(async (_) => {
                var read = await SQLStorage.ReadAsync();
                Data = ConvertData(read);
                Logger.LogWarning("WarnSystem >> MySQL Database has been Loaded!");
            });
        }

        public void ReloadNonAsync()
        {
            SQLStorage.CreateDatabase();
            SQLStorage.CreateTable(CreateTableQuery);
            Data = ConvertData(SQLStorage.Read());
        }

        public void AddWarn(ulong SteamID64, Warn warn)
        {
            if (Data == null) return;
            var WarnGroup = Data.FirstOrDefault(x => x.CSteamID64 == SteamID64);
            if (WarnGroup == null)
            {
                WarnGroup = new WarnGroup
                {
                    CSteamID64 = SteamID64,
                    Warnings = new List<Warn>() { warn }
                };
                Data.Add(WarnGroup);
                return;
            }
            
            WarnGroup.Warnings.Add(warn);
        }

        public void RemoveWarn(ulong SteamID64, int index)
        {
            if (Data == null) return;
            var WarnGroup = Data.FirstOrDefault(x => x.CSteamID64 == SteamID64);
            if (WarnGroup == null)
            {
                Logger.LogError("[WarnSystem] Failed to Remove Warning, Player Does not have any Warnings!");
                return;
            }

            WarnGroup.Warnings.RemoveAt(index);
            if (WarnGroup.Warnings.Count == 0)
            {
                Data.Remove(WarnGroup);
            }
        }

        public void ClearWarns(ulong SteamID64)
        {
            if (Data == null) return;
            var WarnGroup = Data.FirstOrDefault(x => x.CSteamID64 == SteamID64);
            if (WarnGroup == null)
            {
                Logger.LogError("[WarnSystem] Failed to Clear Warnings, Player Does not have any Warnings!");
                return;
            }

            Data.Remove(WarnGroup);
        }

        public void SetData(List<WarnGroup> newData)
        {
            Data = newData;
        }
    }
}