using Steamworks;

namespace WarnSystem.Services
{
    public class getValidCSteamIDService
    {
        public static CSteamID getValidCSteamID(string CSteamIDIn)
        {
            if (!ulong.TryParse(CSteamIDIn, out ulong possibleCSteamID)) return CSteamID.Nil;
            if (!((CSteamID)possibleCSteamID).IsValid()) return CSteamID.Nil;
            return new CSteamID(possibleCSteamID);
        }
    }
}
