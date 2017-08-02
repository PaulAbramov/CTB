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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CTB.JsonClasses;
using CTB.Web;
using CTB.Web.TradeOffer;
using CTB.Web.TradeOffer.JsonClasses;
using SteamKit2;

namespace CTB.HelperClasses
{
    public class TradeOfferHelperClass
    {
        private readonly SemaphoreSlim m_tradesSemaphore = new SemaphoreSlim(1);

        private readonly TradeOfferWebAPI m_tradeOfferWebAPI;
        private readonly MobileHelper m_mobileHelper;
        private readonly BotInfo m_botInfo;

        private bool m_parsingScheduled;

        /// <summary>
        /// Constructor to initialize our variables
        /// </summary>
        /// <param name="_mobileHelper"></param>
        /// <param name="_botInfo"></param>
        public TradeOfferHelperClass(MobileHelper _mobileHelper, BotInfo _botInfo)
        {
            m_mobileHelper = _mobileHelper;
            m_tradeOfferWebAPI = new TradeOfferWebAPI();
            m_botInfo = _botInfo;
        }

        /// <summary>
        /// We want to have 2 tasks running
        /// 
        /// First Task will be going trough, starts to check for tradeoffers and afterwards releases the semaphore
        /// Second Task will set the boolean "parsingScheduled" to true, and will wait for the first Task to complete
        /// When the first Task completes and realeases the semaphore, 
        /// the second Task is going to lock the semaphore and start the function to check for tradeoffers
        /// The next Task has to wait for the Task which is currently checking the offer
        /// 
        /// Every further Task will be returned, so we have 2 Tasks working on this
        /// One is working on the trades
        /// The other is waiting for the first to complete and handling the trades afterwards
        /// 
        /// We can't lock an object in async await, this is a way to lock a function to prevent it from be called multiple times before reaching the end
        /// </summary>
        /// <param name="_steamFriendsHelper"></param>
        /// <param name="_steamID"></param>
        /// <returns></returns>
        public async Task CheckTradeOffers(SteamFriendsHelper _steamFriendsHelper, SteamID _steamID)
        {
            lock (m_tradesSemaphore)
            {
                if (m_parsingScheduled)
                {
                    return;
                }

                m_parsingScheduled = true;
            }

            await m_tradesSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                lock (m_tradesSemaphore)
                {
                    m_parsingScheduled = false;
                }

                await CheckForTradeOffers(_steamFriendsHelper, _steamID).ConfigureAwait(false);
            }
            finally
            {
                m_tradesSemaphore.Release();
            }
        }

        /// <summary>
        /// First get the summary of our tradeoffers, so we know how much tradeoffers we do have to handle
        /// Initialize a counter, which will be increased after every handled tradeoffer
        /// Receive all active tradeoffers
        /// 
        /// Check if the result is not null so we do not run into an error
        /// Go trough every tradeoffer and check if it is active and we did not reach the amount of offers to handle
        /// If it is active get the tradepartners ID so we can print a message with his ID in the accept or decline function
        /// If we have to give Items and do not receive any items decline the trade
        /// Check if the tradeoffer is a donation
        /// If it is not a donation go on and check if the tradeoffer is sent by an admin
        /// If it is sent by an admin accept it and continue with the next trade
        /// If it is not a donation, not sent by an admin and it seems like a fair trade, check for escrow and finally check the tradeitems itself if it is a fair trade
        /// </summary>
        /// <param name="_steamFriendsHelper"></param>
        /// <param name="_steamID"></param>
        public async Task CheckForTradeOffers(SteamFriendsHelper _steamFriendsHelper, SteamID _steamID)
        {
            TradeOffersSummaryResponse tradeOfferCountToHandle = await m_tradeOfferWebAPI.GetTradeOffersSummary().ConfigureAwait(false);
            int tradeOfferHandledCounter = 0;

            GetOffersResponse receivedOffers = await m_tradeOfferWebAPI.GetReceivedActiveTradeOffers(true).ConfigureAwait(false);

            if (receivedOffers.TradeOffersReceived != null)
            {
                foreach (TradeOffer tradeOffer in receivedOffers.TradeOffersReceived)
                {
                    if (tradeOfferHandledCounter >= tradeOfferCountToHandle.PendingReceivedCount)
                    {
                        break;
                    }

                    if (tradeOffer.TradeOfferState != ETradeOfferState.ETradeOfferStateActive)
                    {
                        continue;
                    }

                    if (tradeOffer.ConfirmationMethod == ETradeOfferConfirmationMethod.ETradeOfferConfirmationMethod_Email)
                    {
                        Console.WriteLine($"Accept the trade offer {tradeOffer.TradeOfferID} via your email");
                        tradeOfferHandledCounter++;

                        continue;
                    }

                    //  If we were not logged on to the web or the authentication failed, go to the next tradeoffer and check it again
                    if (!await SteamWeb.Instance.RefreshSessionIfNeeded().ConfigureAwait(false))
                    {
                        continue;
                    }

                    if (tradeOffer.ConfirmationMethod == ETradeOfferConfirmationMethod.ETradeOfferConfirmationMethod_MobileApp)
                    {
                        m_mobileHelper.ConfirmAllTrades(SteamWeb.Instance.SteamLogin, SteamWeb.Instance.SteamLoginSecure, SteamWeb.Instance.SessionID);
                        tradeOfferHandledCounter++;

                        continue;
                    }

                    SteamID tradePartnerID = _steamFriendsHelper.GetSteamID(tradeOffer.AccountIDOther);

                    //  Check for a donation
                    if (m_botInfo.AcceptDonations && await TradeOfferIsDonation(tradeOffer, tradePartnerID).ConfigureAwait(false))
                    {
                        tradeOfferHandledCounter++;

                        continue;
                    }

                    //  Check for a tradeoffer from an admin
                    if (await AdminTradeOffer(_steamFriendsHelper, tradeOffer, tradePartnerID).ConfigureAwait(false))
                    {
                        m_mobileHelper.ConfirmAllTrades(SteamWeb.Instance.SteamLogin, SteamWeb.Instance.SteamLoginSecure, SteamWeb.Instance.SessionID);
                        tradeOfferHandledCounter++;

                        continue;
                    }

                    //  Check if we have to give items but do not receive any items
                    if (tradeOffer.ItemsToGive != null && tradeOffer.ItemsToReceive == null)
                    {
                        await m_tradeOfferWebAPI.DeclineTradeofferShortMessage(tradeOffer.TradeOfferID).ConfigureAwait(false);
                        tradeOfferHandledCounter++;

                        continue;
                    }

                    //  If we do not want to accept escrow tradeoffers, check them here before going on
                    if (!m_botInfo.AcceptEscrow)
                    {
                        if (await CheckTradeOfferForEscrow(tradeOffer, tradePartnerID).ConfigureAwait(false))
                        {
                            tradeOfferHandledCounter++;

                            continue;
                        }
                    }

                    await CheckTradeOffer(receivedOffers, tradeOffer, tradePartnerID).ConfigureAwait(false);
                    tradeOfferHandledCounter++;
                }
            }
        }

        /// <summary>
        /// Check if the tradeoffer is a donation, if so, accept it, else move on
        /// </summary>
        /// <param name="_tradeOffer"></param>
        /// <param name="_tradePartnerID"></param>
        /// <returns></returns>
        private async Task<bool> TradeOfferIsDonation(TradeOffer _tradeOffer, SteamID _tradePartnerID)
        {
            if (_tradeOffer.ItemsToGive == null && _tradeOffer.ItemsToReceive != null)
            {
                await m_tradeOfferWebAPI.AcceptTradeofferShortMessage(_tradeOffer.TradeOfferID).ConfigureAwait(false);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the tradeoffer is sent from an admin, if so, accept it and throw a marked message, else move on
        /// </summary>
        /// <param name="_steamFriendsHelper"></param>
        /// <param name="_tradeOffer"></param>
        /// <param name="_tradePartnerID"></param>
        /// <returns></returns>
        private async Task<bool> AdminTradeOffer(SteamFriendsHelper _steamFriendsHelper, TradeOffer _tradeOffer, SteamID _tradePartnerID)
        {
            if (_steamFriendsHelper.IsBotAdmin(_tradePartnerID, m_botInfo.Admins))
            {
                if (await m_tradeOfferWebAPI.AcceptTradeOffer(_tradeOffer.TradeOfferID).ConfigureAwait(false))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Tradeoffer {_tradeOffer.TradeOfferID} was sent by admin {_tradePartnerID.ConvertToUInt64()}");
                    Console.ForegroundColor = ConsoleColor.White;

                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the tradeoffer has a escrow duration
        /// Escrow holds back out tradeoffers for some days
        /// We want to decline them if they going to be held back
        /// </summary>
        /// <param name="_tradeOffer"></param>
        /// <param name="_tradePartnerID"></param>
        /// <returns></returns>
        private async Task<bool> CheckTradeOfferForEscrow(TradeOffer _tradeOffer, SteamID _tradePartnerID)
        {
            TradeOfferEscrowDuration hasTradeOfferEscrowDuration = await m_tradeOfferWebAPI.GetTradeOfferEscrowDuration(_tradeOffer.TradeOfferID).ConfigureAwait(false);
            if (hasTradeOfferEscrowDuration.Success && hasTradeOfferEscrowDuration.DaysOurEscrow > 0 || hasTradeOfferEscrowDuration.DaysTheirEscrow > 0)
            {
                return await m_tradeOfferWebAPI.DeclineTradeofferShortMessage(_tradeOffer.TradeOfferID).ConfigureAwait(false);
            }

            return !hasTradeOfferEscrowDuration.Success;
        }

        /// <summary>
        /// If we have to give items and we will be receiving items then check the tradeoffer
        /// 
        /// Get more details about the items which will be traded from the offersResponse we got earlier
        /// 
        /// bool shouldAcceptTrade will be checked before every tradecheck so if one method says we can accept the trade we do not have to make the other tradechecks (do not waste time)
        /// 
        /// Check if the trade is a 1:2 trade
        /// Check if the trade is a 1:1 trade
        /// 
        /// At the end check against if the amount of cards we want to accept is equal to the amount of cards which is requested from us
        /// </summary>
        /// <param name="_offersResponse"></param>
        /// <param name="_tradeOffer"></param>
        /// <param name="_partnerID"></param>
        /// <returns></returns>
        private async Task CheckTradeOffer(GetOffersResponse _offersResponse, TradeOffer _tradeOffer, SteamID _partnerID)
        {
            if(_tradeOffer.ItemsToGive != null && _tradeOffer.ItemsToReceive != null)
            {
                bool shouldAcceptTrade = false;

                List<TradeOfferItemDescription> ourItems = FillItemsList(_tradeOffer.ItemsToGive, _offersResponse, ItemType.TRADING_CARD);
                List<TradeOfferItemDescription> theirItems = FillItemsList(_tradeOffer.ItemsToReceive, _offersResponse, ItemType.TRADING_CARD | ItemType.FOIL_TRADING_CARD);

                int ourItemsCount = ourItems.Count;

                // Check 1:2 Trade
                if(m_botInfo.Accept1on2Trades && !shouldAcceptTrade)
                {
                    shouldAcceptTrade = CheckForOneOnTwoCardTrade(ourItems, theirItems);
                }

                // Check 1:1 same Set Trade
                if(m_botInfo.Accept1on1Trades && !shouldAcceptTrade)
                {
                    shouldAcceptTrade = CheckForOneOnOneCardTrade(ourItems, theirItems).Count == ourItemsCount;
                }

                if (_tradeOffer.ItemsToGive.Count > _tradeOffer.ItemsToReceive.Count)
                {
                    await m_tradeOfferWebAPI.DeclineTradeoffer(_tradeOffer.TradeOfferID, _partnerID).ConfigureAwait(false);

                    return;
                }

                if (shouldAcceptTrade)
                {
                    bool accepted = await m_tradeOfferWebAPI.AcceptTradeofferShortMessage(_tradeOffer.TradeOfferID).ConfigureAwait(false);

                    if(accepted)
                    {
                        m_mobileHelper.ConfirmAllTrades(SteamWeb.Instance.SteamLogin, SteamWeb.Instance.SteamLoginSecure, SteamWeb.Instance.SessionID);
                    }
                    else
                    {
                        Console.WriteLine("tradeoffer couldn't be accepted, return true, so we can handle it next time.");
                    }

                    return;
                }
            }

            await m_tradeOfferWebAPI.DeclineTradeofferShortMessage(_tradeOffer.TradeOfferID).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the appID of the game from the marketHashName
        /// The appID is the number at the beginning before the '-' so we want to split it at the '-'
        /// Return the left (first) part of the string array we got as a integer
        /// </summary>
        /// <param name="_item"></param>
        /// <returns></returns>
        private int GetAppIDFromItem(TradeOfferItemDescription _item)
        {
            return Convert.ToInt32(_item.MarketHashName.Split('-')[0]);
        }

        /// <summary>
        /// Check every item inside the list we are going to give away or we are going to receive
        /// The description we got earlier got all the items inside the trade, look there which item is ours or theirs so we have more informations about the item
        /// 
        /// Enumerate trough all values of the enum ItemType
        /// If we want to allow trading cards and foil trading cards to be added to our itemslist, therefore use the binary OR operator "ItemType.TRADING_CARD | ItemType.FOIL_TRADING_CARD"
        /// If it is set inside our parameter "_items" use it on the switch and add the item to the list
        /// </summary>
        /// <param name="_listToCheck"></param>
        /// <param name="_offersResponse"></param>
        /// <param name="_items"></param>
        private List<TradeOfferItemDescription> FillItemsList(List<TradeOfferItem> _listToCheck, GetOffersResponse _offersResponse, ItemType _items)
        {
            List<TradeOfferItemDescription> itemListToFill = new List<TradeOfferItemDescription>();

            foreach (TradeOfferItem item in _listToCheck)
            {
                if (item.AppID == "753")
                {
                    TradeOfferItemDescription itemWithDescription = _offersResponse.Descriptions.First(_itemDescription => _itemDescription.ClassID == item.ClassID && _itemDescription.InstanceID == item.InstanceID);

                    Array checkTypeValues = Enum.GetValues(typeof(ItemType));

                    foreach(ItemType itemType in checkTypeValues)
                    {
                        if((_items & itemType) == itemType)
                        {
                            switch(itemType)
                            {
                                case ItemType.BOOSTER_PACK:
                                    if(itemWithDescription.Type.ToLower().Contains("booster pack"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                                case ItemType.EMOTICON:
                                    if (itemWithDescription.Type.ToLower().Contains("emoticon"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                                case ItemType.PROFILE_BACKGROUND:
                                    if (itemWithDescription.Type.ToLower().Contains("profile background"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                                case ItemType.STEAM_GEMS:
                                    if(itemWithDescription.Type.Equals("Steam Gems"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                                case ItemType.FOIL_TRADING_CARD:
                                    if(itemWithDescription.Type.ToLower().Contains("foil"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                                case ItemType.TRADING_CARD:
                                    if(itemWithDescription.Type.ToLower().Contains("trading card") && !itemWithDescription.Type.ToLower().Contains("foil"))
                                    {
                                        itemListToFill.Add(itemWithDescription);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return itemListToFill;
        }

        /// <summary>
        /// Beautify the code
        /// 
        /// Compare the AppID of every card from us with the AppID of every card from them
        /// If there was a match, put it into the list and remove it from the existing list, so we do not match twice
        /// </summary>
        /// <param name="_ourItems"></param>
        /// <param name="_theirItems"></param>
        /// <returns></returns>
        private List<TradeOfferItemDescription> CheckForOneOnOneCardTrade(List<TradeOfferItemDescription> _ourItems, List<TradeOfferItemDescription> _theirItems)
        {
            List<TradeOfferItemDescription> shouldAccept = new List<TradeOfferItemDescription>();

            for (int i = (_ourItems.Count - 1); i >= 0; i--)
            {
                for (int j = (_theirItems.Count - 1); j >= 0; j--)
                {
                    if (GetAppIDFromItem(_ourItems[i]) == GetAppIDFromItem(_theirItems[j]))
                    {
                        shouldAccept.Add(_ourItems[i]);

                        _ourItems.RemoveAt(i);
                        _theirItems.RemoveAt(j);
                        break;
                    }
                }
            }

            return shouldAccept;
        }

        /// <summary>
        /// Beautify the code
        /// 
        /// Check if the count of their items is atleast the same as our amount of items multiplied by 2
        /// So we have 1:2 or 2:4 trades and so on
        /// 
        /// If we have this kind of trade, return true so we can accept the trade, else return false so we do not accept this trade
        /// </summary>
        /// <param name="_ourItems"></param>
        /// <param name="_theirItems"></param>
        /// <returns></returns>
        private bool CheckForOneOnTwoCardTrade(List<TradeOfferItemDescription> _ourItems, List<TradeOfferItemDescription> _theirItems)
        {
            if (_ourItems.Count > 0 && _theirItems.Count >= _ourItems.Count * 2)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// This are the ItemTypes we have inside our steaminventory for the AppID 753
    /// Tag them as flags and give them a value so they can be used with bitoperations
    /// for example:
    /// 
    /// 1 in bits = 0000 0001
    /// 2 in bits = 0000 0010
    /// 4 in bits = 0000 0100
    /// 8 in bits = 0000 1000
    /// and so on
    /// 
    /// If we want to check the ItemType for boosterpack and emoticon we are going to have:
    /// 
    /// 1 and 2 = 0000 0011
    /// 
    /// Now we can check from right to left whether the value is set as an one or zero
    /// </summary>
    [Flags]
    internal enum ItemType
    {
        BOOSTER_PACK = 1,
        EMOTICON = 2,
        PROFILE_BACKGROUND = 4,
        STEAM_GEMS = 8,
        FOIL_TRADING_CARD = 16,
        TRADING_CARD = 32
    }
}