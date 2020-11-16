using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.jobs;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Enable_Now_Konnektor.src.misc
{
    public class Validator
    {
        public static string EmailPattern = "[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        public static string ServerNamePattern = @"[A-Za-z0-9-]*[A-Za-z0-9]+(\.[A-Za-z0-9-]*[A-Za-z0-9]+)+";
        public static string EnableNowIdPattern = @"^(PR|GR|SL|M)_[0-9A-Za-z]+$";
        public static string UrlPattern = @"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?";

        private static ILog _log = LogManager.GetLogger(typeof(Validator));

        /// <summary>
        /// Prüft, ob ein Wert mit einem regulären Ausdruck übereinstimmt.
        /// </summary>
        /// <param name="value">Der Wert, der geprüft werden soll.</param>
        /// <param name="pattern">Der reguläre Ausdruck.</param>
        /// <returns>Falls der Wert dem regulären Ausdruck entspricht wird wahr zurückgegeben. Falls der Wert oder das Muster null ist oder
        /// der Wert nicht übereinstimmt, wird falsch zurückgegeben.</returns>
        public static bool Validate( string value, string pattern)
        {
            try
            {
                return Regex.Match(value, pattern).Success;
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("ValidatorMessage01", pattern), e);
                return false;
            }
        }


        public bool ValidateConfig(Config config)
        {
            string[] urls = new string[] { config.ConverterUrl, config.FetchUrl, config.IndexUrl, config.ConverterUrl };
            foreach (string url in urls)
            {
                if (!Validate(url, UrlPattern))
                {
                    _log.Error($"Der Wert {url} entspricht keinem URL-Muster.");
                    return false;
                }
            }
            return true;
        }

        public bool ValidateJobConfig(JobConfig jobConfig)
        {
            string[] urls = new string[] { jobConfig.ContentUrl, jobConfig.DemoUrl, jobConfig.EntityUrl };
            foreach (string url in urls)
            {
                if (!Validate(url, UrlPattern))
                {
                    _log.Error($"Der Wert {url} entspricht keinem URL-Muster.");
                    return false;
                }
            }

            string[] emails = new string[] { jobConfig.EmailSender, jobConfig.EmailRecipient };
            foreach (string email in emails)
            {
                if (!Validate(email, EmailPattern))
                {
                    _log.Error($"Der Wert {email} entspricht keinem E-Mail-Muster.");
                    return false;
                }
            }

            return true;
        }
    }
}
