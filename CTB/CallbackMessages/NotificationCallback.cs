/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using SteamKit2.Internal;

namespace CTB.CallbackMessages
{
    /// <summary>
    /// Custom NotificationCallback which will be sent to steam and returned with some filled informations
    /// Has to inherit from "CallbackMsg" to use it as a Callback with steam
    /// Using it as a callback, we want a list of notifications we have
    /// </summary>
    public class NotificationCallback : CallbackMsg
    {
        public readonly List<ENotification> m_Notification;

        /// <summary>
        /// First Constructor
        /// 
        /// Pass a jobID so we can identify the callback if we are going to receive it as an answer from steam
        /// From the returned "_clientUserNotifications" we want to parse the notifications into the list of tradingnotifications
        /// </summary>
        /// <param name="_jobID"></param>
        /// <param name="_clientUserNotifications"></param>
        public NotificationCallback(JobID _jobID, CMsgClientUserNotifications _clientUserNotifications)
        {
            JobID = _jobID;

            if(_clientUserNotifications.notifications.Count > 0)
            {
                m_Notification = new List<ENotification>(_clientUserNotifications.notifications.Select(_notification => (ENotification)_notification.user_notification_type));
            }
        }
    }

    /// <summary>
    /// Enum which allows us to handle specific notifications
    /// </summary>
    public enum ENotification
    {
        UNKNOWN = 0,
        TRADING = 1,
    }
}
