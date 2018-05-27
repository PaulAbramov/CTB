using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CTB;
using CTB.JsonClasses;
using CTBUI.Properties;
using Newtonsoft.Json;

namespace CTBUI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<BotListItem> m_ListOfBots = new ObservableCollection<BotListItem>();

        private readonly Dictionary<string, Bot> m_runningBotDictionary = new Dictionary<string, Bot>();
        private readonly Dictionary<string, Task> m_runningTasks = new Dictionary<string, Task>();

        private string m_pathToFiles;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        /// <summary>
        /// Set the itemsource to our observableCollection so it updates if there are changes to the configs
        /// Create the Folderstructure and add the bots to the list
        /// </summary>
        private void Initialize()
        {
            BotList.ItemsSource = m_ListOfBots;

            CreateFolderStructure();
            
            PopulateBotList();
        }

        /// <summary>
        /// Create the folderstructure for the project
        /// </summary>
        private void CreateFolderStructure()
        {
            Console.SetOut(new ControlWriter.ControlWriter(BotsOutput));
			Console.SetIn(new ControlReader.ControlReader(BotsOutput));

            if (!Directory.Exists("Files"))
            {
                Directory.CreateDirectory("Files");
            }

            string path = GetParentPathAtPosition(Environment.CurrentDirectory, 3);

            path += @"\CTB\bin\";

            string releasePath = path + "Release/Files";
            string debugPath = path + "Debug/Files";

            if (Directory.Exists(releasePath) && Directory.GetDirectories(releasePath).Length > 0)
            {
                m_pathToFiles = releasePath;
                CreateFolderStructureAndCopyFiles();
            }
            else if(Directory.Exists(debugPath) && Directory.GetDirectories(debugPath).Length > 0)
            {
                m_pathToFiles = debugPath;
                CreateFolderStructureAndCopyFiles();
            }
            else
            {
                if (!Directory.Exists(Settings.Default.AuthfilesPath))
                {
                    Directory.CreateDirectory(Settings.Default.AuthfilesPath);
                }
                if (!Directory.Exists(Settings.Default.TwoFAfilesPath))
                {
                    Directory.CreateDirectory(Settings.Default.TwoFAfilesPath);
                }
                if (!Directory.Exists(Settings.Default.ConfigsPath))
                {
                    Directory.CreateDirectory(Settings.Default.ConfigsPath);
                }
            }
        }

        /// <summary>
        /// Clear the list so we do not have multiple same entries
        /// Add every config to the list
        /// </summary>
        private void PopulateBotList()
        {
            m_ListOfBots.Clear();

            foreach(string file in Directory.GetFiles(Settings.Default.ConfigsPath))
            {
                if(file.Contains(".json"))
                {
                    m_ListOfBots.Add(new BotListItem{m_Name = file.Split('/').Last().Split('.')[0], m_Selected = false, m_Status = "offline"});
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        private static string GetParentPathAtPosition(string _path, int _index)
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

        /// <summary>
        /// open the window for a new config
        /// if the window gets closed update the list again
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_e"></param>
        private void AddClick(object _sender, RoutedEventArgs _e)
        {
            NewConfig config = new NewConfig();
            config.Show();

            config.Closed += (_object, _args) => { PopulateBotList(); };
        }

        /// <summary>
        /// go trough every selected bot and delete the files
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_e"></param>
        private async void RemoveClick(object _sender, RoutedEventArgs _e)
        {
            foreach(object bot in BotList.Items)
            {
                if(((BotListItem)bot).m_Selected)
                {
                    await StopBotAndDisposeTask(((BotListItem)bot).m_Name).ConfigureAwait(false);

                    File.Delete($"{Settings.Default.ConfigsPath}{((BotListItem) bot).m_Name}.json");
                }
            }

            PopulateBotList();
        }

        /// <summary>
        /// TODO maybe make Start async
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_e"></param>
        private void StartClick(object _sender, RoutedEventArgs _e)
        {
            foreach (object botName in BotList.Items)
            {
                BotListItem newBot = (BotListItem)botName;
                if (newBot.m_Selected && !m_runningTasks.ContainsKey(newBot.m_Name))
                {
                    m_runningTasks.Add(newBot.m_Name, Task.Run(() =>
                    {
                        BotInfo botInfo = JsonConvert.DeserializeObject<BotInfo>(File.ReadAllText(Settings.Default.ConfigsPath + newBot.m_Name + ".json"));

                        if (!m_runningBotDictionary.ContainsKey(botInfo.Username))
                        {
                            Bot bot = new Bot(botInfo);

                            m_runningBotDictionary.Add(botInfo.Username, bot);

                            m_ListOfBots.First(_bot => _bot.m_Name == newBot.m_Name).m_Status = "online";

                            bot.Start();
                        }
                    }));
                }
            }
        }

        private async void StopClick(object _sender, RoutedEventArgs _e)
        {
            foreach (object botName in BotList.Items)
            {
                BotListItem botToStop = (BotListItem)botName;

                if (botToStop.m_Selected)
                {
                    await StopBotAndDisposeTask(botToStop.m_Name).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// If the window gets closed we want to stop every bot and dispose every task
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_e"></param>
        private async void Window_Closing(object _sender, System.ComponentModel.CancelEventArgs _e)
        {
            foreach (object botName in BotList.Items)
            {
                BotListItem botToStop = (BotListItem)botName;

                await StopBotAndDisposeTask(botToStop.m_Name).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Stop the named bot and dispose the named task
        /// </summary>
        /// <param name="_botName"></param>
        private async Task StopBotAndDisposeTask(string _botName)
        {
            if (m_runningBotDictionary.ContainsKey(_botName))
            {
                m_runningBotDictionary[_botName].Stop();
                m_runningBotDictionary.Remove(_botName);

                await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }

            if (m_runningTasks.ContainsKey(_botName))
            {
                m_runningTasks[_botName].Dispose();
                m_runningTasks.Remove(_botName);
            }
        }

        private void BotsOutput_TextChanged(object _sender, System.Windows.Controls.TextChangedEventArgs _e)
        {
            BotsOutput.ScrollToEnd();
        }
    }
}