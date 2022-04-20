using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WarnSystem.Database;
using WarnSystem.Models;

namespace WarnSystem.Services
{
    public class WarnService : MonoBehaviour
    {
        public JsonDatabase JsonDatabase => WarnSystem.Instance.JsonDatabase;
        public SQLDatabase SQLDatabase => WarnSystem.Instance.SQLDatabase;

        public void RegisterWarn(ulong CSteamID64, ulong ModeratorSteamID64, string Reason)
        {
            var Warn = new Warn()
            {
                owner = CSteamID64,
                moderatorSteamID64 = ModeratorSteamID64,
                dateTime = DateTimeOffset.Now,
                reason = Reason
            };

            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.AddWarn(CSteamID64, Warn);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.AddWarn(CSteamID64, Warn);
        }

        public void RemoveWarn(ulong cSteamID64, int index)
        {
            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.RemoveWarn(cSteamID64, index);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.RemoveWarn(cSteamID64, index);
        }

        public void ClearWarns(ulong cSteamID64)
        {
            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.ClearWarns(cSteamID64);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.ClearWarns(cSteamID64);
        }
    }
}
