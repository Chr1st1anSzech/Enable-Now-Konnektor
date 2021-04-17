using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.http;
using Enable_Now_Konnektor.src.metadata;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Enable_Now_Konnektor.src.indexing
{
    internal class ConverterService
    {
        internal struct ConverterResult
        {
            internal string Application { get; set; }
            internal string Body { get; set; }
            internal string MimeType { get; set; }
        }

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        internal ConverterService()
        {
            jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fileName"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        internal async Task<ConverterResult> ConvertAttachementAsync(Element element, string fileName)
        {
            string url = GetConverterRequestUrl(element, fileName);
            string result;
            try
            {
                result = await new HttpRequest(jobConfig).SendRequestAsync(url);
            }
            catch
            {
                _log.Error(LocalizationService.GetFormattedResource("ConverterServiceMessage02"));
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
            JArray json;
            try
            {
                json = JsonConvert.DeserializeObject<JArray>(jsonString);
            }
            catch (Exception e)
            {
                _log.Error(LocalizationService.GetFormattedResource("ConverterServiceMessage01"), e);
                throw;
            }
            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            var fields = json[0][config.ConverterFieldsIdentifier];
            if (fields == null)
            {
                var message = LocalizationService.GetFormattedResource("ConverterServiceMessage03", config.ConverterFieldsIdentifier);
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
                string message = LocalizationService.GetFormattedResource("ConverterServiceMessage03", fieldName);
                _log.Error(message);
                throw new ArgumentNullException(message);
            }
            return field.Value<string>();
        }

        private string GetConverterRequestUrl(Element element, string fileName)
        {
            string attachementUrl = MetaReader.GetMetaReader().GetMetaUrl(element.Class, element.Id, fileName);
            string contentUrl = HttpUtility.UrlEncode(attachementUrl);
            Config config = ConfigManager.GetConfigManager().ConnectorConfig;
            return config.ConverterUrl + contentUrl;
        }

    }
}
