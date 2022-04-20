﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnSystem.Models;
using WarnSystem.Storage;
using Rocket.Core.Logging;

namespace WarnSystem.Database
{
    public class JsonDatabase
    {
        private DataStorage<List<WarnGroup>> DataStorage { get; set; }

        public List<WarnGroup> Data { get; private set; }

        public JsonDatabase()
        {
            DataStorage = new DataStorage<List<WarnGroup>>(WarnSystem.Instance.Directory, "Warns.json");
        }

        public List<WarnGroup> ReadData()
        {
            return DataStorage.Read();
        }

        public void SaveData()
        {
            DataStorage.Save(Data);
        }

        public void Reload()
        {
            Data = DataStorage.Read();
            if (Data == null)
            {
                Data = new List<WarnGroup>();
                DataStorage.Save(Data);
            }
        }

        public void AddWarn(ulong SteamID64, Warn warn)
        {
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
            var WarnGroup = Data.FirstOrDefault(x => x.CSteamID64 == SteamID64);
            if (WarnGroup == null)
            {
                Logger.LogError("[WarnSystem] Failed to Remove Warning, Player Does not have any Warnings!");
                return;
            }

            WarnGroup.Warnings.RemoveAt(index);
            if(WarnGroup.Warnings.Count == 0)
            {
                Data.Remove(WarnGroup);
            }
        }

        public void ClearWarns(ulong SteamID64)
        {
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
            DataStorage.Save(Data);
        }
    }
}
