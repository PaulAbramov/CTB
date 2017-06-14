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
    /// Class to serialize and deserialize tradeitems from a trade
    /// Every item has an appid from which game it is, and the other ids define which kind of an item it is
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOfferItem
    {
        [JsonProperty("appid")]
        public string AppID { get; set; }

        [JsonProperty("contextid")]
        public string ContextID { get; set; }

        [JsonProperty("assetid")]
        public string AssetID { get; set; }

        [JsonProperty("classid")]
        public string ClassID { get; set; }

        [JsonProperty("instanceid")]
        public string InstanceID { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("missing")]
        public bool Missing { get; set; }
    }
}
