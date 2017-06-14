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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTB.HelperClasses
{
    public class ChatHandler
    {
        /// <summary>
        /// All commands for the Admin, they are all explained in the function, so the user understands what they do
        /// </summary>
        /// <returns></returns>
        public string GetChatCommandsAdmin()
        {
            return "\n!commands or !C - to see all commands" +
                   "\n!generatecode or !GC - to generate the AuthCode, maybe to login to the steamcommunity manually or something else" +
                   "\n!reddem or !R - to reedem a steamgamekey" +
                   "\n!acceptfriendrequests or !AFR - enable/disable accepting friends";
        }
    }
}