using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CTB;
using CTB.JsonClasses;
using Newtonsoft.Json;

namespace CTBUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<BotListItem> m_ListOfBots = new List<BotListItem>();

        private string m_pathToFiles;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

            PopulateBotList();
        }

        private void Initialize()
        {
            Console.SetOut(new ControlWriter.ControlWriter(BotsOutput));

            if (!Directory.Exists("Files"))
            {
                Directory.CreateDirectory("Files");
            }

            string path = GetParentPathAtPosition(Environment.CurrentDirectory, 3);

            path += @"\CTB\bin\";

            string releasePath = path + "Release/Files";
            string debugPath = path + "Debug/Files";

            if (Directory.Exists(releasePath) && Directory.GetFiles(releasePath).Length > 0)
            {
                m_pathToFiles = releasePath;
            }
            else if(Directory.Exists(debugPath) && Directory.GetFiles(debugPath).Length > 0)
            {
                m_pathToFiles = debugPath;
            }

            CreateFolderStructureAndCopyFiles();

            Console.WriteLine(m_pathToFiles);
        }

        private void PopulateBotList()
        {
            foreach(string file in Directory.GetFiles(m_pathToFiles + "/Configs"))
            {
                if(file.Contains(".json"))
                {
                    m_ListOfBots.Add(new BotListItem{m_Name = file.Split('\\').Last().Split('.')[0], m_Selected = false, m_Status = "offline"});
                }
            }

            BotList.ItemsSource = m_ListOfBots;
        }

        private void Button_Click(object _sender, RoutedEventArgs _e)
        {
            foreach(object botName in BotList.Items)
            {
                Task.Run(() =>
                {
                    BotListItem newBot = (BotListItem)botName;

                    if (newBot.m_Selected)
                    {
                        Console.WriteLine(newBot.m_Name);

                        BotInfo botInfo = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText(m_pathToFiles + "/Configs/" + newBot.m_Name + ".json"));

                        Bot bot = new Bot(botInfo);

                        bot.Start();
                    }
                });
            }
        }

        /// <summary>
        /// TODO rework and just split the path and get the item at the position
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        private string GetParentPathAtPosition(string _path, int _index)
        {
            string path = _path;

            for(int i = 0; i < _index; i++)
            {
                path = Directory.GetParent(path).ToString();
            }

            return path;
        }

        /// <summary>
        /// Create the topfolderstructure and copy all files from these into our folderstructure
        /// 
        /// TODO build it recursively?
        /// </summary>
        private void CreateFolderStructureAndCopyFiles()
        {
            string[] directories = Directory.GetDirectories(m_pathToFiles);

            foreach (string directory in directories)
            {
                string directoryLastString = directory.Split('\\').Last();

                DirectoryInfo directoryInfo = null;

                if (!Directory.Exists($"Files/{directoryLastString}"))
                {
                    directoryInfo = Directory.CreateDirectory($"Files/{directoryLastString}");
                }

                if(directoryInfo != null)
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        File.Copy(file, directoryInfo.FullName + "/" + Path.GetFileName(file));
                    }
                }
            }
        }
    }
}
