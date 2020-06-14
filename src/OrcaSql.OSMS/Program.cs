using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using OrcaSql.OSMS.SQLEditor;

namespace OrcaSql.OSMS
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
            var fsmProvider = new AppSyntaxModeProvider(); // Create new provider with the highlighting directory.
            HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider); // Attach to the text editor.
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Run(new Main(args));
		}

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.AppendAllText("ErrorLog.txt",
                DateTime.Now +
                Environment.NewLine +
                "----------" +
                Environment.NewLine +
                e +
                Environment.NewLine +
                Environment.NewLine);

            string msg =
                "An exception has occurred:" + Environment.NewLine +
                e + Environment.NewLine +
                Environment.NewLine +
                "To help improve OrcaSql, I would appreciate if you would send the ErrorLog.txt file to me at ycherkes@outlook.com" + Environment.NewLine +
                Environment.NewLine +
                "The error log does not contain any sensitive information, feel free to check it to be 100% certain. The ErrorLog.txt file is located in the same directory as the OrcaSql Studio application.";

            MessageBox.Show(msg, "Uh oh!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}