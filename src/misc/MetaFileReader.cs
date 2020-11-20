using Enable_Now_Konnektor.src.access;
using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private JobConfig jobConfig;

        public MetaFileReader(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        public void ExtractAssets(MetaFiles metaFiles, out string[] childrenIds, out string[] attachementNames)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            log.Debug("Analysiere alle Kindselemente.");
            var assets = metaFiles.EntityFile?[config.AssetsIdentifier];
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

        public string ExtractValue(MetaFiles metaFiles, string variableName)
        {
            if( variableName == null)
            {
                return null;
            }

            Config config = ConfigReader.LoadConnectorConfig();
            if (variableName.StartsWith(config.EntityIdentifier))
            {
                return ExtractValueFromEntityTxt(variableName[config.EntityIdentifier.Length..], metaFiles.EntityFile);
            }
            else if (variableName.StartsWith(config.LessonIdentifier))
            {
                return ExtractValueFromJson(variableName[config.LessonIdentifier.Length..], metaFiles.LessonFile);
            }
            else if (variableName.StartsWith(config.SlideIdentifier))
            {
                return ExtractValueFromJson(variableName[config.SlideIdentifier.Length..], metaFiles.SlideFile);
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
                log.Warn( Util.GetFormattedResource("MetaFileReaderMessage02"));
                return null;
            }
            IEnumerable<string> values = json.Descendants().OfType<JProperty>()
                .Where((e) => e.Name.Equals(variableName))
                .Select(e => e.Value.Value<string>());
            return Util.JoinArray(values);
        }

        public async Task<MetaFiles> LoadMetaFiles(Element element)
        {
            MetaFiles files = new MetaFiles
            {
                LessonFile = await GetJsonFileAsync(element, MetaAccess.LessonFile),
                SlideFile = await GetJsonFileAsync(element, MetaAccess.SlideFile)
            };
            files.EntityFile = await GetJsonFileAsync(element, MetaAccess.EntityFile);
            if(files.EntityFile == null)
            {
                string message = Util.GetFormattedResource("MetaFileReaderMessage04");
                log.Error(message);
                throw new ArgumentNullException(message);
            }
            return files;
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
            if ((element.Class != Element.Project && fileType == MetaAccess.LessonFile) || (element.Class != Element.Slide && fileType == MetaAccess.SlideFile))
            {
                log.Debug($"Für {element.Class} gibt es keine {fileType}.");
                return null;
            }

            var access = new HttpMetaAccess(jobConfig);
            return await access.GetMetaData(element, fileType);
        }
    }
}
