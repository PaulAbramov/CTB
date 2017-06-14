/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using System;
using System.IO;
using System.Linq;
using CTB.JsonClasses;
using CTB.Web.JsonClasses;
using CTB.Web.SteamUserWeb;
using CTB.Web.TradeOffer;
using Newtonsoft.Json;
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
            return new SteamID(id32);
        }

        /// <summary>
        /// Gets the SteamID for the short groupID
        /// We have to set this steamID to public and to a clan, so we get the right SteamID
        /// </summary>
        /// <param name="_groupID"></param>
        /// <returns></returns>
        public SteamID GetGroupID(uint _groupID)
        {
            return new SteamID(_groupID, EUniverse.Public, EAccountType.Clan);
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

        /// <summary>
        /// Change the permission to acceptfriendrequests in the config file for uses after a relogin
        /// Give the admin a message so he knows everything worked out like it should
        /// </summary>
        /// <param name="_steamFriends"></param>
        /// <param name="_callback"></param>
        /// <param name="_acceptFriendRequests"></param>
        /// <returns></returns>
        public bool SetPermissionAcceptFriendRequests(SteamFriends _steamFriends, SteamFriends.FriendMsgCallback _callback, bool _acceptFriendRequests)
        {
            if(_acceptFriendRequests)
            {
                OverrideConfigAcceptFrienRequests(false);

                _steamFriends.SendChatMessage(_callback.Sender, EChatEntryType.ChatMsg, "Not accepting friendrequests anymore.");
                return false;
            }
            else
            {
                OverrideConfigAcceptFrienRequests(true);

                _steamFriends.SendChatMessage(_callback.Sender, EChatEntryType.ChatMsg, "Accepting friendrequests again.");
                return true;
            }
        }

        /// <summary>
        /// Load the config to not loose information
        /// Set the bool to true or false in the config
        /// Save the config with all information
        /// </summary>
        /// <param name="_acceptFriendRequests"></param>
        private static void OverrideConfigAcceptFrienRequests(bool _acceptFriendRequests)
        {
            BotInfo botInfo = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText("Files/config.json"));
            botInfo.AcceptFriendRequests = _acceptFriendRequests;
            File.WriteAllText("Files/config.json", JsonConvert.SerializeObject(botInfo, Formatting.Indented));
        }

        /// <summary>
        /// Add the user to the friendslist
        /// Welcome the user to the service
        /// </summary>
        /// <param name="_steamFriends"></param>
        /// <param name="_friendSteamID"></param>
        public void AcceptFriendRequest(SteamFriends _steamFriends, SteamID _friendSteamID)
        {
            _steamFriends.AddFriend(_friendSteamID);

            _steamFriends.SendChatMessage(_friendSteamID, EChatEntryType.ChatMsg, "Hello and welcome to my Service!");
        }

        /// <summary>
        /// Add the user to the friendslist
        /// Invite the user to our group and the group passed by the user
        /// Welcome the user to the service and tell him we invited him to our Group
        /// </summary>
        /// <param name="_steamFriends"></param>
        /// <param name="_friendSteamID"></param>
        /// <param name="_groupID"></param>
        /// <param name="_steamUserWebAPI"></param>
        public void AcceptFriendRequestAndInviteToGroup(SteamFriends _steamFriends, SteamID _friendSteamID, SteamUserWebAPI _steamUserWebAPI, string _groupID = "")
        {
            _steamFriends.AddFriend(_friendSteamID);

            for(int i = 0; i < _steamFriends.GetClanCount(); i++)
            {
                SteamID groupID = _steamFriends.GetClanByIndex(i);
                if(groupID.ConvertToUInt64().Equals(103582791458407475) && _steamFriends.GetClanName(groupID).ToUpper().Contains("XETAS"))
                {
                    _steamUserWebAPI.InviteToGroup(groupID.ToString(), _friendSteamID.ConvertToUInt64().ToString());
                }

                if(!String.IsNullOrEmpty(_groupID))
                {
                    //TODO check if the ID is valid, maybe convert it and invite the user to this group
                }
            }

            _steamFriends.SendChatMessage(_friendSteamID, EChatEntryType.ChatMsg, "Hello and welcome to my Service!\nI've invited you to my group, where you can check the other bots or get to learn and trade with other steamusers.");
        }

        /// <summary>
        /// Remove the user from our list
        /// Decline the friendsrequest
        /// </summary>
        /// <param name="_steamFriends"></param>
        /// <param name="_friendID"></param>
        public void DeclineFriendRequest(SteamFriends _steamFriends, SteamID _friendID)
        {
            _steamFriends.RemoveFriend(_friendID);
        }
    }
}
