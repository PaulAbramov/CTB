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

namespace CTB.Web.SteamUserWeb.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize Inventoryresponse
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class InventoryItem
    {
        [JsonProperty("appid")]
        public string AppID { get; set; }

        [JsonProperty("contextid")]
        public string ContextID { get; set; }

        [JsonProperty("assetid")]
        public string AssetID { get; set; }

        [JsonProperty("classid")]
        public string ClassID { get; set; }

        [JsonProperty("intanceid")]
        public string InstanceID { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
