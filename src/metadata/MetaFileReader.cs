using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    internal class MetaFileReader : MetaReader
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        internal MetaFileReader(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        internal override async Task<JObject> GetMetaData(Element element, string fileType)
        {
            string entityPath = GetMetaUrl(element.Class, element.Id, fileType);
            if (!File.Exists(entityPath))
            {
                log.Warn(LocalizationService.GetFormattedResource("MetaFileReaderMessage01", entityPath));
                return null;
            }

            string jsonString = await File.ReadAllTextAsync(entityPath);
            if (string.IsNullOrWhiteSpace(jsonString)) { return null; }

            try
            {
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                log.Warn(LocalizationService.GetFormattedResource("MetaFileReaderMessage02"));
                return null;
            }
        }

        internal override string GetMetaUrl(string className, string id, string fileType)
        {
            return jobConfig.EntityPath.Replace("${Class}", classNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        internal override string GetContentUrl(string className, string id)
        {
            return jobConfig.ContentPath.Replace("${Class}", classNames[className]).Replace("${Id}", id);
        }
    }
}
