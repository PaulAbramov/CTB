using System.Collections.Generic;
using Newtonsoft.Json;

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize the tradeoffer received from web
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    public class TradeOffer
    {
        [JsonProperty("tradeofferid")]
        public string TradeOfferID { get; set; }

        [JsonProperty("accountid_other")]
        public int AccountIDOther { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("expiration_time")]
        public int ExpirationTime { get; set; }

        [JsonProperty("trade_offer_state")]
        public ETradeOfferState TradeOfferState { get; set; }

        [JsonProperty("items_to_give")]
        public List<TradeOfferItem> ItemsToGive { get; set; }

        [JsonProperty("items_to_receive")]
        public List<TradeOfferItem> ItemsToReceive { get; set; }

        [JsonProperty("is_our_offer")]
        public bool IsOurOffer { get; set; }

        [JsonProperty("time_created")]
        public int TimeCreated { get; set; }

        [JsonProperty("time_updated")]
        public int TimeUpdated { get; set; }

        [JsonProperty("from_real_time_trade")]
        public bool FromRealTimeTrade { get; set; }

        [JsonProperty("escrow_end_date")]
        public int EscrowEndDate { get; set; }

        [JsonProperty("confirmation_method")]
        public ETradeOfferConfirmationMethod ConfirmationMethod { get; set; }
    }

    /// <summary>
    /// Enum of the tradeOfferState, which indicates if the offer is active and responsible etc.
    /// </summary>
    public enum ETradeOfferState
    {
        ETradeOfferStateInvalid = 1,
        ETradeOfferStateActive = 2,
        ETradeOfferStateAccepted = 3,
        ETradeOfferStateCountered = 4,
        ETradeOfferStateExpired = 5,
        ETradeOfferStateCanceled = 6,
        ETradeOfferStateDeclined = 7,
        ETradeOfferStateInvalidItems = 8,
        ETradeOfferStateCreatedNeedsConfirmation = 9,
        ETradeOfferStateCanceledBySecondFactor = 10,
        ETradeOfferStateInEscrow = 11
    }

    /// <summary>
    /// Enum of the tradeofferconfirmationmethod, which tells us, how we have to accept the trade
    /// </summary>
    public enum ETradeOfferConfirmationMethod
    {
        ETradeOfferConfirmationMethod_Invalid = 0,
        ETradeOfferConfirmationMethod_Email = 1,
        ETradeOfferConfirmationMethod_MobileApp = 2
    }
}
