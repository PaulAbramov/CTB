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
    /// Class to serialize and deserialize Inventoryresponse
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class InventoryResponse
    {
        [JsonProperty("assets")]
        public List<InventoryItem> Assets { get; set; }

        [JsonProperty("descriptions")]
        public List<InventoryItemsDescription> ItemsDescriptions { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("more_items")]
        public int More { get; set; }

        [JsonProperty("last_assetid")]
        public string LastAssetID { get; set; }

        [JsonProperty("total_inventory_count")]
        public int TotalInventoryCount { get; set; }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("rwgrsn")]
        public int RWGRSN { get; set; }
    }
}
