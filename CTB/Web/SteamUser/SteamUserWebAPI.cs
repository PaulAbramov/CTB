using System;
using System.Collections.Specialized;
using CTB.Web.JsonClasses;
using CTB.Web.TradeOffer;
using Newtonsoft.Json;
using SteamKit2;

namespace CTB.Web.SteamUser
{
    public class SteamUserWebAPI
    {
        private const string steamUserInterface = "ISteamUser";

        private readonly SteamWeb m_steamWeb;

        /// <summary>
        /// Constructor to initialize variables
        /// </summary>
        /// <param name="_steamWeb"></param>
        public SteamUserWebAPI(SteamWeb _steamWeb)
        {
            m_steamWeb = _steamWeb;
        }

        /// <summary>
        /// Get the profilesummaries for the given steamIDs
        /// </summary>
        /// <param name="_steamIDs"></param>
        /// <returns></returns>
        public APIResponse<GetPlayerSummariesResponse> GetPlayerSummaries(SteamID[] _steamIDs)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", m_steamWeb.m_APIKey},
                {"steamids", string.Join(",", Array.ConvertAll(_steamIDs, _steamID => $"{_steamID.ConvertToUInt64()}"))}
            };

            string url = string.Format(m_steamWeb.m_APISteamAddress, steamUserInterface, "GetPlayerSummaries", "v0002");

            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(url, data);

            APIResponse<GetPlayerSummariesResponse> summary = JsonConvert.DeserializeObject<APIResponse<GetPlayerSummariesResponse>>(response);

            return summary;
        }

        /// <summary>
        /// Get the IDs of the groups the user is in
        /// </summary>
        /// <param name="_steamID"></param>
        /// <returns></returns>
        public APIResponse<GetPlayerGroupListResponse> GetUserGroupList(SteamID _steamID)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", m_steamWeb.m_APIKey},
                {"steamid", _steamID.ConvertToUInt64().ToString()}
            };

            string url = string.Format(m_steamWeb.m_APISteamAddress, steamUserInterface, "GetUserGroupList", "v0001");

            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(url, data);

            APIResponse<GetPlayerGroupListResponse> summary = JsonConvert.DeserializeObject<APIResponse<GetPlayerGroupListResponse>>(response);

            return summary;
        }

        /// <summary>
        /// Invite a user to a steamgroup
        /// Conver the steamID into an array shown in a string
        /// 
        /// Set the needed data into the NameValueCollection
        /// Post the data to the groupInviteURL
        /// </summary>
        /// <param name="_groupID"></param>
        /// <param name="_steamID"></param>
        public void InviteToGroup(string _groupID, string _steamID)
        {
            string steamIDFormattedIntoArray = $"[\"{_steamID}\"]";

            NameValueCollection data = new NameValueCollection
            {
                {"json", "1"},
                {"type", "groupInvite"},
                {"group", _groupID},
                {"sessionID", m_steamWeb.SessionID},
                {"invitee_list", steamIDFormattedIntoArray}
            };

            const string groupInviteURL = "https://steamcommunity.com/actions/GroupInvite";

            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(groupInviteURL, data, false);
        }
    }
}
