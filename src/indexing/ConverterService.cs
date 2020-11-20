using Enable_Now_Konnektor.src.access;
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
        private readonly JobConfig jobConfig;

        public ConverterService(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fileName"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public async Task<ConverterResult> ConvertAttachementAsync(Element element, string fileName)
        {
            string url = GetConverterRequestUrl(element, fileName);
            string result;
            try
            {
                result = await new HttpRequest().SendRequestAsync(url);
            }
            catch
            {
                _log.Error(Util.GetFormattedResource("ConverterServiceMessage02"));
                throw;
            }

            return ExtractValues(result);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        private ConverterResult ExtractValues(string jsonString)
        {
            JObject json;
            try
            {
                json = JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch (Exception e)
            {
                _log.Error(Util.GetFormattedResource("ConverterServiceMessage01"), e);
                throw;
            }
            Config config = ConfigReader.LoadConnectorConfig();
            var fields = json[config.ConverterFieldsIdentifier];
            if (fields == null)
            {
                var message = Util.GetFormattedResource("ConverterServiceMessage03", config.ConverterFieldsIdentifier);
                _log.Error(message);
                throw new ArgumentNullException(message);
            }

            ConverterResult res = new ConverterResult();

            string fieldName = $"{config.StringIdentifier}.{config.BodyFieldName}";
            res.Body = Util.RemoveMarkup(GetConverterFieldValue(fields, fieldName));

            fieldName = $"{config.StringIdentifier}.{config.MimeTypeFieldName}";
            res.MimeType = GetConverterFieldValue(fields, fieldName);

            fieldName = $"{config.StringIdentifier}.{config.ApplicationFieldName}";
            string app = GetConverterFieldValue(fields, fieldName);

            res.Application = config.ConverterApplicationMapping.ContainsKey(app) ?
                config.ConverterApplicationMapping[app] : config.ConverterApplicationDefaultMapping;

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        private string GetConverterFieldValue(JToken fields, string fieldName)
        {
            var field = fields[fieldName]?[0];
            if ( field == null)
            {
                string message = Util.GetFormattedResource("ConverterServiceMessage03", fieldName);
                _log.Error(message);
                throw new ArgumentNullException(message);
            }
            return field.Value<string>();
        }

        private string GetConverterRequestUrl(Element element, string fileName)
        {
            string attachementUrl = new HttpMetaAccess(jobConfig).GetMetaUrl(element.Class, element.Id, fileName);
            string contentUrl = HttpUtility.UrlEncode(attachementUrl);
            Config config = ConfigReader.LoadConnectorConfig();
            return config.ConverterUrl + contentUrl;
        }

    }
}
