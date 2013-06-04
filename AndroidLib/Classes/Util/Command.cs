﻿/*
 * Command.cs - Developed by Dan Wager for AndroidLib.dll - 04/12/12
 */

using System.Diagnostics;
using System.IO;

namespace RegawMOD
{
    internal static class Command
    {
        internal static void RunProcessNoReturn(string executable, string arguments, bool waitForExit = true)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                if (waitForExit)
                    p.WaitForExit();
            }
        }

        internal static string RunProcessReturnOutput(string executable, string arguments)
        {
            string output, error, regular;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                
                p.Start();

                regular = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();

		/* It's more accurate to check for length of a string when we are expecting an empty string after calling Trim().
		 * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
                 */
                if (error.Trim().Length.Equals(0))
                    output = regular;
                else
                    output = error;
            }

            return output;
        }

        internal static string RunProcessReturnOutput(string executable, string arguments, bool forceRegular)
        {
            string output, error, regular;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.Start();

                regular = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();

		/* It's more accurate to check for length of a string when we are expecting an empty string after calling Trim().
		 * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
                 */
                if (error.Trim().Length.Equals(0) || forceRegular)
                    output = regular;
                else
                    output = error;
            }

            return output;
        }

        internal static int RunProcessReturnExitCode(string executable, string arguments)
        {
            int exitCode;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();
                p.WaitForExit();
                exitCode = p.ExitCode;
            }

            return exitCode;
        }

        internal static bool RunProcessOutputContains(string executable, string arguments, string containsString, bool ignoreCase = false)
        {
            string output, error, regular;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();

                regular = p.StandardOutput.ReadToEnd();
                error = p.StandardError.ReadToEnd();

		/* It's more accurate to check for length of a string when we are expecting an empty string after calling Trim().
		 * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
                 */
                if (error.Trim().Length.Equals(0))
                    output = regular;
                else
                    output = error;
            }

            if (ignoreCase)
                return output.ContainsIgnoreCase(containsString);

            return output.Contains(containsString);
        }

        internal static void RunProcessWriteInput(string executable, string arguments, params string[] input)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                using (StreamWriter w = p.StandardInput)
                    for (int i = 0; i < input.Length; i++)
                        w.WriteLine(input[i]);

                p.WaitForExit();
            }
        }

        internal static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                    return true;

            return false;
        }

        internal static void KillProcess(string processName)
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                if (p.ProcessName.ToLower().Contains(processName.ToLower()))
                {
                    p.Kill();
                    return;
                }
            }
        }
    }
}
