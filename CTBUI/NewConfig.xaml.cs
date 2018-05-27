using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CTB.JsonClasses;
using CTBUI.Properties;
using Newtonsoft.Json;

namespace CTBUI
{
    /// <summary>
    /// Interaction logic for NewConfig.xaml
    /// </summary>
    public partial class NewConfig
    {
        private readonly Brush m_defaultColor;

        public NewConfig()
        {
            InitializeComponent();

            m_defaultColor = UsernameTextBox.BorderBrush;
        }

        /// <summary>
        /// Set the borderbrushes to the normal color
        /// 
        /// If botname or botpassword wasn't set, mark them red so the user knows why it doesn't work
        /// 
        /// Set default values if there wasn't made a selection
        /// Trim the admins string and put the multiple admins into an array
        /// 
        /// parse the data into the botInfo object and serialize it to a file
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_e"></param>
        private void SaveButton_Click(object _sender, RoutedEventArgs _e)
        {
            UsernameTextBox.BorderBrush = m_defaultColor;
            PasswordTextBox.BorderBrush = m_defaultColor;

            if(string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                UsernameTextBox.BorderBrush = Brushes.Red;
            }
            else if(string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                PasswordTextBox.BorderBrush = Brushes.Red;
            }
            else
            {
                int acceptFriendRequests = SetIntValue(AcceptFriendRequestsDropDown, EDefaultBoolValue.NO);
                int acceptDonations = SetIntValue(AcceptDonationsDropDown, EDefaultBoolValue.YES);
                int acceptEscrow = SetIntValue(AcceptEscrowDropDown, EDefaultBoolValue.NO);
                int accept1On1 = SetIntValue(Accept1on1DropDown, EDefaultBoolValue.YES);
                int accept1On2 = SetIntValue(Accept1on2DropDown, EDefaultBoolValue.NO);

                List<string> admins = new List<string>();

                foreach(string admin in AdminsTextBox.Text.Split(','))
                {
                    admins.Add(admin.Trim());
                }

                BotInfo botInfo = new BotInfo
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordTextBox.Text,
                    BotName = BotNameTextBox.Text,
                    AcceptFriendRequests = ConvertIntToBool(acceptFriendRequests),
                    AcceptDonations = ConvertIntToBool(acceptDonations),
                    Accept1on1Trades = ConvertIntToBool(accept1On1),
                    Accept1on2Trades = ConvertIntToBool(accept1On2),
                    AcceptEscrow = ConvertIntToBool(acceptEscrow),
                    Admins = admins.ToArray(),
                    GroupToInviteTo = GroupToInvetToTextBox.Text
                };

                if(!File.Exists($"{Settings.Default.ConfigsPath}{botInfo.Username}.json"))
                {
                    File.WriteAllText($"{Settings.Default.ConfigsPath}{botInfo.Username}.json", JsonConvert.SerializeObject(botInfo, Formatting.Indented));
                    Close();
                }
                else
                {
                    MessageBox.Show(this, "There is already a config for this bot.");
                }
            }
        }

        private void CloseButton_Click(object _sender, RoutedEventArgs _e)
        {
            Close();
        }

        /// <summary>
        /// We want to convert 0 to true and 1 to false
        /// </summary>
        /// <param name="_intValue"></param>
        /// <returns></returns>
        private static bool ConvertIntToBool(int _intValue)
        {
            if(_intValue == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the dropdown has a value
        /// If it has a value then just return it, else return a default value which is given by the parameter
        /// An enum is used because of the readability
        /// </summary>
        /// <param name="_comboBox"></param>
        /// <param name="_defaultBoolValue"></param>
        /// <returns></returns>
        private static int SetIntValue(ComboBox _comboBox, EDefaultBoolValue _defaultBoolValue)
        {
            if (_comboBox.SelectedIndex == -1)
            {
                return (int) _defaultBoolValue;
            }
            else
            {
                return _comboBox.SelectedIndex;
            }
        }
    }

    internal enum EDefaultBoolValue
    {
        YES = 0,
        NO = 1
    }
}
