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
using CTB.Web;
using CTB.Web.SteamUserWeb;
using CTB.Web.SteamUserWeb.JsonClasses;
using SteamKit2;

namespace CTB.HelperClasses
{
    public class CardFarmHelperClass
    {
        private readonly SteamUserWebAPI m_steamUserWebAPI;
        private readonly GamesLibraryHelperClass m_gamesLibraryHelper;
        private readonly SteamWeb m_steamWeb;
        private readonly Logger.Logger m_logger;

        private CancellationTokenSource m_cardFarmCancellationTokenSource;

        /// <summary>
        /// Constructor to initialize variables and the class
        /// </summary>
        /// <param name="_gamesLibraryHelper"></param>
        /// <param name="_steamWeb"></param>
        /// <param name="_logger"></param>
        public CardFarmHelperClass(GamesLibraryHelperClass _gamesLibraryHelper, SteamWeb _steamWeb, Logger.Logger _logger)
        {
            m_steamWeb = _steamWeb;
            m_steamUserWebAPI = new SteamUserWebAPI(m_steamWeb, _logger);

            m_logger = _logger;

            m_gamesLibraryHelper = _gamesLibraryHelper;
        }

        /// <summary>
        /// Initialize the cancellationtoken so we can interrupt the method on lost connection
        /// Create a new task with the cancellationtoken
        /// While the task is not cancelled get all badges to farm
        /// If we do not have any badge to farm, remove the playing status so we are shown as online
        /// 
        ///     If we have some badges to farm start a new while loop and start to farm this badge
        ///     Check every 5 minutes if we have still some cards left to farm for this badge
        ///     If we do not have any left, leave the while loop
        /// 
        /// If we are not comming out from the while loop check every 5 minutes for new badges to farm
        /// If we are comming out fron the while loop we want to check for the next badge to farm, therefore we do not wait 5 minuts
        /// </summary>
        /// <param name="_steamClient"></param>
        public void StartFarmCards(SteamClient _steamClient)
        {
            m_cardFarmCancellationTokenSource = new CancellationTokenSource();

            bool checkForNewGame = false;

            Task.Run(async () =>
            {
                while (!m_cardFarmCancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!await m_steamWeb.RefreshSessionIfNeeded().ConfigureAwait(false))
                    {
                        continue;
                    }

                    List<GameToFarm> gamesToFarm = await m_steamUserWebAPI.GetBadgesToFarm().ConfigureAwait(false);

                    bool isRunning = (gamesToFarm.Count > 0);

                    if (!isRunning)
                    {
                        m_gamesLibraryHelper.SetGamePlaying(0);
                    }

                    while (isRunning && !m_cardFarmCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        m_gamesLibraryHelper.SetGamePlaying(Convert.ToInt32(gamesToFarm.First().AppID));

                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), m_cardFarmCancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                            break;
                        }

                        if (!await m_steamWeb.RefreshSessionIfNeeded().ConfigureAwait(false))
                        {
                            continue;
                        }

                        isRunning = await m_steamUserWebAPI.GetGameCardsRemainingForGame(Convert.ToUInt32(gamesToFarm.First().AppID)).ConfigureAwait(false) > 0;

                        if (!isRunning)
                        {
                            checkForNewGame = true;
                        }
                    }

                    if (!checkForNewGame)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), m_cardFarmCancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }

                    checkForNewGame = false;
                }

                m_logger.Warning("Cancelled the CardFarmTask.");
                m_cardFarmCancellationTokenSource.Dispose();
            }, m_cardFarmCancellationTokenSource.Token);
        }

        /// <summary>
        /// Cancel the task from the function "StartCheckForTradeOffers" if the token is not null
        /// </summary>
        public void StopCheckFarmCards()
        {
            try
            {
                m_cardFarmCancellationTokenSource?.Cancel();
            }
            catch (Exception exception)
            {
                if (exception.GetType() != typeof(ObjectDisposedException))
                {
                    m_logger.Error(exception.Message);
                    throw;
                }
            }
        }
    }
}
