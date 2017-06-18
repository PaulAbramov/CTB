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
using System.Threading.Tasks;
using CTB.CallbackMessages;
using SteamKit2;
using SteamKit2.Internal;

namespace CTB.HelperClasses
{
    /// <summary>
    /// We are sending messages to steam and want to receive an answer
    /// Also we do want to handle the answers and use them with the CallbackManager
    /// Therefore we have to inherit from "ClientMsgHandler"
    /// </summary>
    public class GamesLibraryHelperClass : ClientMsgHandler
    {
        /// <summary>
        /// Must implement because of "ClientMsgHandler"
        /// Here we get a response from steam with a packettype
        /// we want to handle the packet
        /// </summary>
        /// <param name="_packetMsg"></param>
        public override void HandleMsg(IPacketMsg _packetMsg)
        {
            switch(_packetMsg.MsgType)
            {
                case EMsg.ClientPurchaseResponse:
                    HandlePurchaseResponse(_packetMsg);
                    break;
            }
        }

        /// <summary>
        /// Get a reference to protobufmessages of type "GamesPlayed"
        /// Set the gameid in the reference to the given gameid parameter
        /// Send the reference to steam
        /// 
        /// With this function we can idle for cards
        /// We can use "Client" from the inherited class "ClientMsgHandler"
        /// </summary>
        /// <param name="_gameID"></param>
        public void SetGamePlaying(int _gameID)
        {
            ClientMsgProtobuf<CMsgClientGamesPlayed> gamePlaying = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            gamePlaying.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed { game_id = new GameID(_gameID) });

            Client.Send(gamePlaying);
        }

        /// <summary>
        /// Get a reference to protobufmessages of type "ClientRegisterKey"
        /// Set the jobID of this protobufmsg so we know it is the same request when we receive an answer
        /// Set the key of the body to the gamekey we passed
        /// Send the reference to steam
        /// 
        /// Also we do want to return a callback directly so we can handle it and answer the person who redeemed the key
        /// 
        /// With this function we can register gamekeys to our gameslibrary
        /// We can use "Client" from the inherited class "ClientMsgHandler"
        /// </summary>
        /// <param name="_key"></param>
        /// <returns></returns>
        public async Task<PurchaseResponseCallback> RedeemKey(string _key)
        {
            ClientMsgProtobuf<CMsgClientRegisterKey> registerKey = new ClientMsgProtobuf<CMsgClientRegisterKey>(EMsg.ClientRegisterKey)
            {
                SourceJobID = Client.GetNextJobID()
            };

            registerKey.Body.key = _key;

            Client.Send(registerKey);

            try
            {
                return await new AsyncJob<PurchaseResponseCallback>(Client, registerKey.SourceJobID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// We want to handle the response for the specific type "PurchasesResponse"
        /// To handle it post a callback which will be caught by the callbackmanager
        /// </summary>
        /// <param name="_packetMsg"></param>
        private void HandlePurchaseResponse(IPacketMsg _packetMsg)
        {
            if (_packetMsg == null)
            {
                return;
            }

            ClientMsgProtobuf<CMsgClientPurchaseResponse> response = new ClientMsgProtobuf<CMsgClientPurchaseResponse>(_packetMsg);
            Client.PostCallback(new PurchaseResponseCallback(_packetMsg.TargetJobID, response.Body));
        }
    }
}
