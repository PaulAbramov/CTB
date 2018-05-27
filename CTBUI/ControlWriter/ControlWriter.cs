/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using System.IO;
using System.Text;
using System.Windows.Controls;

namespace CTBUI.ControlWriter
{
    /// <inheritdoc />
    /// <summary>
    /// With this class we are able to redirect the Console.Writeline to our textbox
    /// </summary>
    public class ControlWriter : TextWriter
    {
        private readonly TextBox m_textBox;

        public ControlWriter(TextBox _textBox)
        {
            m_textBox = _textBox;
        }

        public override void WriteLine(char _value)
        {
            m_textBox.Text += _value;
        }

        public override void WriteLine(string _value)
        {
            m_textBox.Dispatcher.Invoke(() => { m_textBox.Text += "\n" + _value; });
        }

        /// <inheritdoc />
        /// <summary>
        /// This function has to be overriden because of the inheritance
        /// </summary>
        public override Encoding Encoding => Encoding.ASCII;
    }
}
