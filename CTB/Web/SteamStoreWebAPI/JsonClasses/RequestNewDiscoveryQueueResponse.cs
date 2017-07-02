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

namespace CTB.Web.SteamStoreWebAPI.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize discoveryqueue and its items
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class RequestNewDiscoveryQueueResponse
    {
        [JsonProperty("queue")]
        public List<uint> Queue { get; set; }

        [JsonProperty("rgAppData")]
        public Dictionary<uint, DiscoveryQueueAppData> AppDatas { get; set; }
    }
}
