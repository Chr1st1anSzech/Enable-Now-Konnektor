using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Enable_Now_Konnektor.src.config
{
    public class ConfigWriter
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void SaveConfig(Config config)
        {
            string jsonString = JsonConvert.SerializeObject(config);
            WriteFile(jsonString);
        }

        private void WriteFile(string jsonString)
        {
            try
            {
                File.WriteAllText(ConfigReader.FilePath, jsonString);
            }
            catch (Exception e)
            {
                throw new Exception("Fehler beim Schreiben der Konfigurationsdatei", e);
            }
        }
    }
}
