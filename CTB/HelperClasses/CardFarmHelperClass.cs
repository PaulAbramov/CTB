using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CTB.Web;
using CTB.Web.SteamUserWeb;
using CTB.Web.SteamUserWeb.JsonClasses;
using SteamKit2;
using SteamKit2.Internal;

namespace CTB.HelperClasses
{
    public class CardFarmHelperClass
    {
        private readonly SteamUserWebAPI m_steamUserWebAPI;
        private readonly SteamWeb m_steamWeb;

        private CancellationTokenSource m_cardFarmCancellationTokenSource;

        /// <summary>
        /// Constructor to initialize variables and the class
        /// </summary>
        /// <param name="_steamWeb"></param>
        public CardFarmHelperClass(SteamWeb _steamWeb)
        {
            m_steamWeb = _steamWeb;

            m_steamUserWebAPI = new SteamUserWebAPI(_steamWeb);
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
                while(!m_cardFarmCancellationTokenSource.Token.IsCancellationRequested)
                {
                    await m_steamWeb.RefreshSessionIfNeeded();

                    List<GameToFarm> gamesToFarm = m_steamUserWebAPI.GetBadgesToFarm();

                    bool isRunning = (gamesToFarm.Count > 0);

                    if(!isRunning)
                    {
                        SetGamePlaying(0, _steamClient);
                    }

                    while(isRunning && !m_cardFarmCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        SetGamePlaying(Convert.ToInt32(gamesToFarm.First().AppID), _steamClient);

                        await Task.Delay(TimeSpan.FromMinutes(5));

                        await m_steamWeb.RefreshSessionIfNeeded();

                        isRunning = m_steamUserWebAPI.GetGameCardsRemainingForGame(Convert.ToUInt32(gamesToFarm.First().AppID)) > 0;

                        if(!isRunning)
                        {
                            checkForNewGame = true;
                        }
                    }

                    if(!checkForNewGame)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }

                    checkForNewGame = false;
                }

                Console.WriteLine("Cancelled the CardFarmTask.");
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
                if (exception.GetType() == typeof(ObjectDisposedException))
                {
                    return;
                }
                else
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }

        /// <summary>
        /// Get a reference to protobufmessages of type "GamesPlayed"
        /// Set the gameid in the reference to the given gameid parameter
        /// Send the reference to steam
        /// 
        /// With this function we can idle for cards
        /// </summary>
        /// <param name="_gameID"></param>
        /// <param name="_steamClient"></param>
        private void SetGamePlaying(int _gameID, SteamClient _steamClient)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> gamePlaying = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            gamePlaying.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed { game_id = new GameID(_gameID) });

            _steamClient.Send(gamePlaying);
        }
    }
}
