using System;
using System.IO;
using System.Reflection;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;

namespace Enable_Now_Konnektor.src.config
{
    public class ConfigReader
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static string FilePath = Path.Combine(Util.GetApplicationRoot(), "config.json");
        private static Config _config;

        private ConfigReader() { }

        public static Config LoadConnectorConfig()
        {
            if(_config == null )
            {
                string jsonString = ReadFile();
                _config = JsonConvert.DeserializeObject<Config>(jsonString);
                Validator configValidator = new Validator();
                try
                {
                    configValidator.ValidateConfig(_config);
                }
                catch (Exception e)
                {
                    throw new Exception("Die Konfiguration ist fehlerhaft", e);
                }
            }
            
            
            return _config;
        }

        private static string ReadFile()
        {
            try
            {
                return File.ReadAllText(FilePath);
            }
            catch (Exception e)
            {
                throw new Exception("Fehler beim Lesen der Konfigurationsdatei", e);
            }
        }
    }
}
