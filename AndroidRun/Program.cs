using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegawMOD.Android;
using System.IO;

namespace AndroidRun
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("简体中文打印测试");

            var android = AndroidController.Instance;
            Console.WriteLine("List of devices attached");

            foreach (var device in android.ConnectedDevices)
            {
                Console.WriteLine("{0}\tdevice", device);
            }

            Console.WriteLine();
            Console.Write("Drag APK here: ");
            
            string apk = Console.ReadLine();
            if (apk.Substring(0, 1) == "\"")
            {
                apk = apk.Substring(1, apk.Length - 2);
            }

            if (!File.Exists(apk))
            {
                Console.WriteLine("File: {0}", apk);
                Console.WriteLine("File not exist.");
            }
            else
            {
                AAPT aapt = new AAPT();
                var info = aapt.DumpBadging(new FileInfo(apk));
                Console.WriteLine("{1}: ({0})", apk, info.Application.Label);
                Console.WriteLine("Package: {0} / Ver: {1}", info.Package.Name, info.Package.VersionName);
            }

            Console.Write("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
