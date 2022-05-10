using System;
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

        public void AddWarn(Warn warn)
        {
            var WarnGroup = Data.FirstOrDefault(x => x.SteamID == warn.owner);
            if (WarnGroup == null)
            {
                WarnGroup = new WarnGroup
                {
                    SteamID = warn.owner,
                    Warnings = new List<Warn>() { warn }
                };
                Data.Add(WarnGroup);
                return;
            }

            WarnGroup.Warnings.Add(warn);
        }

        public void RemoveWarn(int index, WarnGroup WarnGroup)
        {
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

        public void ClearWarns(WarnGroup WarnGroup)
        {
            if (WarnGroup == null)
            {
                Logger.LogError("[WarnSystem] Failed to Clear Warnings, Player Does not have any Warnings!");
                return;
            }

            Data.Remove(WarnGroup);
        }

        public void SetSaveData(List<WarnGroup> newData)
        {
            Data = newData;
            DataStorage.Save(Data);
        }
    }
}
