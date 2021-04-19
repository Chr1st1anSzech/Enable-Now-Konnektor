using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.http;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    internal class MetaWebsiteReader : MetaReader
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        internal MetaWebsiteReader()
        {
            jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }


        internal override async Task<JObject> GetMetaDataAsync(Element element, string fileType)
        {
            string entityUrl = GetMetaUrl(element.Class, element.Id, fileType);
            try
            {
                string jsonString = await new HttpRequest(jobConfig).SendRequestAsync(entityUrl);
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                log.Warn(LocalizationService.FormatResourceString("MetaWebsiteReaderMessage01"));
                return null;
            }
        }

        internal override string GetContentUrl(string className, string id)
        {
            return jobConfig.ContentUrl.Replace("${Class}", ClassNames[className]).Replace("${Id}", id);
        }

        internal override string GetMetaUrl(string className, string id, string fileType)
        {
            return jobConfig.EntityUrl.Replace("${Class}", ClassNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }
    }
}
