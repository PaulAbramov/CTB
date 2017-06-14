/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using System;

namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize the escrowdurationerror of a trade, so we filter error out 
    /// </summary>
    public class TradeOfferEscrowDurationParseException : Exception
    {
        public TradeOfferEscrowDurationParseException() : base() { }
        public TradeOfferEscrowDurationParseException(string _message) : base(_message) { }
    }
}
