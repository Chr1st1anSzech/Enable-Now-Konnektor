﻿using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.indexing;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using Enable_Now_Konnektor.src.service;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Enable_Now_Konnektor.src.indexing.ConverterService;

namespace Enable_Now_Konnektor.src.crawler
{
    internal class AttachementCrawler
    {
        private readonly JobConfig jobConfig;
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal AttachementCrawler(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        internal async Task<List<Element>> CrawlAttachementsAsync(Element element)
        {
            ConverterService converter = new ConverterService(jobConfig);
            var attachements = new List<Element>();
            Statistics statisticService = Statistics.GetService(jobConfig.Id);
            
            foreach (var attachementName in element.AttachementNames)
            {
                ConverterResult res;
                try
                {
                    res = await converter.ConvertAttachementAsync(element, attachementName);
                }
                catch
                {
                    _log.Error( Util.GetFormattedResource("AttachementCrawlerMessage01", attachementName, element.Id) );
                    ErrorControl.GetService().IncreaseErrorCount();
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
            Config cfg = ConfigReader.LoadConnectorConfig();

            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.BodyFieldName}", res.Body);

            element.ReplaceValues($"{cfg.FacetIdentifier}.{cfg.ApplicationFieldName}", res.Application);
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.ApplicationFieldName}", res.Application);

            element.ReplaceValues($"{cfg.FacetIdentifier}.{cfg.MimeTypeFieldName}", res.MimeType);
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.MimeTypeFieldName}", res.MimeType);

            string newId = $"{element.Id}_{fileName}";
            element.ReplaceValues($"{cfg.StringIdentifier}.{cfg.UidFieldName}", newId);
            element.Id = newId;
        }
    }
}
