using Steamworks;

namespace WarnSystem.Events
{
    public delegate void OnWarnHandler(CSteamID targetSteamID, CSteamID moderatorSteamID, string reason);
}
