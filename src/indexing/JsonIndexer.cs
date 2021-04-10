using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.http;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Enable_Now_Konnektor.src.indexing
{
    internal class JsonIndexer : Indexer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal JsonIndexer(JobConfig jobConfig)
        {
            base.jobConfig = jobConfig;
        }

        internal async override Task<bool> AddElementToIndexAsync(Element element)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            string paramString = GetIndexingParameterString(element);
            string url = $"{config.IndexUrl}{paramString}";
            try
            {
                await new HttpRequest(jobConfig).SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(LocalizationService.GetFormattedResource("JsonIndexerMessage01"), e);
                return false;
            }
        }

        internal override Task<bool> RemoveElementFromIndexAsync(Element element)
        {
            return RemoveElementFromIndexAsync(element.Id);
        }

        internal async override Task<bool> RemoveElementFromIndexAsync(string id)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            string encodedParam = HttpUtility.UrlEncode($"[{GetElasticsearchId(id)}]");
            string url = $"{config.RemoveUrl}{encodedParam}";
            try
            {
                await new HttpRequest(jobConfig).SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(LocalizationService.GetFormattedResource("JsonIndexerMessage02"), e);
                return false;
            }
        }

        private string GetIndexingParameterString(Element element)
        {
            IndexingElement indexingElement = new IndexingElement()
            {
                id = GetElasticsearchId(element.Id),
                fields = element.Fields
            };
            var jsonString = JsonConvert.SerializeObject(indexingElement);
            return HttpUtility.UrlEncode( "[" + jsonString + "]" );
        }

        private string GetElasticsearchId(string elementId)
        {
            return jobConfig.Id + "-" + elementId;
        }
    }
}
