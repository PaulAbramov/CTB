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
using System.Linq;
using System.Windows.Controls;

namespace CTBUI.ControlReader
{
	public class ControlReader : TextReader
	{
	    private readonly TextBox m_textBox;

        public ControlReader(TextBox _textBox)
        {
            m_textBox = _textBox;
        }

        /// <inheritdoc />
        /// <summary>
        /// Open the inputwindow and get the text onclose
        /// </summary>
        /// <returns></returns>
		public override string ReadLine()
        {
            string lastline = m_textBox.Text.Split('\n').LastOrDefault();

            InputWindow input = new InputWindow
            {
                OutputText = {Text = lastline}
            };

            input.ShowDialog();

			return input.InputText.Text;
        }
	}
}