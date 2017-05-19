namespace CTB.Web.TradeOffer.JsonClasses
{
    /// <summary>
    /// Class to serialize and deserialize the escrowduration of a trade, so we know if it will be hold by steam for some days
    /// </summary>
    public class TradeOfferEscrowDuration
    {
        public int DaysOurEscrow { get; set; }
        public int DaysTheirEscrow { get; set; }
    }
}
