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

namespace CTB.Web.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a groupID
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class GetPlayerGroupID
    {
        [JsonProperty("gid")]
        public string GroupID { get; set; }
    }
}
