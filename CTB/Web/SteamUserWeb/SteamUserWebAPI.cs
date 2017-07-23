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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        /// <summary>
        /// Get the profilesummaries for the given steamIDs
        /// </summary>
        /// <param name="_steamIDs"></param>
        /// <returns></returns>
        public async Task<APIResponse<GetPlayerSummariesResponse>> GetPlayerSummaries(SteamID[] _steamIDs)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", SteamWeb.Instance.APIKey},
                {"steamids", string.Join(",", Array.ConvertAll(_steamIDs, _steamID => $"{_steamID.ConvertToUInt64()}"))}
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, steamUserInterface, "GetPlayerSummaries", "v0002");

            string response = await WebHelper.Instance.GetStringFromRequest(url, data);

            APIResponse<GetPlayerSummariesResponse> summary = JsonConvert.DeserializeObject<APIResponse<GetPlayerSummariesResponse>>(response);

            return summary;
        }

        /// <summary>
        /// Get the IDs of the groups the user is in
        /// </summary>
        /// <param name="_steamID"></param>
        /// <returns></returns>
        public async Task<APIResponse<GetPlayerGroupListResponse>> GetUserGroupList(SteamID _steamID)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", SteamWeb.Instance.APIKey},
                {"steamid", _steamID.ConvertToUInt64().ToString()}
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, steamUserInterface, "GetUserGroupList", "v0001");

            string response = await WebHelper.Instance.GetStringFromRequest(url, data);

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
        public async Task<string> GetGroupIDFromGroupAdress(string _url)
        {
            string response = await WebHelper.Instance.GetStringFromRequest($"{_url}/memberslistxml/?xml=1");

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
        public async Task InviteToGroup(string _groupID, string _steamID)
        {
            string steamIDFormattedIntoArray = $"[\"{_steamID}\"]";

            NameValueCollection data = new NameValueCollection
            {
                {"json", "1"},
                {"type", "groupInvite"},
                {"group", _groupID},
                {"sessionID", SteamWeb.Instance.SessionID},
                {"invitee_list", steamIDFormattedIntoArray}
            };

            const string groupInviteURL = "https://steamcommunity.com/actions/GroupInvite";

            await WebHelper.Instance.SendWebRequest(groupInviteURL, data, false);
        }

        /// <summary>
        /// Join a specific group with the passed groupID as a string
        /// </summary>
        /// <param name="_groupID"></param>
        public async Task JoinGroup(string _groupID)
        {
            string url = $"https://{SteamWeb.Instance.m_SteamCommunityHost}/gid/{_groupID}";

            NameValueCollection data = new NameValueCollection()
            {
                {"sessionID", SteamWeb.Instance.SessionID},
                {"action", "join"}
            };

            await WebHelper.Instance.SendWebRequest(url, data, false);
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
        public async Task JoinGroupIfNotJoinedAlready(SteamFriends _steamFriends, ulong _groupID)
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
                await JoinGroup(_groupID.ToString());
            }
        }

        /// <summary>
        /// Make a webrequest to the badge we want to farm
        /// Load it into a htmldocument so we can analyze it easier
        /// Call "GetCardAmountToEarn" to readout the amount of cards to earn from the htmldocument
        /// </summary>
        /// <param name="_appID"></param>
        /// <returns></returns>
        public async Task<int> GetGameCardsRemainingForGame(uint _appID)
        {
            string url = $"http://{SteamWeb.Instance.m_SteamCommunityHost}/my/gamecards/{_appID}";

            string response = await WebHelper.Instance.GetStringFromRequest(url);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            return GetCardAmountToEarn(htmlDocument.DocumentNode);
        }

        /// <summary>
        /// Make a webrequest to our badge page and parse the response into a htmldocument to analyze the response
        /// Get the last node with the class "pagelink"
        /// If there is one, parse it into a string and overwrite the pages variable
        /// For the amount of sites get the list of games which we can farm and return the list
        /// 
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<GameToFarm>> GetBadgesToFarm()
        {
            string url = $"http://{SteamWeb.Instance.m_SteamCommunityHost}/my/badges/?p=1";

            string response = await WebHelper.Instance.GetStringFromRequest(url);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("(//a[@class='pagelink'])[last()]");

            int pages = 1;

            if(htmlNode != null)
            {
                string lastPage = htmlNode.InnerText;
                pages = Convert.ToInt32(lastPage);
            }

            List<GameToFarm> gamesToFarm = new List<GameToFarm>();

            for(int i = 0; i < pages; i++)
            {
                //TODO multiple same entries shouldn't make any problems, but make sure they don't
                gamesToFarm.AddRange(await GetBadgesToFarmFromSite(i));
            }

            return gamesToFarm;
        }

        /// <summary>
        /// Make a webrequest to our badge page and parse the response into a htmldocument to analyze the response
        /// Get all Nodes with the class 'badge_title_stats_content' tagged, every node holds one Badge which is displayed
        /// Loop trough every Node and check if it is a node we can farm and add it to the list of games to farm  we are going to return
        /// </summary>
        /// <param name="_site"></param>
        /// <returns></returns>
        private async Task<List<GameToFarm>> GetBadgesToFarmFromSite(int _site)
        {
            string url = $"http://{SteamWeb.Instance.m_SteamCommunityHost}/my/badges/?p={_site}";

            string response = await WebHelper.Instance.GetStringFromRequest(url);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='badge_title_stats_content']");

            List<GameToFarm> gamesToFarm = new List<GameToFarm>();
            if (nodes == null)
            {
                return gamesToFarm;
            }

            foreach (HtmlNode node in nodes)
            {
                string appID = GetBadgeAppID(node);

                //  If the string is empty it is a badge we can not farm, so just go to the next one
                if (string.IsNullOrEmpty(appID))
                {
                    continue;
                }

                int cardsToEarn = GetCardAmountToEarn(node);

                int cardsEarned = GetCardAmountEarned(node);

                string hoursString = GetBadgeFarmedTime(node);

                string name = GetBadgeName(node);

                if (cardsToEarn > 0)
                {
                    gamesToFarm.Add(new GameToFarm { AppID = appID, CardsToEarn = cardsToEarn, CardsEarned = cardsEarned, Name = name, HoursPlayed = hoursString });
                }
            }

            //  foreachloop into linq (alternative to the foreachloop)
            //  gamesToFarm.AddRange(from node in nodes let appID = GetBadgeAppID(node) where !string.IsNullOrEmpty(appID)
            //                     let cardsToEarn = GetCardAmountToEarn(node)
            //                     let cardsEarned = GetCardAmountEarned(node)
            //                     let hoursString = GetBadgeFarmedTime(node)
            //                     let name = GetBadgeName(node) where cardsToEarn > 0
            //                     select new GameToFarm {AppID = appID, CardsToEarn = cardsToEarn, CardsEarned = cardsEarned, Name = name, HoursPlayed = hoursString});

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

        /// <summary>
        /// We are generating our sessionID by converting our steamid to a byteArray and from there to a Base64String
        /// So to  get our steamID from the sessionID we want to reverse this
        /// Get our SteamInventory with the contextID 6 which is for cards, backgrounds and emoticons
        /// </summary>
        /// <returns></returns>
        public async Task<InventoryResponse> GetOurSteamInventory()
        {
            SteamID ourSteamID = new SteamID(Encoding.UTF8.GetString(Convert.FromBase64String(SteamWeb.Instance.SessionID)));

            return await GetInventory(ourSteamID.ConvertToUInt64(), 753, 6);
        }

        /// <summary>
        /// We are generating our sessionID by converting our steamid to a byteArray and from there to a Base64String
        /// So to  get our steamID from the sessionID we want to reverse this
        /// Get our CSGOInventory with the contextID 2 which is for skins, cases etc.
        /// </summary>
        /// <returns></returns>
        public async Task<InventoryResponse> GetOurCSGOInventory()
        {
            SteamID ourSteamID = new SteamID(Encoding.UTF8.GetString(Convert.FromBase64String(SteamWeb.Instance.SessionID)));

            return await GetInventory(ourSteamID.ConvertToUInt64(), 730, 2);
        }

        /// <summary>
        /// Create an url to the inventory with the steamID, appID and contextID
        /// Deserialize the response to an object so we can ease the work
        /// 
        /// If we have more than 5000 items, page the request with the extension "start_assetid" and the last assetid from the previous request
        /// Make another request, if there are more pages set the variable of the default object so we can make a new request
        /// Add the items and the descriptions to the default inventoryresponse object
        /// 
        /// Finally return the inventoryobject with all items from the inventory
        /// </summary>
        /// <param name="_steamID"></param>
        /// <param name="_appID"></param>
        /// <param name="_contextID"></param>
        /// <returns></returns>
        public async Task<InventoryResponse> GetInventory(ulong _steamID, int _appID, int _contextID)
        {
            try
            {
                string url = $"http://{SteamWeb.Instance.m_SteamCommunityHost}/inventory/{_steamID}/{_appID}/{_contextID}?l=english&trading=1&count=5000";

                string response = await WebHelper.Instance.GetStringFromRequest(url);

                InventoryResponse inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(response);

                while (inventoryResponse.More == 1)
                {
                    url += $"&start_assetid={inventoryResponse.LastAssetID}";

                    response = await WebHelper.Instance.GetStringFromRequest(url);

                    InventoryResponse inventoryResponseMore = JsonConvert.DeserializeObject<InventoryResponse>(response);

                    inventoryResponse.More = inventoryResponseMore.More;
                    inventoryResponse.LastAssetID = inventoryResponseMore.LastAssetID;

                    inventoryResponse.Assets.AddRange(inventoryResponseMore.Assets);
                    inventoryResponse.ItemsDescriptions.AddRange(inventoryResponseMore.ItemsDescriptions);
                }

                return inventoryResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetInventory : {e}");
            }
            
            return new InventoryResponse();
        }
    }
}