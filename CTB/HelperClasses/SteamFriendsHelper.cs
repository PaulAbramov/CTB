using System.Linq;
using SteamKit2;

namespace CTB.HelperClasses
{
    /// <summary>
    /// Helperclass for functions to ease the use of functions to interact with other persons on Steam
    /// </summary>
    public class SteamFriendsHelper
    {
        /// <summary>
        /// Converts a accountid, which is common in tradeoffers into a usable SteamID
        /// </summary>
        /// <param name="_accountid"></param>
        /// <returns></returns>
        public SteamID GetSteamID(int _accountid)
        {
            string id32 = $"STEAM_0:{_accountid & 1}:{_accountid >> 1}";
            return new SteamID(id32).ConvertToUInt64();
        }

        /// <summary>
        /// Check if the user entered an admin ID in int64 and compare it with the user which interacts with the bot right now
        /// Give back true if the user is the admin, else false
        /// </summary>
        /// <param name="_steamID"></param>
        /// <param name="_admins"></param>
        /// <returns></returns>
        public bool IsBotAdmin(SteamID _steamID, string[] _admins)
        {
            return _admins.Length > 0 && _admins.Any(_admin => _admin == _steamID.ConvertToUInt64().ToString());
        }
    }
}
