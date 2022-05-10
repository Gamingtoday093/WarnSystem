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

        public void RegisterWarn(ulong SteamID64, ulong ModeratorSteamID64, string Reason)
        {
            var Warn = new Warn()
            {
                owner = SteamID64,
                moderatorSteamID64 = ModeratorSteamID64,
                dateTime = DateTimeOffset.Now,
                reason = Reason
            };

            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.AddWarn(Warn);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.AddWarn(Warn);
        }

        public void RemoveWarn(int index, WarnGroup warnGroup)
        {
            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.RemoveWarn(index, warnGroup);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.RemoveWarn(index, warnGroup);
        }

        public void ClearWarns(WarnGroup warnGroup)
        {
            if (WarnSystem.DatabaseSystem == EDatabase.JSON) JsonDatabase.ClearWarns(warnGroup);
            if (WarnSystem.DatabaseSystem == EDatabase.MYSQL) SQLDatabase.ClearWarns(warnGroup);
        }
    }
}
