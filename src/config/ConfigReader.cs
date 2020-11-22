using System;
using System.IO;
using System.Reflection;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;

namespace Enable_Now_Konnektor.src.config
{
    internal class ConfigReader
    {
        private static string filePath = Path.Combine(Util.GetApplicationRoot(), "config.json");
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Config config;

        private ConfigReader() { }

        internal static void Initialize()
        {
            if (config == null)
            {
                string jsonString = ReadFile();
                try
                {
                    config = JsonConvert.DeserializeObject<Config>(jsonString);
                    Validator configValidator = new Validator();
                    configValidator.ValidateConfig(config);
                }
                catch (Exception e)
                {
                    throw new Exception("Die Konfiguration ist fehlerhaft", e);
                }
            }
        }

        internal static Config LoadConnectorConfig()
        {
            return config;
        }

        private static string ReadFile()
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                throw new Exception("Fehler beim Lesen der Konfigurationsdatei", e);
            }
        }
    }
}
