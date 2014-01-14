/*
 * Command.cs - Developed by Dan Wager for AndroidLib.dll - 04/12/12
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace RegawMOD
{
    internal static class Command
    {
        /// <summary>
        /// The defualt timeout for commands. -1 implies infinite time
        /// </summary>
        public const int DEFAULT_TIMEOUT = -1;
        
        [Obsolete("Method is deprecated, please use RunProcessNoReturn(string, string, int) instead.")]
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

        internal static void RunProcessNoReturn(string executable, string arguments, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = true;

                p.Start();

                p.WaitForExit(timeout);
            }
        }

        internal static string RunProcessReturnOutput(string executable, string arguments, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, false);
            }
        }



        internal static string RunProcessReturnOutput(string executable, string arguments, bool forceRegular, int timeout)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        return HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, forceRegular);
            }
        }

        [Obsolete("Method is deprecated, please use RunProcessOutputContains(string, string, string, int, [bool]) instead.")]
        internal static bool RunProcessOutputContains(string executable, string arguments, string containsString, bool ignoreCase = false)
        {
            string output, error, regular;

            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
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


        internal static bool RunProcessOutputContains(string executable, string arguments, string containsString, int timeout, bool ignoreCase = false)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;

                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;

                string output = "";

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    output = HandleOutput(p, outputWaitHandle, errorWaitHandle, timeout, false);

                if (ignoreCase)
                    return output.ContainsIgnoreCase(containsString);

                return output.Contains(containsString);
            }
        }

        private static string HandleOutput(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle, int timeout, bool forceRegular)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            p.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    outputWaitHandle.Set();
                else
                    output.AppendLine(e.Data);
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    errorWaitHandle.Set();
                else
                    error.AppendLine(e.Data);
            };

            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (p.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
            {
                string strReturn = "";

                if (error.ToString().Trim().Length.Equals(0) || forceRegular)
                    strReturn = output.ToString().Trim();
                else
                    strReturn = error.ToString().Trim();

                return strReturn;
            }
            else
            {
                // Timed out.
                return "PROCESS TIMEOUT";
            }
        }

        internal static int RunProcessReturnExitCode(string executable, string arguments, int timeout)
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
                p.WaitForExit(timeout);
                exitCode = p.ExitCode;
            }

            return exitCode;
        }

        // TODO: Change to timeout implementation
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
