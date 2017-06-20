/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/


using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize tradeitems from a tradeoffer we send
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOfferSendAssets
    {
        [JsonProperty("assets")]
        public List<TradeOfferItem> m_Assets = new List<TradeOfferItem>();
    }
}
