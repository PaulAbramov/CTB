using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SteamKit2;

namespace CTB.Web
{
    public class SteamWeb
    {
        public string SessionID { get; private set; }
        public string SteamLogin { get; private set; }
        public string SteamLoginSecure { get; private set; }

        public readonly WebHelper m_WebHelper = new WebHelper();
        public readonly string m_APIKey;

        private SteamClient m_steamClient;
        private string m_webAPIUserNonce;

        public readonly string m_SteamCommunityHost = "steamcommunity.com";
        public readonly string m_APISteamAddress = "http://api.steampowered.com/{0}/{1}/{2}";
        

        /// <summary>
        /// Initialize the SteamWeb object with the apikey
        /// </summary>
        /// <param name="_apiKey"></param>
        public SteamWeb(string _apiKey)
        {
            m_APIKey = _apiKey;
        }

        /// <summary>
        /// Send a request to the homepage of steam, check if there is something with "profiles/OurSteamID64/friends"
        /// If there is such a content inside the string, then we are still authenticated return true, so we know we are loggedon
        /// If there is not such a content inside the string, then we are not authenticated and try to authenticate to the web again
        /// </summary>
        /// <returns></returns>
        public bool RefreshSessionIfNeeded()
        {
            string response = m_WebHelper.GetStringFromRequest("http://steamcommunity.com/my/");

            bool isNotLoggedOn = response.Contains("Sign In");

            if (isNotLoggedOn)
            {
                Console.WriteLine("Reauthenticating...");

                return AuthenticateUser(m_steamClient, m_webAPIUserNonce);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Authenticate the user at the online services of Steam
        /// </summary>
        public bool AuthenticateUser(SteamClient _steamClient, string _webAPIUserNonce )
        {
            m_steamClient = _steamClient;
            m_webAPIUserNonce = _webAPIUserNonce;

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

                m_WebHelper.m_CookieContainer.Add(new Cookie("sessionid", SessionID, string.Empty, m_SteamCommunityHost));
                m_WebHelper.m_CookieContainer.Add(new Cookie("steamLogin", SteamLogin, string.Empty, m_SteamCommunityHost));
                m_WebHelper.m_CookieContainer.Add(new Cookie("steamLoginSecure", SteamLoginSecure, string.Empty, m_SteamCommunityHost));

                return true;
            }
        }
    }
}