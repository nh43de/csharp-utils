using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Csharp.Utilities.ConsoleTools
{

    /// <summary>
    /// Helper class for starting command line applications.
    /// </summary>
    public static class Cmd
    {
        public static void StartProcess(string path)
        {
            Process.Start(path);
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="startIn"></param>
        /// <param name="createWindow">Whether to create a new window (experimental - use default)</param>
        public static void ExecuteCommandSync(string command, string startIn = null, bool createWindow = false)
        {
            // create the ProcessStartInfo using "cmd" as the program to be run,
            // and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows,
            // and then exit.
            var procStartInfo =
                new ProcessStartInfo("cmd", "/c " + command);

            var asFile = new FileInfo(Assembly.GetExecutingAssembly().Location);

            procStartInfo.WorkingDirectory = 
                string.IsNullOrWhiteSpace(startIn) 
                ? asFile.DirectoryName 
                : startIn;
            

            // The following commands are needed to redirect he standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = !createWindow;

            procStartInfo.UseShellExecute = createWindow;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = !createWindow;
            // Now we create a process, assign its ProcessStartInfo and start it
            var proc = new Process {StartInfo = procStartInfo};

            proc.Start();

            if(createWindow)
                proc.WaitForExit();
            
            // Get the output into a string
            if (createWindow == false)
            {
                var result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
        }
    }
}