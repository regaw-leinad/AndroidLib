using System;
using System.Collections.Generic;
using System.IO;

namespace RegawMOD.Android
{
    /// <summary>
    /// Wrapper for the AAPT Android binary
    /// </summary>
    public partial class AAPT
    {
        internal static string resPath;

        /// <summary>
        /// Initializes a new instance of the <c>AAPT</c> class
        /// </summary>
        public AAPT()
        {
            if (resPath == null)
            {
                resPath = Path.Combine(Utils.binDir, @"AAPT\");
            }
        }

        /// <summary>
        /// Dumps the specified Apk's badging information
        /// </summary>
        /// <param name="source">Source Apk on local machine</param>
        /// <returns><see cref="AAPT.Badging"/> object containing badging information</returns>
        public Badging DumpBadging(FileInfo source)
        {
            if (!source.Exists)
                throw new FileNotFoundException();

            return new Badging(source, Command.RunProcessReturnOutput(Path.Combine(resPath, "aapt.exe"), "dump badging \"" + source.FullName + "\"", true, Command.DEFAULT_TIMEOUT));
        }
    }
}
