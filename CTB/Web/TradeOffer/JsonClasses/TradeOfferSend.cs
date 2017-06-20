/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/


using Newtonsoft.Json;

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize tradeoffers which we want to send
    /// Both have a list of items which they are going to send
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOfferSend
    {
        [JsonProperty("me")]
        public TradeOfferSendAssets m_ItemsToGive = new TradeOfferSendAssets();

        [JsonProperty("them")]
        public TradeOfferSendAssets m_ItemsToReceive = new TradeOfferSendAssets();
    }
}
