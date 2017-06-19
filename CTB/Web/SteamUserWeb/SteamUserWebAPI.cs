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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml;
using CTB.Web.JsonClasses;
using CTB.Web.SteamUserWeb.JsonClasses;
using CTB.Web.TradeOffer;
using HtmlAgilityPack;
using Newtonsoft.Json;
using SteamKit2;

namespace CTB.Web.SteamUserWeb
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
                {"key", m_steamWeb.APIKey},
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
                {"key", m_steamWeb.APIKey},
                {"steamid", _steamID.ConvertToUInt64().ToString()}
            };

            string url = string.Format(m_steamWeb.m_APISteamAddress, steamUserInterface, "GetUserGroupList", "v0001");

            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(url, data);

            APIResponse<GetPlayerGroupListResponse> playerGroupList = JsonConvert.DeserializeObject<APIResponse<GetPlayerGroupListResponse>>(response);

            return playerGroupList;
        }

        /// <summary>
        /// Extend the groupurl by the string to get a XML overview of the groupdata
        /// Parse the content into a XMLDocument
        /// Select the root node
        /// Get the Text from the first child node which is the groupID64 and return it
        /// </summary>
        /// <param name="_url"></param>
        /// <returns></returns>
        public string GetGroupIDFromGroupAdress(string _url)
        {
            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(_url + "/memberslistxml/?xml=1");

            XmlDocument groupXML = new XmlDocument();
            groupXML.LoadXml(response);

            XmlNode groupID64Node = groupXML.SelectSingleNode("/memberList");

            return groupID64Node?.FirstChild.InnerText;
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

            m_steamWeb.m_WebHelper.SendWebRequest(groupInviteURL, data, false);
        }

        /// <summary>
        /// Join a specific group with the passed groupID as a string
        /// </summary>
        /// <param name="_groupID"></param>
        public void JoinGroup(string _groupID)
        {
            string url = $"https://{m_steamWeb.m_SteamCommunityHost}/gid/{_groupID}";

            NameValueCollection data = new NameValueCollection()
            {
                {"sessionID", m_steamWeb.SessionID},
                {"action", "join"}
            };

            m_steamWeb.m_WebHelper.SendWebRequest(url, data, false);
        }

        /// <summary>
        /// We do not want to send unnecessary webrequests
        /// So we do want to check if we are already a member of this group
        /// Therefore we want to loop trough all of our groups and check the groupID
        /// If we are already a member just leave the function
        /// If we are not a member join the group
        /// </summary>
        /// <param name="_steamFriends"></param>
        /// <param name="_groupID"></param>
        public void JoinGroupIfNotJoinedAlready(SteamFriends _steamFriends, ulong _groupID)
        {
            bool alreadyJoined = false;

            for (int i = 0; i < _steamFriends.GetClanCount(); i++)
            {
                SteamID groupID = _steamFriends.GetClanByIndex(i);
                if (groupID.ConvertToUInt64().Equals(_groupID))
                {
                    alreadyJoined = true;
                    break;
                }
            }

            if(!alreadyJoined)
            {
                JoinGroup(_groupID.ToString());
            }
        }

        /// <summary>
        /// Make a webrequest to the badge we want to farm
        /// Load it into a htmldocument so we can analyze it easier
        /// Call "GetCardAmountToEarn" to readout the amount of cards to earn from the htmldocument
        /// </summary>
        /// <param name="_appID"></param>
        /// <returns></returns>
        public int GetGameCardsRemainingForGame(uint _appID)
        {
            string url = "http://" + m_steamWeb.m_SteamCommunityHost + "/my/gamecards/" + _appID;

            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(url);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            return GetCardAmountToEarn(htmlDocument.DocumentNode);
        }

        /// <summary>
        /// Make a webrequest to our badge page and parse the response into a htmldocument to analyze the response
        /// Get all Nodes with the class 'badge_title_stats_content' tagged, every node holds one Badge which is displayed
        /// Loop trough every Node and check if it is a node we can farm and add it to the list of games to farm  we are going to return
        /// </summary>
        /// <returns></returns>
        public List<GameToFarm> GetBadgesToFarm()
        {
            string url = "http://" + m_steamWeb.m_SteamCommunityHost + "/my/badges/?p=1" ;
            // TODO Check the other sites too
            string response = m_steamWeb.m_WebHelper.GetStringFromRequest(url);

            List<GameToFarm> gamesToFarm = new List<GameToFarm>();

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='badge_title_stats_content']");

            if(nodes == null)
            {
                return gamesToFarm;
            }

            foreach(HtmlNode node in nodes)
            {
                string appID = GetBadgeAppID(node);

                //  If the string is empty it is a badge we can not farm, so just go to the next one
                if(string.IsNullOrEmpty(appID))
                {
                    continue;
                }

                int cardsToEarn = GetCardAmountToEarn(node);

                int cardsEarned = GetCardAmountEarned(node);

                string hoursString = GetBadgeFarmedTime(node);

                string name = GetBadgeName(node);

                if(cardsToEarn > 0)
                {
                    gamesToFarm.Add(new GameToFarm { AppID = appID, CardsToEarn = cardsToEarn, CardsEarned = cardsEarned, Name = name, HoursPlayed = hoursString});
                }
            }

            return gamesToFarm;
        }

        /// <summary>
        /// Get the Node with the class "progress_info_bold" tagged, which holds the info what the AppID is
        /// If the Node is null then it is just a badge, not anything we can farm, return an empty string so we know we do not have to handle it
        /// If the Node is not null then get the ID attribute of the Node 
        /// It contains a string of multiple substrings connected by a '_', where the AppID is the 4th substring
        /// </summary>
        /// <param name="_htmlNode"></param>
        /// <returns></returns>
        private string GetBadgeAppID(HtmlNode _htmlNode)
        {
            HtmlNode appIDNode = _htmlNode.SelectSingleNode(".//div[@class='card_drop_info_dialog']");

            if (appIDNode == null)
            {
                return "";
            }

            return appIDNode.Id.Split('_')[4];
        }

        /// <summary>
        /// Get the Node with the class "progress_info_bold" tagged, which holds the info how much cards we can earn
        /// Parse the text into a string and check the string if it holds any digit
        /// If it holds a digit this is the amount of cards we can earn, parse it into an integer and return it
        /// If it does not hold a digit we can't farm more cards from the badge
        /// </summary>
        /// <param name="_htmlNode"></param>
        /// <returns></returns>
        private int GetCardAmountToEarn(HtmlNode _htmlNode)
        {
            HtmlNode progressNode = _htmlNode.SelectSingleNode(".//span[@class='progress_info_bold']");

            string progressText = progressNode?.InnerText;

            if (progressText == null)
            {
                return 0;
            }

            Match cardsToEarnMatch = Regex.Match(progressText, @"\d+");

            if (cardsToEarnMatch.Success)
            {
                return Convert.ToInt32(cardsToEarnMatch.Value);
            }

            return 0;
        }

        /// <summary>
        /// Get the Node with the class "card_drop_info_header" tagged, which holds the info how much cards we have earned
        /// Parse the text into a string and check the string if it holds any digit
        /// If it holds a digit this is the amount of cards we can earn, parse it into an integer and return it
        /// </summary>
        /// <param name="_htmlNode"></param>
        /// <returns></returns>
        private int GetCardAmountEarned(HtmlNode _htmlNode)
        {
            HtmlNode cardsEarnedNode = _htmlNode.SelectSingleNode(".//div[@class='card_drop_info_header']");

            Match cardsEarnedMatch = Regex.Match(cardsEarnedNode.InnerText, @"\d+");

            int cardsEarned = 0;

            if (cardsEarnedMatch.Success)
            {
                cardsEarned = Convert.ToInt32(cardsEarnedMatch.Value);
            }

            return cardsEarned;
        }

        /// <summary>
        /// Get the Node with the class "badge_title_stats_playtime" tagged, which holds the info how long we have farmed this badge already
        /// Check the string if it holds a substring like : "2.2 hrs on record"
        /// If we have found one, return this string, else return an empty string
        /// </summary>
        /// <param name="_htmlNode"></param>
        /// <returns></returns>
        private string GetBadgeFarmedTime(HtmlNode _htmlNode)
        {
            HtmlNode timeNode = _htmlNode.SelectSingleNode(".//div[@class='badge_title_stats_playtime']");

            Match hoursPlayedMatch = Regex.Match(timeNode.InnerText, @"\d.\d hrs on record");

            return hoursPlayedMatch.Success ? hoursPlayedMatch.Value : "";
        }

        /// <summary>
        /// Get the last Node with the class "card_drop_info_body" tagged, which holds the info what the name of the Badge is
        /// Parse the whole sentence into a string and get the index of the words right before the name of the game
        /// Also get the index of the dot at the end of the sentence.
        /// Now get the substring from the sentence which begins at the words right before the name of the game (which are always the same)
        /// The length of the name is calculated by the index of the dot at the end of the sentence minus the index of the substring before the name of the game
        /// </summary>
        /// <param name="_htmlNode"></param>
        /// <returns></returns>
        private string GetBadgeName(HtmlNode _htmlNode)
        {
            HtmlNode nameNode = _htmlNode.SelectSingleNode("(.//div[@class='card_drop_info_body'])[last()]");

            string name = nameNode.InnerText;

            int nameStartIndex = name.IndexOf(" by playing ", StringComparison.Ordinal);

            if (nameStartIndex <= 0)
            {
                nameStartIndex = name.IndexOf("You don't have any more drops remaining for ", StringComparison.Ordinal);

                nameStartIndex += 32;
            }

            nameStartIndex += 12;

            int nameEndIndex = name.LastIndexOf('.');

            name = name.Substring(nameStartIndex, nameEndIndex - nameStartIndex);

            return name;
        }
    }
}
