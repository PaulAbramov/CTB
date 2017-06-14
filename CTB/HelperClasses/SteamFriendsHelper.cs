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
        /// Invite the user to our group and the group passed by the admin
        /// Allowed string passed by admin: group url, groupID32 and groupID64, groupID64 is prefered because of error measures
        /// If a url is passed, so get the groupID64 from the grouppage
        /// groupID32 will be converted into a groupID64
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

                if (!string.IsNullOrEmpty(_groupID))
                {
                    string groupID64 = "";
                    if (_groupID.Contains("steamcommunity") && _groupID.Contains("groups"))
                    {
                        groupID64 = _steamUserWebAPI.GetGroupIDFromGroupAdress(_groupID);
                    }
                    else
                    {
                        groupID64 = GetGroupID64String(_groupID);
                    }

                    _steamUserWebAPI.InviteToGroup(groupID64, _friendSteamID.ConvertToUInt64().ToString());
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

        /// <summary>
        /// We want to try and parse the string into an unsigned long value
        /// Ulong because it might be a ID64 which would be too big for an int and it is always used as an ulong inside steam
        /// If it parsed without a problem it is a numeric
        /// Which allows us to get the amount of digits in this number
        /// If the number is equal to 18 it is a groupID64, so we just want to return it as a string
        /// If the number doesn't equal to 18 it should be a groupID32, so we want to get the groupID64
        /// Return the converted groupID64 as a sting
        /// </summary>
        /// <param name="_groupID"></param>
        /// <returns></returns>
        private string GetGroupID64String(string _groupID)
        {
            ulong groupID64Or32;
            bool isNumeric = ulong.TryParse(_groupID, out groupID64Or32);

            if (isNumeric)
            {
                //  Safe way be cause we can the amount of digits in a number without casting it to a string
                //  var amountOfDigits = GetAmountOfDigits(groupID64Or32);

                //  We have checked if it is a numeric, so we can just get the digit amount of the string we have parsed
                if (_groupID.Length == 18)
                {
                    return groupID64Or32.ToString();
                }
                else
                {
                    SteamID groupID64SteamID = GetGroupID(Convert.ToUInt32(groupID64Or32));
                    return groupID64SteamID.ConvertToUInt64().ToString();
                }
            }

            return "";
        }

        /// <summary>
        /// SAFE WAY TO GET A DIGITAMOUNT OF A NUMBER
        /// BECAUSE WE ARE NOT CASTING TO A STRING
        /// 
        /// We Want to get the amount of digits in the number
        /// 
        /// Therefore we are going to use logarithm of 10 on the passed number
        /// 
        /// our groupID64: 103582791458407475
        /// 
        /// 10                        1 time
        /// 100                       2 times
        /// 1000                      3 times
        /// 10000                     4 times
        /// 100000                    5 times
        /// 1000000                   6 times
        /// 10000000                  7 times
        /// 100000000                 8 times
        /// 1000000000                9 times
        /// 10000000000               10times
        /// 100000000000              11times
        /// 1000000000000             12times
        /// 10000000000000            13times
        /// 100000000000000           14times
        /// 1000000000000000          15times
        /// 10000000000000000         16times
        /// 100000000000000000        17times
        /// 103582791458407475
        /// 
        /// So the result will be 17, but we started with a 10, which includes 2 digits
        /// Therefore we are going to increase the result by one so we get 18 digits
        /// GroupID64 and SteamID64 are always 18 digits long
        /// 
        /// Because we are using log10 on our groupID64 our result will be a floating number like 18,01528...
        /// Use Math.floor to round the result to 18, cast it to an unsigned it and return it
        /// </summary>
        /// <param name="_number"></param>
        /// <returns></returns>
        private static uint GetAmountOfDigits(ulong _number)
        {
            return Convert.ToUInt32(Math.Floor(Math.Log10(_number) + 1));
        }
    }
}
