using System;
using System.Diagnostics;
using System.Windows.Forms;
using Tsukikage.Util;

namespace Tsukikage.XGTGCtrl2
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleWindow.Allocate();

            if (Array.IndexOf(args, "--with-console") < 0) { ConsoleWindow.Hide(); }

            Trace.WriteLine(EntryAssemblyInformation.Title + " Version " + EntryAssemblyInformation.Version);
            ConsoleWindow.SetIcon(Properties.Resources.icon);
            Console.Title = EntryAssemblyInformation.Title + " log";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.MainForm());
        }

    }
}
