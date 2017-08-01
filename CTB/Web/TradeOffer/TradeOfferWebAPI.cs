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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CTB.Web.TradeOffer.JsonClasses;
using Newtonsoft.Json;
using SteamKit2;

namespace CTB.Web.TradeOffer
{
    public class TradeOfferWebAPI
    {
        private const string IEconServiceInterface = "IEconService";

 #region WebCalls
        /// <summary>
        /// This is the actual call to get all tradeoffers we want to see
        /// Convert all bool values to int, and set the data into one string, afterwards call the API function to get all tradeoffers which show up these datas
        /// Get the response, deserialize and return it, so we can handle it
        /// 
        /// The user can specify every parameter here
        /// </summary>
        /// <param name="_getSentOffers"></param>
        /// <param name="_getReceivedOffers"></param>
        /// <param name="_getDescription"></param>
        /// <param name="_activeOnly"></param>
        /// <param name="_historicalOnly"></param>
        /// <param name="_historicalCutOff"></param>
        /// <param name="_language"></param>
        public async Task<GetOffersResponse> GetTradeOffers(bool _getSentOffers, bool _getReceivedOffers, bool _getDescription, bool _activeOnly, bool _historicalOnly, int _historicalCutOff = 1389106496, string _language = "en_us")
        {
            if (!_getSentOffers && !_getReceivedOffers)
            {
                throw new ArgumentException("getSentOffers and getReceivedOffers can't be both false, we do not get a response which we can handle");
            }

            NameValueCollection data = new NameValueCollection
            {
                {"key", SteamWeb.Instance.APIKey},
                {"get_sent_offers", BoolToInt(_getSentOffers).ToString()},
                {"get_received_offers", BoolToInt(_getReceivedOffers).ToString()},
                {"get_descriptions", BoolToInt(_getDescription).ToString()},
                {"language", _language},
                {"active_only", BoolToInt(_activeOnly).ToString()},
                {"historical_only", BoolToInt(_historicalOnly).ToString()},
                {"time_historical_cutoff", _historicalCutOff.ToString()}
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, IEconServiceInterface, "GetTradeOffers", "v1");

            try
            {
                string response = await WebHelper.Instance.GetStringFromRequest(url, data);
                APIResponse<GetOffersResponse> offersResponse = JsonConvert.DeserializeObject<APIResponse<GetOffersResponse>>(response);
                return offersResponse.Response;
            }
            catch (Exception)
            {
                return new GetOffersResponse();
            }
        }

#region Some functions to simplify the handling of the bot

        /// <summary>
        /// Get sent and received active tradeoffers
        /// </summary>
        /// <param name="_descriptionOfItems"></param>
        /// <returns></returns>
        public async Task<GetOffersResponse> GetAllActiveTradeOffers(bool _descriptionOfItems)
        {
            return await GetTradeOffers(true, true, _descriptionOfItems, true, false);
        }

        /// <summary>
        /// Get received active tradeoffers
        /// </summary>
        /// <param name="_descriptionOfItems"></param>
        /// <returns></returns>
        public async Task<GetOffersResponse> GetReceivedActiveTradeOffers(bool _descriptionOfItems)
        {
            return await GetTradeOffers(false, true, _descriptionOfItems, true, false);
        }

        /// <summary>
        /// Get sent active tradeoffers
        /// </summary>
        /// <param name="_descriptionOfItems"></param>
        /// <returns></returns>
        public async Task<GetOffersResponse> GetSentActiveTradeOffers(bool _descriptionOfItems)
        {
            return await GetTradeOffers(true, false, _descriptionOfItems, true, false);
        }
#endregion

        /// <summary>
        /// Get a single tradeoffer, but only if we do know the id of the trade
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <param name="_language"></param>
        /// <returns></returns>
        public async Task<GetOfferResponse> GetTradeOffer(string _tradeOfferID, string _language = "en_us")
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", SteamWeb.Instance.APIKey},
                {"tradeofferid", _tradeOfferID},
                {"language", _language}
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, IEconServiceInterface, "GetTradeOffer", "v1");

            try
            {
                string response = await WebHelper.Instance.GetStringFromRequest(url, data);

                APIResponse<GetOfferResponse> offerResponse = JsonConvert.DeserializeObject<APIResponse<GetOfferResponse>>(response);

                return offerResponse.Response;
            }
            catch (Exception)
            {
                return new GetOfferResponse();
            }
        }

        /// <summary>
        /// Get the amount of pending offers etc. so we know how much offers we have to handle
        /// </summary>
        /// <returns></returns>
        public async Task<TradeOffersSummaryResponse> GetTradeOffersSummary()
        {
            NameValueCollection data = new NameValueCollection
            {
                {"key", SteamWeb.Instance.APIKey}
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, IEconServiceInterface, "GetTradeOffersSummary", "v1");

            try
            {
                string response = await WebHelper.Instance.GetStringFromRequest(url, data);

                APIResponse<TradeOffersSummaryResponse> tradeOffersSummaryResponse = JsonConvert.DeserializeObject<APIResponse<TradeOffersSummaryResponse>>(response);

                return tradeOffersSummaryResponse.Response;
            }
            catch (Exception)
            {
                return new TradeOffersSummaryResponse();
            }
        }

        /// <summary>
        /// WebFunction, use it to modify your response/consoleoutput after getting this
        /// 
        /// Accept the tradeoffer with the given id
        /// To accept a trade we need the current sessionID which we have in our steamWebobject stored
        /// We have to pass a referer else we can't accept the tradeoffer
        /// Make the Post-Call and get the response from the web
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public async Task<bool> AcceptTradeOffer(string _tradeOfferID)
        {
            NameValueCollection data = new NameValueCollection
            {
                {"sessionid", SteamWeb.Instance.SessionID},
                {"serverid", "1"},
                {"tradeofferid", _tradeOfferID}
            };

            string referer = $"https://steamcommunity.com/tradeoffer/{_tradeOfferID}";

            string response = await WebHelper.Instance.GetStringFromRequest($"{referer}/accept", data, false, referer);

            TradeOfferAcceptResponse acceptResponse = JsonConvert.DeserializeObject<TradeOfferAcceptResponse>(response);

            if(acceptResponse != null && (acceptResponse.TradeID != null || acceptResponse.NeedsEmailConfirmation || acceptResponse.NeedsMobileConfirmation))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Simplified function, which also gives out a consoletext
        /// uses the webfunction internal
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <param name="_partnerID"></param>
        /// <returns></returns>
        public async Task<bool> AcceptTradeoffer(string _tradeOfferID, SteamID _partnerID)
        {
            bool acceptedOffer = await AcceptTradeOffer(_tradeOfferID);

            if (acceptedOffer)
            {
                Console.WriteLine($"Accepted the offer with the id {_tradeOfferID} from the user {_partnerID.ConvertToUInt64()}");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Simplified function, which also gives out a short consoletext
        /// uses the webfunction internal
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public async Task<bool> AcceptTradeofferShortMessage(string _tradeOfferID)
        {
            bool acceptedOffer = await AcceptTradeOffer(_tradeOfferID);

            if (acceptedOffer)
            {
                Console.WriteLine($"Accepted offer: {_tradeOfferID}");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// WebFunction, use it to modify your response/consoleoutput after getting this
        /// 
        /// Decline the tradeoffer with the given tradeOfferID
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public async Task<bool> DeclineTradeOffer(string _tradeOfferID)
        {
            NameValueCollection data = new NameValueCollection
            {
                { "key", SteamWeb.Instance.APIKey },
                { "tradeofferid", _tradeOfferID }
            };

            string url = string.Format(SteamWeb.Instance.m_APISteamAddress, IEconServiceInterface, "DeclineTradeOffer", "v1");

            string response = await WebHelper.Instance.GetStringFromRequest(url, data, false);

            return true;
        }

        /// <summary>
        /// Simplified function, which also gives out a consoletext
        /// uses the webfunction internal
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <param name="_partnerID"></param>
        /// <returns></returns>
        public async Task<bool> DeclineTradeoffer(string _tradeOfferID, SteamID _partnerID)
        {
            bool declinedoffer = await DeclineTradeOffer(_tradeOfferID);

            if (declinedoffer)
            {
                Console.WriteLine($"Declined the offer with the id {_tradeOfferID} from the user {_partnerID.ConvertToUInt64()}");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Simplified function, which also gives out a short consoletext
        /// uses the webfunction internal
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public async Task<bool> DeclineTradeofferShortMessage(string _tradeOfferID)
        {
            bool declinedoffer = await DeclineTradeOffer(_tradeOfferID);

            if (declinedoffer)
            {
                Console.WriteLine($"Declined offer: {_tradeOfferID}");

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create a tradeoffer object
        /// Add the passed list of assets to the assets of the itemsToGive list inside the tradeoffer
        /// Create a NameValueCollection with our data
        /// Format is Json so we can just do "JsonConvert.SerializeObject" which parses the object into Json string
        /// Create an url and send the request
        /// </summary>
        /// <param name="_tradeOfferItems"></param>
        /// <param name="_partnerID"></param>
        /// <param name="_tradeOfferMessage"></param>
        /// <returns></returns>
        public async Task<bool> SendTradeOffer(List<TradeOfferItem> _tradeOfferItems, string _partnerID, string _tradeOfferMessage = "")
        {
            if(_tradeOfferItems.Count == 0)
            {
                return false;
            }

            TradeOfferSend tradeoffer = new TradeOfferSend();

            tradeoffer.m_ItemsToGive.m_Assets.AddRange(_tradeOfferItems);

            NameValueCollection data = new NameValueCollection()
            {
                {"sessionid", SteamWeb.Instance.SessionID},
                {"serverid", "1"},
                {"partner", _partnerID},
                {"tradeoffermessage", _tradeOfferMessage},
                {"json_tradeoffer", JsonConvert.SerializeObject(tradeoffer)},
                {"trade_offer_create_params", ""}
            };

            string referer = $"https://{SteamWeb.Instance.m_SteamCommunityHost}/tradeoffer/new";
            string url = $"{referer}/send";

            string response = await WebHelper.Instance.GetStringFromRequest(url, data, false, referer);

            TradeOfferAcceptResponse acceptResponse = JsonConvert.DeserializeObject<TradeOfferAcceptResponse>(response);

            if (acceptResponse != null && (acceptResponse.TradeID != null || acceptResponse.NeedsEmailConfirmation || acceptResponse.NeedsMobileConfirmation))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region InterfaceCalls alternative
        /// <summary>
        /// Get tradeoffers with specific information
        /// </summary>
        public KeyValue GetTradeOffersInterface(bool _getSentOffers, bool _getReceivedOffers, bool _getDescription, bool _activeOnly, bool _historicalOnly)
        {
            // Reuqest the IEconService with our apikey
            using (dynamic IEconService = WebAPI.GetInterface(IEconServiceInterface, SteamWeb.Instance.APIKey))
            {
                // ALWAYS TRY to work with interfaces, because it could go wrong and destroy everything
                try
                {
                    return IEconService.GetTradeOffers(
                        get_sent_offers: BoolToInt(_getSentOffers),
                        get_received_offers: BoolToInt(_getReceivedOffers),
                        get_descriptions: BoolToInt(_getDescription),
                        active_only: BoolToInt(_activeOnly),
                        historical_only: BoolToInt(_historicalOnly),
                        secure: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
        }

        /// <summary>
        /// Get one tradeoffer with the passed tradeOfferID
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public KeyValue GetTradeOfferInterface(int _tradeOfferID)
        {
            using (dynamic IEconService = WebAPI.GetInterface(IEconServiceInterface, SteamWeb.Instance.APIKey))
            {
                // ALWAYS TRY to work with interfaces, because it could go wrong and destroy everything
                try
                {
                    return IEconService.GetTradeOffer(
                        tradeofferid: _tradeOfferID,
                        secure: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get the tradeoffersSummary, so we have an overview of all our tradeoffers
        /// </summary>
        /// <returns></returns>
        public KeyValue GetTradeOffersSummaryInterface()
        {
            using (dynamic IEconService = WebAPI.GetInterface(IEconServiceInterface, SteamWeb.Instance.APIKey))
            {
                try
                {
                    return IEconService.GetTradeOffersSummary(
                        secure: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Decline a received tradeoffer
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public KeyValue DeclineTradeOfferInterface(int _tradeOfferID)
        {
            using (dynamic IEconService = WebAPI.GetInterface(IEconServiceInterface, SteamWeb.Instance.APIKey))
            {
                try
                {
                    return IEconService.DeclineTradeOffer(
                        tradeofferid: _tradeOfferID,
                        method: WebRequestMethods.Http.Post,
                        secure: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Cancel a sent tradeoffer
        /// </summary>
        /// <param name="_tradeOfferID"></param>
        /// <returns></returns>
        public KeyValue CancelTradeOfferInterface(int _tradeOfferID)
        {
            using (dynamic IEconService = WebAPI.GetInterface(IEconServiceInterface, SteamWeb.Instance.APIKey))
            {
                try
                {
                    return IEconService.CancelTradeOffer(
                        tradeofferid: _tradeOfferID,
                        method: WebRequestMethods.Http.Post,
                        secure: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        #endregion

        /// <summary>
        /// Get the escrowduration in days from the trade
        /// 
        /// Therefor send a request to get information about the tradeoffer
        /// Search for our and their Escrowduration, if successful, give the value out, else throw an exception which will tell us, what went wrong
        /// </summary>
        /// <param name="_tradeofferID"></param>
        /// <returns></returns>
        public async Task<TradeOfferEscrowDuration> GetTradeOfferEscrowDuration(string _tradeofferID)
        {
            string url = "http://steamcommunity.com/tradeoffer/" + _tradeofferID;

            string response = await WebHelper.Instance.GetStringFromRequest(url);

            // HTMLAgility and XPath would be a chaos here, so Regex is easier and the way to go
            Match ourMatch = Regex.Match(response, @"g_daysMyEscrow(?:[\s=]+)(?<days>[\d]+);", RegexOptions.IgnoreCase);
            Match theirMatch = Regex.Match(response, @"g_daysTheirEscrow(?:[\s=]+)(?<days>[\d]+);", RegexOptions.IgnoreCase);

            if (!ourMatch.Groups["days"].Success || !theirMatch.Groups["days"].Success)
            {
                Match steamErrorMatch = Regex.Match(response, @"<div id=""error_msg"">([^>]+)<\/div>", RegexOptions.IgnoreCase);

                if (steamErrorMatch.Groups.Count > 1)
                {
                    string steamError = Regex.Replace(steamErrorMatch.Groups[1].Value.Trim(), @"\t|\n|\r", "");
                    Console.WriteLine(steamError);
                }

                Console.WriteLine($"Not logged in, can't retrieve escrow duration for tradeofferID: {_tradeofferID}");

                return new TradeOfferEscrowDuration
                {
                    Success = false,
                    DaysOurEscrow = -1,
                    DaysTheirEscrow = -1
                };
            }

            return new TradeOfferEscrowDuration
            {
                Success = true,
                DaysOurEscrow = int.Parse(ourMatch.Groups["days"].Value),
                DaysTheirEscrow = int.Parse(theirMatch.Groups["days"].Value)
            };
        }

        /// <summary>
        /// Converts a bool into a number so we can make a webrequest
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        private static int BoolToInt(bool _value) => _value ? 1 : 0;
    }

    /// <summary>
    /// If we receive a response, there is a "response:" tag, which will be here excluded, so we do not get a nullreference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIResponse<T>
    {
        public T Response { get; set; }
    }
}