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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CTBUI.ControlReader
{
	public class ControlReader : TextReader
	{
		public override string ReadLine()
		{
			AllocConsole();

			//string test = Console.ReadLine();
			string test2 = base.ReadLine();

			return test2;

			//bool ready = false;
			//
			//Task<string> test = Task.Run(() =>
			//{
			//	InputWindow input = new InputWindow();
			//
			//	input.Show();
			//
			//	input.OkayButton.Click += (_sender, _args) => { ready = true; };
			//
			//	return input.InputText.Text;
			//});
			//
			//test.Wait();
		}
	}

	// TODO check writeline for string and open a window, let the user enter it and click the button, then on readline read the text and close the window
}