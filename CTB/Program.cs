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
using CTB.JsonClasses;
using Newtonsoft.Json;

namespace CTB
{
    class Program
    {
        /// <summary>
        /// First check if there is a configfile, so we do not have to enter the username and password everytime
        /// If there is no config file, create one and enter the username and the password into it
        /// Close the program
        /// 
        /// If there is a config file, create the Bot with the config file and start the Bot
        /// </summary>
        /// <param name="_args"></param>
        private static void Main(string[] _args)
        {
            const string configPath = "Files/config.json";

            BotInfo botInfo = new BotInfo();

            // Create the Directories to store custom files
            if(!Directory.Exists("Files"))
            {
                Directory.CreateDirectory("Files");
            }
            if(!Directory.Exists("Files/Authfiles"))
            {
                Directory.CreateDirectory("Files/Authfiles");
            }
            if (!Directory.Exists("Files/2FAFiles"))
            {
                Directory.CreateDirectory("Files/2FAFiles");
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine("There is no config, create one in the rootfolder, add username and password: ");

                Console.Write("Username: ");
                botInfo.Username = Console.ReadLine();

                Console.Write("Password: ");
                botInfo.Password = Console.ReadLine();

                File.WriteAllText(configPath, JsonConvert.SerializeObject(botInfo, Formatting.Indented));

                Console.WriteLine("Console will be closed, fill in all values in the config.txt and restart the Bot");
                Console.ReadKey();
                Environment.Exit(0);
            }
            else
            {
                botInfo = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText(configPath));

                Bot bot = new Bot(botInfo);

                bot.Start();
            }
        }
    }
}