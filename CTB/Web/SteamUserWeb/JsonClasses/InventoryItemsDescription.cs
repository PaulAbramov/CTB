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

namespace CTB.Web.SteamUserWeb.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize itemsdescription from the inventory
    /// Every item has an appid from which game it is, and the other ids define which kind of an item it is
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class InventoryItemsDescription
    {
        [JsonProperty("appid")]
        public int AppID { get; set; }

        [JsonProperty("classid")]
        public string ClassID { get; set; }

        [JsonProperty("instanceid")]
        public string InstanceID { get; set; }

        [JsonProperty("currency")]
        public int Currency { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("icon_url")]
        public string IconURL { get; set; }

        [JsonProperty("icon_url_large")]
        public string IconURLLarge { get; set; }

        [JsonProperty("descriptions")]
        public List<InventoryItemDescriptions> Descriptions { get; set; }

        [JsonProperty("tradable")]
        public int Tradable { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_color")]
        public string NameColor { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("marekt_name")]
        public string MarketName { get; set; }

        [JsonProperty("market_hash_name")]
        public string MarketHashName { get; set; }

        [JsonProperty("market_fee_app")]
        public string MarketFeeApp { get; set; }

        [JsonProperty("commodity")]
        public int Commodity { get; set; }

        [JsonProperty("market_tradable_restriction")]
        public int MarketTradableRestriction { get; set; }

        [JsonProperty("market_marketable_restriction")]
        public int MarketMarketableRestriction { get; set; }

        [JsonProperty("marketable")]
        public int Marketable { get; set; }

        [JsonProperty("tags")]
        public List<InventoryItemTag> Tags { get; set; }
    }
}
