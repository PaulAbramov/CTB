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
