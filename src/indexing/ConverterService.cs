using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Enable_Now_Konnektor.src.indexing
{
    class ConverterService
    {
        public struct ConverterResult
        {
            public string Application { get; set; }
            public string Body { get; set; }
            public string MimeType { get; set; }
        }

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly UrlFormatter _urlFormatter;

        public ConverterService(UrlFormatter urlFormatter)
        {
            _urlFormatter = urlFormatter;
        }

        public async Task<ConverterResult> ConvertAttachementAsync(Element element, string fileName)
        {
            string url = GetConverterRequestUrl(element, fileName);
            string result = await HttpRequest.SendRequestAsync(url);
            return ExtractValues(result);

        }

        private ConverterResult ExtractValues(string jsonString)
        {
            ConverterResult res = new ConverterResult();
            JObject json = JsonConvert.DeserializeObject<JObject>(jsonString);
            try
            {
                Config config = ConfigReader.LoadConnectorConfig();
                var fields = json[config.ConverterFieldsIdentifier];
                res.Body = Util.RemoveMarkup(fields[$"{config.StringIdentifier}.{config.BodyFieldName}"]?[0].Value<string>());

                res.MimeType = fields[$"{config.StringIdentifier}.{config.MimeTypeFieldName}"]?[0].Value<string>();

                string app = fields[$"{config.StringIdentifier}.{config.ApplicationFieldName}"]?[0].Value<string>();
                res.Application = config.ConverterApplicationMapping.ContainsKey(app) ?
                    config.ConverterApplicationMapping[app] : config.ConverterApplicationDefaultMapping;
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("ConverterServiceMessage01"), e);
            }
            return res;
        }

        private string GetConverterRequestUrl(Element element, string fileName)
        {
            string contentUrl = HttpUtility.UrlEncode(_urlFormatter.GetEntityUrl(element.Class, element.Id, fileName));
            Config config = ConfigReader.LoadConnectorConfig();
            return config.ConverterUrl + contentUrl;
        }

    }
}
