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
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SteamAuth;
using SteamKit2;

namespace CTB.HelperClasses
{
    public class MobileHelper
    {
        private SteamGuardAccount m_steamGuardAccount;

        private readonly Logger.Logger m_logger;

        public MobileHelper(Logger.Logger _logger)
        {
            m_logger = _logger;
        }

        /// <summary>
        /// If the account isn't initialized, check if we have an authfile for this account and deserialize it
        /// If the account is valid create a authcode, else return an empty string
        /// If we have already a account, just return the authcode without deserializing the file
        /// </summary>
        /// <returns></returns>
        public string GetMobileAuthCode(string _userName)
        {
            if(m_steamGuardAccount == null)
            {
                string authFile = Path.Combine("Files/2FAFiles", $"{_userName}.auth");

                if (File.Exists(authFile))
                {
                    m_steamGuardAccount = JsonConvert.DeserializeObject<SteamGuardAccount>(File.ReadAllText(authFile));
                    return m_steamGuardAccount.GenerateSteamGuardCode();
                }
            }
            else
            {
                return m_steamGuardAccount.GenerateSteamGuardCode();
            }
            
            return string.Empty;
        }

        /// <summary>
        /// If we didn't link our mobile via the bot, just return a message
        /// If we did link our mobile via the bot, we have to get the SteamLogin and SteamLoginSecure, which we get from logging in to the web
        /// Pass the sessionID because we need it aswell, generate a uint64 SteamID from the SessionID, without this we can't fetch the confirmations
        /// With these values we can get all confirmations and accept or deny them, without it will throw errors
        /// </summary>
        public void ConfirmAllTrades(string _steamLogin, string _steamLoginSecure, string _sessionID)
        {
            if (m_steamGuardAccount == null)
            {
                m_logger.Warning("Bot account does not have 2FA enabled.");
            }
            else
            {
                m_steamGuardAccount.Session = new SessionData
                {
                    SteamLogin = _steamLogin,
                    SteamLoginSecure = _steamLoginSecure,
                    SessionID = _sessionID,
                    SteamID = new SteamID(Encoding.UTF8.GetString(Convert.FromBase64String(_sessionID))).ConvertToUInt64()
                };

                Confirmation[] confirmations = m_steamGuardAccount.FetchConfirmations();

                if (confirmations != null)
                {
                    foreach (Confirmation confirmation in confirmations)
                    {
                        bool confirmedTrade = m_steamGuardAccount.AcceptConfirmation(confirmation);

                        if(confirmedTrade)
                        {
                            m_logger.Info($"Confirmed {confirmation.Description}, (Confirmation ID #{confirmation.ID})");

                        }
                        else
                        {
                            m_logger.Warning($"Could not confirm {confirmation.Description}, (Confirmation ID #{confirmation.ID})");
                        }
                    }
                }
                else
                {
                    m_logger.Error("Mobilehelper: Must Login");
                }
            }
        }
    }
}