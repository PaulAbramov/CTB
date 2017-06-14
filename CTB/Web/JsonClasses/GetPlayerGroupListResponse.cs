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

namespace CTB.Web.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a grouplist of the groups of a steamuser
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class GetPlayerGroupListResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("groups")]
        public List<GetPlayerGroupID> GroupIDs { get; set; }
    }
}