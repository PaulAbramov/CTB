/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

namespace CTB.Web.SteamUserWeb.JsonClasses
{
    public class GameToFarm
    {
        public string AppID { get; set; }
        public int CardsToEarn { get; set; }
        public int CardsEarned { get; set; }
        public string HoursPlayed { get; set; }
        public string Name { get; set; }
    }
}
