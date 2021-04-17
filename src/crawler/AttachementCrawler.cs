using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.indexing;
using Enable_Now_Konnektor.src.metadata;
using Enable_Now_Konnektor_Bibliothek.src.service;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using log4net;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Enable_Now_Konnektor.src.indexing.ConverterService;
using Enable_Now_Konnektor.src.service;

namespace Enable_Now_Konnektor.src.crawler
{
    internal class AttachementCrawler
    {
        private readonly JobConfig jobConfig;
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal AttachementCrawler()
        {
            jobConfig = JobManager.GetJobManager().SelectedJobConfig;
        }

        internal async Task<List<Element>> CrawlAttachementsAsync(Element element)
        {
            ConverterService converter = new ConverterService();
            var attachements = new List<Element>();
            StatisticService statisticService = StatisticService.GetService(jobConfig.Id);
            
            foreach (var attachementName in element.AttachementNames)
            {
                ConverterResult res;
                try
                {
                    res = await converter.ConvertAttachementAsync(element, attachementName);
                }
                catch
                {
                    _log.Error( LocalizationService.GetFormattedResource("AttachementCrawlerMessage01", attachementName, element.Id) );
                    ErrorControlService.GetService().IncreaseErrorCount();
                    continue;
                }
                Element attachement = element.Clone() as Element;
                OverwriteAttachementValues(attachement, res, attachementName);
                attachements.Add(attachement);
                statisticService.IncreaseFoundDocumentsCount();
            }
            return attachements;
        }

        private void OverwriteAttachementValues(Element element, ConverterResult res, string fileName)
        {
            Config cfg = ConfigManager.GetConfigManager().ConnectorConfig;

            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.BodyFieldName}", res.Body);

            element.ReplaceValues($"{cfg.FacetIdentifier}.{cfg.ApplicationFieldName}", res.Application);
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.ApplicationFieldName}", res.Application);

            element.ReplaceValues($"{cfg.FacetIdentifier}.{cfg.MimeTypeFieldName}", res.MimeType);
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.MimeTypeFieldName}", res.MimeType);

            string attachementUrl = MetaReader.GetMetaReader().GetMetaUrl(element.Class, element.Id, fileName);
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.UrlFieldName}", attachementUrl );

            string newId = $"{element.Id}_{fileName}";
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.UidFieldName}", newId);
            element.Id = newId;
        }
    }
}
