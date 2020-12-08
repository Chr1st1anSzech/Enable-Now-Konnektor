using Enable_Now_Konnektor.locals;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace Enable_Now_Konnektor.src.misc
{
    public class Util
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Util));
        private static ResourceManager res;

        public static string GetApplicationRoot()
        {

            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            return exePath[6..];
        }

        public static string GetFormattedResource(string key, params object[] parameters)
        {
            if (res == null)
            {
                InitializeResourceManager();
            }
            var txt = res.GetString(key) ?? "";
            if( parameters != null)
            {
                txt = string.Format(txt, parameters);
            }
            return txt;
        }

        private static void InitializeResourceManager()
        {
            res = de_DE.ResourceManager;
        }

        public static string ConvertToUnixTime(DateTime date)
        {
            TimeSpan timeSpan = date - new DateTime(1970, 1, 1);
            return timeSpan.TotalSeconds.ToString().Substring(0,10) + "000";
        }

        public static string ConvertToUnixTime(string dateString)
        {
            DateTime parsedDate = DateTime.Now;
            try
            {
                parsedDate = DateTime.Parse(dateString);
            }
            catch
            {
                _log.Info(GetFormattedResource("UtilMessage01", dateString));
            }

            return ConvertToUnixTime(parsedDate);
        }

        public static bool IsDirectoryWritable(string dirPath)
        {
            try
            {
                using FileStream fs = File.Create(
                Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Entfernt das HTML Markup. Falls der Parameter null oder leer ist, wird eine leere Zeichenkette zurückgegeben.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveMarkup(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            string result = Regex.Replace(text, @"<\/?[^>]*?>", "");
            result = Regex.Replace(result, @"(\\r\\n|\\r|\\n|\\t|\s{2,}|&nbsp;)", " ");
            var matches = Regex.Matches(result, "\"(\\S[^\"]*\\S|\\S?)\"");
            foreach (Match match in matches)
            {
                string v = match.Value[1..^1];
                v = ((char)0x201e) + v + ((char)0x201f);
                result = result.Replace(match.Value, v);
            }

            return result;
        }

        public static string JoinArray(params string[] array)
        {
            return string.Join(" ", array.Where((v) => v != null));
        }

        public static string JoinArray(IEnumerable<string> array)
        {
            return string.Join(" ", array.Where((v) => v != null));
        }
    }
}
