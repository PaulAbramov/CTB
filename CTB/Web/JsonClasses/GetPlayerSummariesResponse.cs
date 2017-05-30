using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTB.Web.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a list of summaries of the profiles of steamusers
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class GetPlayerSummariesResponse
    {
        [JsonProperty("players")]
        public List<GetPlayerSummary> PlayerSummaries { get; set; } 
    }
}