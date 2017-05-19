using System;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;

namespace CTB.Web
{
    public class SteamWeb
    {
        public string SessionID { get; private set; }
        public string SteamLogin { get; private set; }
        public string SteamLoginSecure { get; private set; }

        public readonly WebHelper m_WebHelper = new WebHelper();
        private const string SteamCommunityHost = "steamcommunity.com";
        
        private CancellationTokenSource m_authenticateUserTokenSource;

        /// <summary>
        /// Initialize the cancellation token, we are going to need it to cancel the whole task
        /// Create a new task, which runs parallel
        /// While the task is not canceled try to authenticate the user every hour, so we are getting reconnected if we are disconnected
        /// 
        /// After canceling the task, print a message and free the allocated memory
        /// </summary>
        /// <param name="_steamClient"></param>
        /// <param name="_webAPIUserNonce"></param>
        public void StartAuthenticateUserLoop(SteamClient _steamClient, string _webAPIUserNonce)
        {
            m_authenticateUserTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while(!m_authenticateUserTokenSource.Token.IsCancellationRequested)
                {
                    AuthenticateUser(_steamClient, _webAPIUserNonce);

                    await Task.Delay(TimeSpan.FromHours(1));
                }

                Console.WriteLine("Cancelled the AuthenticateTask.");
                m_authenticateUserTokenSource.Dispose();
            }, m_authenticateUserTokenSource.Token);

            //task.Dispose();
        }

        /// <summary>
        /// Cancel the task from the function "StartAuthenticateUserLoop" if the token is not null
        /// </summary>
        public void StopAuthenticateUserLoop()
        {
            m_authenticateUserTokenSource?.Cancel();
        }

        /// <summary>
        /// Authenticate the user at the online services of Steam
        /// </summary>
        public bool AuthenticateUser(SteamClient _steamClient, string _webAPIUserNonce )
        {
            // Get the interface for the authentication of the steamuser
            using (dynamic authenticator = WebAPI.GetInterface("ISteamUserAuth"))
            {
                SessionID = Convert.ToBase64String(Encoding.UTF8.GetBytes(_steamClient.SteamID.ToString()));

                // Generate a random block of 32 bytes for the security
                byte[] sessionKey = CryptoHelper.GenerateRandomBlock(32);

                // Encrypt the above generated block of bytes with the Steam systems public key
                byte[] encryptedSessionKey;
                using (RSACrypto rsa = new RSACrypto(KeyDictionary.GetPublicKey(_steamClient.ConnectedUniverse)))
                {
                    encryptedSessionKey = rsa.Encrypt(sessionKey);
                }

                // Copy the string into the bytearray
                byte[] loginkey = new byte[_webAPIUserNonce.Length];
                Array.Copy(Encoding.ASCII.GetBytes(_webAPIUserNonce), loginkey, _webAPIUserNonce.Length);

                // AES encrypt the loginkey with our sessionkey
                byte[] encryptedLoginKey = CryptoHelper.SymmetricEncrypt(loginkey, sessionKey);

                // The value returned by the AuthenticateUser function are KeyValues
                KeyValue authResult;

                // Always TRY to work with interfaces, because it could go wrong and destroy everything
                try
                {
                    authResult = authenticator.AuthenticateUser(
                        steamid: _steamClient.SteamID.ConvertToUInt64(),
                        sessionkey: Encoding.ASCII.GetString(WebUtility.UrlEncodeToBytes(encryptedSessionKey, 0, encryptedSessionKey.Length)),
                        encrypted_loginkey: Encoding.ASCII.GetString(WebUtility.UrlEncodeToBytes(encryptedLoginKey, 0, encryptedLoginKey.Length)),
                        method: WebRequestMethods.Http.Post,
                        secure: true);
                }
                catch (Exception e)
                {
                    if(!e.Message.Contains("403"))
                    {
                        Console.WriteLine(e);
                    }
                    return false;
                }

                // Double check if it is null then return because we do not have anything to do here
                if (authResult == null)
                {
                    return false;
                }

                // Set the cookies
                SteamLogin = authResult["token"].Value;
                SteamLoginSecure = authResult["tokensecure"].Value;

                m_WebHelper.m_CookieContainer.Add(new Cookie("sessionid", SessionID, string.Empty, SteamCommunityHost));
                m_WebHelper.m_CookieContainer.Add(new Cookie("steamLogin", SteamLogin, string.Empty, SteamCommunityHost));
                m_WebHelper.m_CookieContainer.Add(new Cookie("steamLoginSecure", SteamLoginSecure, string.Empty, SteamCommunityHost));

                return true;
            }
        }
    }
}
