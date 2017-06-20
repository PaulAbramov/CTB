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
    /// Class to serialize and deserialize InventoryItemTag
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class InventoryItemTag
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("internal_name")]
        public string InternalName { get; set; }

        [JsonProperty("localized_category_name")]
        public string LocalizedCategoryName { get; set; }

        [JsonProperty("localized_tag_name")]
        public string LocalizedTagName { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}
