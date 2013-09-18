using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RegawMOD.Util
{
    class Logger
    {
        public static string ErrorLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.Combine("AndroidLib", "ErrorLog.txt"));

        public static bool WriteLog(string Message, string Title, string StackTrace)
        {
            try
            {
                using (FileStream fs = new FileStream(ErrorLogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(String.Join(" ", new string[] { Title, Message, StackTrace }));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
