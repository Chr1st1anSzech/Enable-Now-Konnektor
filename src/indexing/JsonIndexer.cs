using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.http;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Enable_Now_Konnektor.src.indexing
{
    internal class JsonIndexer : Indexer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);



        /// <summary>
        /// 
        /// </summary>
        internal JsonIndexer()
        {
            jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal async override Task<bool> AddElementToIndexAsync(Element element)
        {
            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            string paramString = GetIndexingParameterString(element);
            string url = $"{config.IndexUrl}{paramString}";
            try
            {
                await new HttpRequest(jobConfig).SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(LocalizationService.FormatResourceString("JsonIndexerMessage01"), e);
                return false;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal async override Task<bool> RemoveElementFromIndexAsync(Element element)
        {
            return await RemoveElementFromIndexAsync(element.Id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async override Task<bool> RemoveElementFromIndexAsync(string id)
        {
            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            string encodedParam = HttpUtility.UrlEncode($"[{GetElasticsearchId(id)}]");
            string url = $"{config.RemoveUrl}{encodedParam}";
            try
            {
                await new HttpRequest(jobConfig).SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(LocalizationService.FormatResourceString("JsonIndexerMessage02"), e);
                return false;
            }
        }

        struct IndexElement
        {
            // müssen klein geschrieben werden
            public string id { get;  }
            public Dictionary<string, List<string>> fields { get; }

            public IndexElement(string id, Dictionary<string, List<string>> fields)
            {
                this.id = id;
                this.fields = fields;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetIndexingParameterString(Element element)
        {

            IndexElement indexingElement = new(GetElasticsearchId(element.Id), element.Fields);
            var jsonString = JsonConvert.SerializeObject(indexingElement);
            return HttpUtility.UrlEncode( $"[{jsonString}]" );
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private string GetElasticsearchId(string elementId)
        {
            return $"{jobConfig.Id}-{elementId}";
        }
    }
}
