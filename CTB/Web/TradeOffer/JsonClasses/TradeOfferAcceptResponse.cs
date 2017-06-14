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
    /// Class to serialize and deserialize the tradeoffer accepted response
    /// JsonProperty gets the result default values and parses it into our variables
    /// </summary>
    class TradeOfferAcceptResponse
    {
        [JsonProperty("tradeid")]
        public string TradeID { get; set; }

        [JsonProperty("needs_mobile_confirmation")]
        public bool NeedsMobileConfirmation { get; set; }

        [JsonProperty("needs_email_confirmation")]
        public bool NeedsEmailConfirmation { get; set; }

        [JsonProperty("email_domain")]
        public string EmailDomain { get; set; }
    }
}