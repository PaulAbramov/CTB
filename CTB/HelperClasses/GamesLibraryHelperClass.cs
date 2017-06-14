/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using SteamKit2;
using SteamKit2.Internal;

namespace CTB.HelperClasses
{
    public class GamesLibraryHelperClass
    {
        /// <summary>
        /// Get a reference to protobufmessages of type "GamesPlayed"
        /// Set the gameid in the reference to the given gameid parameter
        /// Send the reference to steam
        /// 
        /// With this function we can idle for cards
        /// </summary>
        /// <param name="_gameID"></param>
        /// <param name="_steamClient"></param>
        public void SetGamePlaying(int _gameID, SteamClient _steamClient)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> gamePlaying = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            gamePlaying.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed { game_id = new GameID(_gameID) });

            _steamClient.Send(gamePlaying);
        }

        public void RedeemKey(string _key, SteamClient _steamClient)
        {
            ClientMsgProtobuf<CMsgClientRegisterKey> registerKey = new ClientMsgProtobuf<CMsgClientRegisterKey>(EMsg.ClientRegisterKey);

            registerKey.Body.key = _key;

            _steamClient.Send(registerKey);
        }
    }
}
