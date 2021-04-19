using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    /// <summary>
    /// Eine Klasse, um die Metadateien von Enabe Now auszulesen. Relevant sind die entity.txt, slide.js und lesson.js.
    /// </summary>
    internal class MetaAnalyzer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        internal MetaAnalyzer()
        {
            jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }

        internal void ExtractAssets(MetaDataCollection metaData, out string[] childrenIds, out string[] attachementNames)
        {
            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            log.Debug("Analysiere alle Kindselemente.");
            var assets = metaData.Entity?[config.AssetsIdentifier];
            if (assets == null)
            {
                childrenIds = new string[0];
                attachementNames = new string[0];
                return;
            }

            childrenIds = (from JToken asset in assets
                               where asset[config.AutostartIdentifier]?.Value<bool>() != true
                               && asset[config.UidFieldName]?.Value<string>() != null
                               select asset[config.UidFieldName].Value<string>()).ToArray();

            attachementNames = (from JToken asset in assets
                                    where asset[config.TypeIdentifier]?.Value<string>() == config.DocuIdentifier
                                    && asset[config.FileNameIdentifier]?.Value<string>() != null
                                    select asset[config.FileNameIdentifier].Value<string>()).ToArray();
        }

        internal string ExtractValue(MetaDataCollection metaData, string variableName)
        {
            if( variableName == null)
            {
                return null;
            }

            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            if (variableName.StartsWith(config.EntityIdentifier))
            {
                return ExtractValueFromEntityTxt(variableName[config.EntityIdentifier.Length..], metaData.Entity);
            }
            else if (variableName.StartsWith(config.LessonIdentifier))
            {
                return ExtractValueFromJson(variableName[config.LessonIdentifier.Length..], metaData.Lesson);
            }
            else if (variableName.StartsWith(config.SlideIdentifier))
            {
                return ExtractValueFromJson(variableName[config.SlideIdentifier.Length..], metaData.Slide);
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
                log.Warn( LocalizationService.FormatResourceString("MetaAnalyzerMessage02"));
                return null;
            }
            IEnumerable<string> values = json.Descendants().OfType<JProperty>()
                .Where((e) => e.Name.Equals(variableName))
                .Select(e => e.Value.Value<string>());
            return Util.JoinArray(values);
        }

        internal async Task<MetaDataCollection> LoadMetaFilesAsync(Element element)
        {
            MetaDataCollection metaData = new()
            {
                Lesson = await GetJsonFileAsync(element, MetaReader.LessonFile),
                Slide = await GetJsonFileAsync(element, MetaReader.SlideFile)
            };
            metaData.Entity = await GetJsonFileAsync(element, MetaReader.EntityFile);
            if(metaData.Entity == null)
            {
                string message = LocalizationService.FormatResourceString("MetaAnalyzerMessage04");
                log.Error(message);
                throw new ArgumentNullException(message);
            }
            return metaData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        private async Task<JObject> GetJsonFileAsync(Element element, string fileType)
        {
            // nur Projekte haben eine lesson.js
            // nur Buchseiten haben eine slide.js
            if ((element.Class != Element.Project && fileType == MetaReader.LessonFile) || (element.Class != Element.Slide && fileType == MetaReader.SlideFile))
            {
                log.Debug($"Für {element.Class} gibt es keine {fileType}.");
                return null;
            }

            var access = MetaReader.GetMetaReader();
            return await access.GetMetaDataAsync(element, fileType);
        }
    }
}
