/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using CTB.JsonClasses;

namespace CTB.HelperClasses
{
    public class ChatHandler
    {
        private readonly BotInfo m_botInfo;

        /// <summary>
        /// Constructor to initialize this class and our botInfo variable
        /// </summary>
        /// <param name="_botinfo"></param>
        public ChatHandler(BotInfo _botinfo)
        {
            m_botInfo = _botinfo;
        }

        /// <summary>
        /// All commands for the Admin, they are all explained in the function, so the user understands what they do
        /// </summary>
        /// <returns></returns>
        public string GetChatCommandsAdmin()
        {
            return "\n!commands or !C - to see all commands" +
                   "\n!generatecode or !GC - to generate the AuthCode, maybe to login to the steamcommunity manually or something else" +
                   "\n!redeem or !R - to redeem a steamgamekey" +
                   "\n!acceptfriendrequests or !AFR - enable/disable accepting friends";
        }

        /// <summary>
        /// All commands for the User, they are all explained in the function, so the user understands what they do
        /// </summary>
        /// <returns></returns>
        public string GetChatCommandsUser()
        {
            return "\n!commands or !C - to see all commands" +
                   "\n!redeem or !R - to redeem a steamgamekey" +
                   "\n!rules - to see the tradingrules and what I am accepting";
        }

        /// <summary>
        /// Print out the rules to the user so he knows what he can request
        /// </summary>
        /// <returns></returns>
        public string GetTradeRules()
        {
            string response = "";

            if(m_botInfo.Accept1on1Trades)
            {
                response += $"\nI am accepting 1:1 tradeoffers for the same set.";
            }
            if(m_botInfo.Accept1on2Trades)
            {
                response += $"\nI am accepting 1:2 tradeoffers for cross sets.";
            }

            if(m_botInfo.AcceptEscrow)
            {
                response += $"\nI am accepting escrowed tradeoffers.";
            }
            else
            {
                response += $"\nI am not accepting escrowed tradeoffers.";
            }

            if(m_botInfo.AcceptDonations)
            {
                response += $"\nI am accepting donations.";
            }

            return response;
        }
    }
}