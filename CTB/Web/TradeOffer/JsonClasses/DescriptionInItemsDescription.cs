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

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a description in a description of an item
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class DescriptionInItemsDescription
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}