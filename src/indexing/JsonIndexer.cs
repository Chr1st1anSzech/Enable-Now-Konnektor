using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.indexing
{
    internal class JsonIndexer : Indexer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private struct IndexingElement
        {
            internal string id;
            internal Dictionary<string, List<string>> fields;
        }

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
                await new HttpRequest().SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(Util.GetFormattedResource("JsonIndexerMessage01"), e);
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
            string url = $"{config.RemoveUrl}[{id}]";
            try
            {
                await new HttpRequest().SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                log.Error(Util.GetFormattedResource("JsonIndexerMessage02"), e);
                return false;
            }
        }

        private string GetIndexingParameterString(Element element)
        {
            IndexingElement indexingElement = new IndexingElement()
            {
                id = element.Id,
                fields = element.Fields
            };
            var jsonString = JsonConvert.SerializeObject(indexingElement);
            jsonString = "[" + jsonString + "]";
            return jsonString;
        }
    }
}
