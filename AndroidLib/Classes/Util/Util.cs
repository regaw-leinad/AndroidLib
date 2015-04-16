/*
 * Util.cs - Extension methods for RegawMOD namespace
 * Developed by Dan Wager for AndroidLib.dll
 */

using System.IO;
using System.Reflection;
namespace RegawMOD
{
    internal static class Utils
    {
        private static string _binDir;
        internal static string binDir
        {
            get
            {
                if (_binDir == null)
                    _binDir = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        @"_bin\"
                    );

                return _binDir;
            }
        }
    }


    internal static class ExtensionMethods
    {
        internal static bool ContainsIgnoreCase(this string s, string str)
        {
            return s.ToLower().Contains(str.ToLower());
        }

        internal static string ProperCase(this string s)
        {
            string final = "";

            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0 && char.IsLetter(s[i]))
                {
                    final += char.ToUpper(s[i]);
                    continue;
                }

                if (char.IsWhiteSpace(s[i - 1]) || char.IsControl(s[i - 1]) && char.IsLetter(s[i]))
                    final += char.ToUpper(s[i]);
                else
                    final += char.ToLower(s[i]);
            }

            return final;
        }
    }
}