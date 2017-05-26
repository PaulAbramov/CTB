using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using CTB.HelperClasses;
using CTB.JsonClasses;
using SteamAuth;
using SteamKit2;
using SteamWeb = CTB.Web.SteamWeb;

namespace CTB
{
    public class Bot
    {
#region SteamKit2 variables
        private readonly SteamClient m_steamClient;
        private readonly CallbackManager m_callbackManager;
        private readonly SteamUser m_steamUser;
        private readonly SteamUser.LogOnDetails m_steamUserLogonDetails;
        private readonly SteamFriends m_steamFriends;
#endregion

        private readonly SteamWeb m_steamWeb;
        private readonly TradeOfferHelperClass m_tradeOfferHelper;
        private readonly SteamFriendsHelper m_steamFriendsHelper;
        private readonly MobileHelper m_mobileHelper;

        private string m_webAPIUserNonce;

        private readonly bool m_neededInfosAreGiven;
        private readonly string m_botName;

        /// <summary>
        /// initialize the Bot
        /// </summary>
        /// <param name="_botInfo"></param>
        public Bot(BotInfo _botInfo)
        {
            // The Steamclient we are going to log on to
            m_steamClient = new SteamClient();

            // CallbackManager, which handles all callbacks we are going to get from the client
            m_callbackManager = new CallbackManager(m_steamClient);

            m_steamUser = m_steamClient.GetHandler<SteamUser>();

            m_steamFriends = m_steamClient.GetHandler<SteamFriends>();

            #region Callbacks
            #region SteamClient Callbacks
            m_callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            m_callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            #endregion

            #region SteamUser Callbacks
            m_callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            m_callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            m_callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            #endregion

            #region SteamFriends Callbacks
            m_callbackManager.Subscribe<SteamFriends.FriendsListCallback>(OnLoadedFriendsList);
            #endregion
            #endregion

            // Check if all needed informations are given
            m_neededInfosAreGiven = CheckForNeededBotInfo(_botInfo);

            m_botName = _botInfo.BotName;

            m_steamUserLogonDetails = new SteamUser.LogOnDetails
            {
                Username = _botInfo.Username,
                Password = _botInfo.Password,
                ShouldRememberPassword = true
            };

            m_steamWeb = new SteamWeb();
            m_mobileHelper = new MobileHelper();
            m_tradeOfferHelper = new TradeOfferHelperClass(m_mobileHelper, m_steamWeb, _botInfo);

            m_steamFriendsHelper = new SteamFriendsHelper();
        }

        /// <summary>
        /// Start the Bot
        /// If not all needed informations are given, end the Program
        /// Load the Steam's serverlist so we do not try to connect to an offline server
        /// Load the current bot sentryfile if it does exist, so we do not have to enter the E-Mail authcode everytime
        /// Start the connection to the SteamClient and start a never ending loop to listen to callbacks
        /// </summary>
        public void Start()
        {
            if(!m_neededInfosAreGiven)
            {
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Connecting to Steam...");

            // Load the serverlist to get an available server to connect to
            // Prevent from trying to login to offline servers
            try
            {
                SteamDirectory.Initialize().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load serverlist with the message: " + e.Message);
            }

            FileInfo sentryFileInfo = new FileInfo($"Files/Authfiles/{m_steamUserLogonDetails.Username}.sentryfile");

            if(sentryFileInfo.Exists && sentryFileInfo.Length > 0)
            {
                m_steamUserLogonDetails.SentryFileHash = CryptoHelper.SHAHash(File.ReadAllBytes(sentryFileInfo.FullName));
            }

            // When the serverlist is loaded, try to connect to a server
            m_steamClient.Connect();

            // After a successful login check every second if we have a callback returned to us
            while(true)
            {
                m_callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// Perform action on successful connection
        /// Login to the Useraccount
        /// </summary>
        /// <param name="_callback"></param>
        private void OnConnected(SteamClient.ConnectedCallback _callback)
        {
            Console.WriteLine("Connected to Steam!");
            
            m_steamUser.LogOn(m_steamUserLogonDetails);
        }

        /// <summary>
        /// Throw a message if we are successfully logged on
        /// Authenticate in the web and start polling for trades
        /// 
        /// If we do not have linked a authenticator to our phone, print a message to let the user know to enter the code sent to the email
        /// If we do have linked a authenticator to our phone, try to get the authcode from our mobileHelper
        /// If the returned answer is empty tell the user to link it via the bot or add the .maFile to the directory in a specific format
        /// If the returned answer is not empty tell the user that we have generated the 2FA authcode
        /// 
        /// If the entered/ returned authCode is false, align our time with the time of the steamservers and try again the upper case
        /// 
        /// Throw a message if there occured an error
        /// </summary>
        /// <param name="_callback"></param>
        private void OnLoggedOn(SteamUser.LoggedOnCallback _callback)
        {
            m_steamUserLogonDetails.AuthCode = "";

            m_webAPIUserNonce = _callback.WebAPIUserNonce;

            switch (_callback.Result)
            {
                case EResult.OK:
                    Console.WriteLine("Successfully logged on.");

                    bool loggedon = m_steamWeb.AuthenticateUser(m_steamClient, m_webAPIUserNonce);

                    if (loggedon)
                    {
                        Console.WriteLine("Successfully authenticated the user in the web.");

                        m_tradeOfferHelper.StartCheckForTradeOffers(m_steamFriendsHelper, m_steamClient.SteamID);
                    }
                    break;
               case EResult.AccountLogonDenied:
                    Console.WriteLine("Enter the auth code sent to the email at {0}: ", _callback.EmailDomain);
                    m_steamUserLogonDetails.AuthCode = Console.ReadLine();
                    break;
                case EResult.InvalidLoginAuthCode:
                    Console.WriteLine("Enter the new auth code sent to the email at {0}: ", _callback.EmailDomain);
                    m_steamUserLogonDetails.AuthCode = Console.ReadLine();
                    break;
                case EResult.AccountLoginDeniedNeedTwoFactor:
                    string twoFactorCode = m_mobileHelper.GetMobileAuthCode(m_steamUserLogonDetails.Username);

                    if (string.IsNullOrEmpty(twoFactorCode))
                    {
                        Console.WriteLine("Be sure to link the account to the mobileauthenticator via the bot or add the .maFile to the 2FAFiles directory with the format: 'username.auth'");
                        Console.WriteLine("If you have your phone already linked enter your code: ");
                        m_steamUserLogonDetails.TwoFactorCode = Console.ReadLine();
                    }
                    else
                    {
                        m_steamUserLogonDetails.TwoFactorCode = twoFactorCode;
                        Console.WriteLine("2FA-Code was generated.");
                    }
                    break;
                case EResult.TwoFactorCodeMismatch:
                    TimeAligner.AlignTime();

                    twoFactorCode = m_mobileHelper.GetMobileAuthCode(m_steamUserLogonDetails.Username);

                    if (string.IsNullOrEmpty(twoFactorCode))
                    {
                        Console.WriteLine("Be sure to link the account to the mobileauthenticator via the bot");
                        Console.WriteLine("If you have your phone already linked enter your code: ");
                        m_steamUserLogonDetails.TwoFactorCode = Console.ReadLine();
                    }
                    else
                    {
                        m_steamUserLogonDetails.TwoFactorCode = twoFactorCode;
                        Console.WriteLine("2FA-Code was generated.");
                    }
                    break;

                default:
                    Console.WriteLine("Unable to logon to Steam: {0} / {1}", _callback.Result, _callback.ExtendedResult);
                    break;
            }
        }

        /// <summary>
        /// Authorize the PC/Machine for this account, so we do not have to enter the authcode on every login
        /// </summary>
        /// <param name="_callback"></param>
        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback _callback)
        {
            Console.WriteLine("Updateing sentryfile...");

            // variables we need later in another scope
            int fileSize;
            byte[] sentryHash;

            // Create a filestream to a file named like the current bots username
            // With this filestream get all the info we got from the "_callback"
            // Calculate the Hashvalue of the info inside the filestream
            using (FileStream filestream = File.Open($"Files/Authfiles/{m_steamUserLogonDetails.Username}.sentryfile", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                filestream.Seek(_callback.Offset, SeekOrigin.Begin);
                filestream.Write(_callback.Data, 0, _callback.BytesToWrite);
                fileSize = (int) filestream.Length;

                filestream.Seek(0, SeekOrigin.Begin);

                using (SHA1CryptoServiceProvider shaHash = new SHA1CryptoServiceProvider())
                {
                    sentryHash = shaHash.ComputeHash(filestream);
                }
            }

            // We have obtained the filesize and the Hashvalue of the given Data
            // The left info we need, we are going to get from the "_callback" object
            // If we were successful, the PC is now authorized
            m_steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                Result          = EResult.OK,
                Offset          = _callback.Offset,
                BytesWritten    = _callback.BytesToWrite,
                FileName        = _callback.FileName,
                FileSize        = fileSize,
                JobID           = _callback.JobID,
                LastError       = 0,
                OneTimePassword = _callback.OneTimePassword,
                SentryFileHash  = sentryHash
            });

            Console.WriteLine("Done updating sentryfile!");
        }

        /// <summary>
        /// Callback will be called if our friendslist is successfully loaded
        /// Set our state to online and change our name to the name inside the config
        /// </summary>
        /// <param name="_callback"></param>
        private void OnLoadedFriendsList(SteamFriends.FriendsListCallback _callback)
        {
            m_steamFriends.SetPersonaState(EPersonaState.Online);

            if (!string.IsNullOrEmpty(m_botName))
            {
                m_steamFriends.SetPersonaName(m_botName);
            }
        }

        /// <summary>
        /// Throw a Message on Loggedoff
        /// </summary>
        /// <param name="_callback"></param>
        private void OnLoggedOff(SteamUser.LoggedOffCallback _callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", _callback.Result);

            m_tradeOfferHelper.StopCheckForTradeOffers();
        }

        /// <summary>
        /// Perform action if we are disconnected from the Steamservers
        /// 
        /// Retry to connect to the steamservers after 5 seconds
        /// </summary>
        /// <param name="_callback"></param>
        private void OnDisconnected(SteamClient.DisconnectedCallback _callback)
        {
            Console.WriteLine("Disconnected from Steam, try to connect again in 5 seconds!");

            m_tradeOfferHelper.StopCheckForTradeOffers();

            Thread.Sleep(TimeSpan.FromSeconds(5));

            m_steamClient.Connect();
        }

        /// <summary>
        /// Check all properties of the passed botinfo so we all important details we need are set
        /// </summary>
        /// <param name="_botInfo"> where we want to check the properties </param>
        /// <returns> true if everything is okay </returns>
        private bool CheckForNeededBotInfo(BotInfo _botInfo)
        {
            if(string.IsNullOrEmpty(_botInfo.Username))
            {
                Console.WriteLine("Username is not set in the config file, please set it!");
                return false;
            }
            if (string.IsNullOrEmpty(_botInfo.Password))
            {
                Console.WriteLine("Password is not set in the config file, please set it!");
                return false;
            }
            if (string.IsNullOrEmpty(_botInfo.APIKey))
            {
                Console.WriteLine("APIKey is not set in the config file, please set it!");
                return false;
            }

            return true;
        }
    }
}
