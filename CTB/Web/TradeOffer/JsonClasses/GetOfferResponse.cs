using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize a response from the API call "IEconService/GetTradeOffer/v1"
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class GetOfferResponse
    {
        [JsonProperty("offer")]
        public TradeOffer Offer { get; set; }

        [JsonProperty("descriptions")]
        public List<TradeOfferItemDescription> Descriptions { get; set; }
    }
}
