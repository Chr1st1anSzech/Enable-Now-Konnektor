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
        private static readonly ILog s_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig _jobConfig;

        internal MetaFileReader()
        {
            _jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }

        internal override async Task<JObject> GetMetaDataAsync(Element element, string fileType)
        {
            string entityPath = GetMetaUrl(element.Class, element.Id, fileType);
            if (!File.Exists(entityPath))
            {
                s_log.Warn(LocalizationService.FormatResourceString("MetaFileReaderMessage01", entityPath));
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
                s_log.Warn(LocalizationService.FormatResourceString("MetaFileReaderMessage02"));
                return null;
            }
        }

        internal override string GetMetaUrl(string className, string id, string fileType)
        {
            return _jobConfig.EntityPath.Replace("${Class}", ClassNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        internal override string GetContentUrl(string className, string id)
        {
            return _jobConfig.ContentPath.Replace("${Class}", ClassNames[className]).Replace("${Id}", id);
        }
    }
}
