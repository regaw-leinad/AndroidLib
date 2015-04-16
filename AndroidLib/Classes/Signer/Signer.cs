/*
 * Signer.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.Collections.Generic;
using System.IO;

namespace RegawMOD.Android
{
    /// <summary>
    /// Digitally signs files
    /// </summary>
    public static class Signer
    {
        /// <summary>
        /// Signs an Update.zip with test keys to flash on an Android device
        /// </summary>
        /// <param name="unsigned">Full path to unsigned update.zip</param>
        /// <returns>True if successful, false if file <paramref name="unsigned"/> does not exist or if file <paramref name="unsigned"/> is not a zip</returns>
        /// <remarks><para>Outputs signed zip in same directory as unsigned zip</para></remarks>
        public static bool SignUpdateZip(string unsigned)
        {
            if (!File.Exists(unsigned) || Path.GetExtension(unsigned).ToLower() != ".zip")
                return false;

            string signed = Path.Combine(
                Path.GetDirectoryName(unsigned),
                Path.GetFileNameWithoutExtension(unsigned) + "_signed.zip"
            );

            bool result;

            string resDir = Path.Combine(Utils.binDir, @"\Signer\");

            result = Java.RunJar(resDir + "signapk.jar",
                string.Format(Properties.Resources.SignerCommandline, resDir, resDir, unsigned, signed)
            );
            
            return result;
        }
    }
}