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

namespace CTB.Web.SteamStoreWebAPI.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize the appdatas inside the discoveryqueueresponse
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class DiscoveryQueueAppData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url_name")]
        public string UrlName { get; set; }

        [JsonProperty("discount_block")]
        public string DiscountBlock { get; set; }

        [JsonProperty("header")]
        public string Header { get; set; }

        [JsonProperty("os_windows")]
        public bool OSWindows { get; set; }

        [JsonProperty("discount")]
        public bool Discount { get; set; }

        [JsonProperty("localized")]
        public bool Localized { get; set; }
    }
}
