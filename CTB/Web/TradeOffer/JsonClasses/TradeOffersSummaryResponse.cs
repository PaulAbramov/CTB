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
    /// Class to serialize and deserialize the TradeOffersSummary, where we get the amount of new Offers
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOffersSummaryResponse
    {
        [JsonProperty("pending_received_count")]
        public int PendingReceivedCount { get; set; }

        [JsonProperty("new_received_count")]
        public int NewReceivedCount { get; set; }

        [JsonProperty("updated_received_count")]
        public int UpdatedReceivedCount { get; set; }

        [JsonProperty("historical_received_count")]
        public int HistoricalReceivedCount { get; set; }

        [JsonProperty("pending_sent_count")]
        public int PendingSentCount { get; set; }

        [JsonProperty("newly_accepted_sent_count")]
        public int NewlyAcceptedSentCount { get; set; }

        [JsonProperty("updated_sent_count")]
        public int UpdatedSentCount { get; set; }

        [JsonProperty("historical_sent_count")]
        public int HistoricalSentCount { get; set; }

        [JsonProperty("escrow_received_count")]
        public int EscrowReceivedCount { get; set; }

        [JsonProperty("escrow_sent_count")]
        public int EscrowSentCount { get; set; }
    }
}
