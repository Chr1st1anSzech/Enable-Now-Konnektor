using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
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

        internal MetaWebsiteReader(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }


        internal override async Task<JObject> GetMetaData(Element element, string fileType)
        {
            string entityUrl = GetMetaUrl(element.Class, element.Id, fileType);
            try
            {
                string jsonString = await new HttpRequest().SendRequestAsync(entityUrl);
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                log.Warn(Util.GetFormattedResource("MetaWebsiteReaderMessage01"));
                return null;
            }
        }

        internal override string GetContentUrl(string className, string id)
        {
            return jobConfig.ContentUrl.Replace("${Class}", classNames[className]).Replace("${Id}", id);
        }

        internal override string GetMetaUrl(string className, string id, string fileType)
        {
            return jobConfig.EntityUrl.Replace("${Class}", classNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        private string GetDemoUrl(string id)
        {
            return jobConfig.DemoUrl.Replace("${Id}", id);
        }

    }
}
