using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.misc
{
    /// <summary>
    /// Eine Klasse, um die Metadateien von Enabe Now auszulesen. Relevant sind die entity.txt, slide.js und lesson.js.
    /// </summary>
    class MetaFileReader
    {
        public struct MetaFiles
        {
            public JObject EntityFile { get; set; }
            public JObject SlideFile { get; set; }
            public JObject LessonFile { get; set; }
        }

        private readonly ILog _log = LogManager.GetLogger(typeof(MetaFileReader));
        private readonly UrlFormatter _urlFormatter;
        private readonly Config _config;

        public MetaFileReader(UrlFormatter urlFormatter)
        {
            _config = ConfigReader.LoadConnectorConfig();
            _urlFormatter = urlFormatter;
        }

        public void ExtractAssets(MetaFiles metaFiles, out string[] childrenIds, out string[] attachementNames)
        {
            _log.Debug("Analysiere alle Kindselemente.");
            var assets = metaFiles.EntityFile[_config.AssetsIdentifier];
            if (assets == null)
            {
                childrenIds = null;
                attachementNames = null;
                return;
            }

            childrenIds = (from JToken asset in assets
                               where asset[_config.AutostartIdentifier]?.Value<bool>() != true
                               && asset[_config.UidFieldName]?.Value<string>() != null
                               select asset[_config.UidFieldName].Value<string>()).ToArray();

            attachementNames = (from JToken asset in assets
                                    where asset[_config.TypeIdentifier]?.Value<string>() == _config.DocuIdentifier
                                    && asset[_config.FileNameIdentifier]?.Value<string>() != null
                                    select asset[_config.FileNameIdentifier].Value<string>()).ToArray();
        }

        public string ExtractValue(MetaFiles metaFiles, char firstChar, string variableName)
        {
            if (firstChar.Equals(_config.EntityIdentifier))
            {
                return ExtractValueFromEntityTxt(variableName, metaFiles.EntityFile);
            }
            else if (firstChar.Equals(_config.LessonIdentifier))
            {
                return ExtractValueFromJson(variableName, metaFiles.LessonFile);
            }
            else if (firstChar.Equals(_config.SlideIdentifier))
            {
                return ExtractValueFromJson(variableName, metaFiles.SlideFile);
            }
            return "";
        }

        private string ExtractValueFromEntityTxt(string variableName, JObject json)
        {
            return json?[variableName]?.Value<string>();
        }

        private string ExtractValueFromJson(string variableName, JObject json)
        {
            if (json == null)
            {
                _log.Info($"Die Variable {variableName} ist nicht in der Metadatei vorhanden");
                return null;
            }
            IEnumerable<string> values = json.Descendants().OfType<JProperty>()
                .Where((e) => e.Name.Equals(variableName))
                .Select(e => e.Value.Value<String>());
            return Util.JoinArray(values);
        }

        public async Task<MetaFiles> LoadMetaFiles(Element element)
        {
            MetaFiles files = new MetaFiles
            {
                EntityFile = await GetJsonFileAsync(element, UrlFormatter.EntityFile),
                LessonFile = await GetJsonFileAsync(element, UrlFormatter.LessonFile),
                SlideFile = await GetJsonFileAsync(element, UrlFormatter.SlideFile)
            };
            return files;
        }

        private async Task<JObject> GetJsonFileAsync(Element element, string fileType)
        {
            // nur Projekte haben eine lesson.js
            // nur Buchseiten haben eine slide.js
            if ((element.Class != Element.Project && fileType == UrlFormatter.LessonFile) || (element.Class != Element.Slide && fileType == UrlFormatter.SlideFile))
            {
                _log.Debug($"Keine Datei gefunden. Für {element.Class} gibt es keine {fileType}.");
                return null;
            }
            string entityUrl = _urlFormatter.GetEntityUrl(element.Class, element.Id, fileType);
            try
            {
                string jsonString = await HttpRequest.SendRequestAsync(entityUrl);
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                _log.Error( Util.GetFormattedResource("MetaFileReaderMessage01", entityUrl) );
                throw;
            }
        }
    }
}
