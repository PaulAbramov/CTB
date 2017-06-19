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
using System.Linq;
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
        /// <param name="_keyToActivate"></param>
        /// <returns></returns>
        public async Task<PurchaseResponseCallback> RedeemKey(string _keyToActivate)
        {
            ClientMsgProtobuf<CMsgClientRegisterKey> registerKey = new ClientMsgProtobuf<CMsgClientRegisterKey>(EMsg.ClientRegisterKey)
            {
                SourceJobID = Client.GetNextJobID()
            };

            registerKey.Body.key = _keyToActivate;

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
        /// Function to give a response to the person who sent us the key
        /// Get a callbackobject from RedeemKey, which holds informations about the activation and the game
        /// create a string and return it, so we can send  the user a response and he knows if the Key is still valid
        /// </summary>
        /// <param name="_keyToActivate"></param>
        /// <returns></returns>
        public async Task<string> RedeemKeyResponse(string _keyToActivate)
        {
            PurchaseResponseCallback activatedResponse = await RedeemKey(_keyToActivate);

            return $"Status: {activatedResponse.m_Result}/{activatedResponse.m_PurchaseResultDetail}, {string.Join(",", activatedResponse.m_Items.Select(_key => $"Key: [ {_keyToActivate} ] Game: [ {_key.Key}/{_key.Value} ]").ToArray())}";
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
