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
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTB.Web.SteamStoreWebAPI.JsonClasses;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CTB.Web.SteamStoreWebAPI
{
    public class SteamStoreWebAPI
    {
        /// <summary>
        /// Get the amount of cards we can farm
        /// We need to clear one discoveryqueue for one card, which contains 12 AppID's
        /// 
        /// For every card generate a new discoveryqueue and clear every appid in this queue
        /// </summary>
        public async Task<string> ExploreDiscoveryQueues()
        {
            int cardsToEarn = 0;
            string responseToAdmin = "";

            await Task.Run(async () =>
            {
                if(!await SteamWeb.Instance.RefreshSessionIfNeeded())
                {
                    responseToAdmin = "Could not reauthenticate.";
                }

                cardsToEarn = await GetCardsToEarnFromDiscoveryQueue();

                if (cardsToEarn != 0)
                {
                    for (int i = 0; i < cardsToEarn; i++)
                    {
                        RequestNewDiscoveryQueueResponse discoveryQueue = await GenerateNewDiscoveryQueue();

                        foreach (uint appID in discoveryQueue.Queue)
                        {
                            string urlToApp = $"http://{SteamWeb.Instance.m_SteamStoreHost}/app/{appID}";

                            NameValueCollection data = new NameValueCollection()
                            {
                                {"sessionid", SteamWeb.Instance.SessionID},
                                {"appid_to_clear_from_queue", appID.ToString()}
                            };

                            string response = await WebHelper.Instance.GetStringFromRequest(urlToApp, data, false);
                        }
                    }
                }
            });

            if(string.IsNullOrEmpty(responseToAdmin))
            {
                responseToAdmin = cardsToEarn == 0 ? "There were no cards to earn from discoveryqueues" : $"Successfully explored {cardsToEarn} discoveryqueues";
            }

            return responseToAdmin;
        }

        /// <summary>
        /// Check the explore page if we can receive any cards from discoveryqueues
        /// The string is inside the class "subtext"
        /// Check the string if it starts with a specific substring 
        /// If so, get the digit out of the string and convert it to an int and return it
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetCardsToEarnFromDiscoveryQueue()
        {
            string url = $"http://{SteamWeb.Instance.m_SteamStoreHost}/explore?l=english";

            string response = await WebHelper.Instance.GetStringFromRequest(url);

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(response);

            HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@class='subtext']");

            if(node == null)
            {
                return 0;
            }

            bool cardsAvailable = node.InnerText.StartsWith("You can get");

            if(cardsAvailable)
            {
                Match cardAmount = Regex.Match(node.InnerText, @"\d");

                if(cardAmount.Success)
                {
                    return Convert.ToInt32(cardAmount.Value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Make a post request to an url to get a new discoveryqueue which we can work with
        /// </summary>
        /// <returns></returns>
        private async Task<RequestNewDiscoveryQueueResponse> GenerateNewDiscoveryQueue()
        {
            string url = $"http://{SteamWeb.Instance.m_SteamStoreHost}/explore/generatenewdiscoveryqueue";

            NameValueCollection data = new NameValueCollection()
                {
                    {"sessionid", SteamWeb.Instance.SessionID},
                    {"queuetype", "0"}
                };

            string stringResponse = await WebHelper.Instance.GetStringFromRequest(url, data, false);

            RequestNewDiscoveryQueueResponse response = JsonConvert.DeserializeObject<RequestNewDiscoveryQueueResponse>(stringResponse);

            return response;
        }
    }
}