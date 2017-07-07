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

namespace CTB.CallbackMessages
{
    public class CustomHandler : ClientMsgHandler
    {
        /// <summary>
        /// Must implement because of "ClientMsgHandler"
        /// Here we get a response from steam with a packettype
        /// we want to handle the packet
        /// </summary>
        /// <param name="_packetMsg"></param>
        public override void HandleMsg(IPacketMsg _packetMsg)
        {
            switch (_packetMsg.MsgType)
            {
                case EMsg.ClientUserNotifications:
                    HandleUserNotifications(_packetMsg);
                    break;
            }
        }

        /// <summary>
        /// We want to handle the response for the specific type "UserNotifications"
        /// To handle it, post a callback which will be caught by the callbackmanager
        /// </summary>
        /// <param name="_packetMsg"></param>
        private void HandleUserNotifications(IPacketMsg _packetMsg)
        {
            if(_packetMsg == null)
            {
                return;
            }

            ClientMsgProtobuf<CMsgClientUserNotifications> response = new ClientMsgProtobuf<CMsgClientUserNotifications>(_packetMsg);
            Client.PostCallback(new NotificationCallback(_packetMsg.TargetJobID, response.Body));
        }
    }
}
