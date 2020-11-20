using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.access
{
    class HttpMetaAccess : MetaAccess
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        public HttpMetaAccess(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }


        public override async Task<JObject> GetMetaData(Element element, string fileType)
        {
            string entityUrl = GetMetaUrl(element.Class, element.Id, fileType);
            try
            {
                string jsonString = await new HttpRequest().SendRequestAsync(entityUrl);
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                log.Warn(Util.GetFormattedResource("MetaFileReaderMessage01", element.Id, fileType));
                return null;
            }
        }

        public override string GetContentUrl(string className, string id)
        {
            log.Debug(Util.GetFormattedResource("HttpMetaAccessMessage01"));
            return jobConfig.ContentUrl.Replace("${Class}", classNames[className]).Replace("${Id}", id);
        }

        public override string GetMetaUrl(string className, string id, string fileType)
        {
            log.Debug(Util.GetFormattedResource("HttpMetaAccessMessage02"));
            return jobConfig.EntityUrl.Replace("${Class}", classNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        private string GetDemoUrl(string id)
        {
            log.Debug(Util.GetFormattedResource("HttpMetaAccessMessage03"));
            return jobConfig.DemoUrl.Replace("${Id}", id);
        }

    }
}
