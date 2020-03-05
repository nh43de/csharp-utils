using System.Diagnostics;
using System.IO;

namespace Csharp.Utilities.ConsoleTools
{
    //Experimental
    public static class Piping
    {
        public static StreamWriter ExecPro() //(string ProcessName, string args, string WrkDir, string cmdtxt, out string coutext)
        {
            Process cmd = new Process();

            cmd.StartInfo.FileName = "cmd";
            //cmd.StartInfo.Arguments = args;
            cmd.StartInfo.UseShellExecute = false;
            //cmd.StartInfo.WorkingDirectory = WrkDir;
            cmd.StartInfo.CreateNoWindow = false;
            cmd.StartInfo.ErrorDialog = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardError = false;

            cmd.Start();
            StreamWriter cin = cmd.StandardInput;
            //StreamReader cout = cmd.StandardOutput;

            cin.WriteLine("TEST");

            return cin;
            //cin.Close();
            //coutext = cout.ReadToEnd();
            //cmd.WaitForExit();
            //cmd.Close();
        }
    }
}
