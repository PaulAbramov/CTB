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
    /// Class to serialize and deserialize the items description from the offers
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOfferItemDescription
    {
        [JsonProperty("appid")]
        public int AppID { get; set; }

        [JsonProperty("classid")]
        public string ClassID { get; set; }

        [JsonProperty("instanceid")]
        public string InstanceID { get; set; }

        [JsonProperty("currency")]
        public bool Currency { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("icon_url_large")]
        public string IconUrlLarge { get; set; }

        [JsonProperty("descriptions")]
        public List<DescriptionInItemsDescription> Descriptions { get; set; }

        [JsonProperty("tradable")]
        public bool Tradable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_color")]
        public string NameColor { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("market_name")]
        public string MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }
    }
}
