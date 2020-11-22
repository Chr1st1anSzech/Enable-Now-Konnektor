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
    class JsonIndexer : Indexer
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private struct IndexingElement
        {
            public string id;
            public Dictionary<string, List<string>> fields;
        }

        public JsonIndexer(JobConfig jobConfig)
        {
            _config = ConfigReader.LoadConnectorConfig();
            _jobConfig = jobConfig;
        }

        public async override Task<bool> AddElementToIndex(Element element)
        {
            string paramString = GetParameterString(element);
            string url = _config.IndexUrl + paramString;
            try
            {
                await new HttpRequest().SendRequestAsync(url);
                return true;
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("JsonIndexerMessage01"), e);
                return false;
            }
        }

        public override bool RemoveElementFromIndex(Element element)
        {
            return true;
        }

        public override bool RemoveElementFromIndex(string id)
        {
            return true;
        }

        private string GetParameterString(Element element)
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
