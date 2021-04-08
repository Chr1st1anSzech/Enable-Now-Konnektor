using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Collections.Generic;

namespace Enable_Now_Konnektor.src.service
{
    public class StatisticService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, StatisticService> statisticServices = new Dictionary<string, StatisticService>();
        public int IndexedDocumentsCount { get; private set; } = 0;
        public int UnchangedDocumentsCount { get; private set; } = 0;
        public int SkippedDocumentsCount { get; private set; } = 0;
        public int RemovedDocumentsCount { get; private set; } = 0;
        public int FoundDocumentsCount { get; private set; } = 0;
        public int AutostartElementsCount { get; private set; } = 0;

        public static void Initialize(string jobId)
        {
            if ( string.IsNullOrWhiteSpace(jobId))
            {
                string message = LocalizationService.GetFormattedResource("StatisticsMessage01");
                log.Error(message);
                throw new ArgumentNullException(message);
            }
            statisticServices.Add(jobId, new StatisticService());
        }

        public static StatisticService GetService(string jobId)
        {
            if( !statisticServices.ContainsKey(jobId))
            {
                Initialize(jobId);
            }
            return statisticServices[jobId];
        }

        public void PrintStatistic()
        {
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage02", FoundDocumentsCount));
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage03", AutostartElementsCount));
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage04", UnchangedDocumentsCount));
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage05", SkippedDocumentsCount));
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage06", IndexedDocumentsCount));
            log.Info(LocalizationService.GetFormattedResource("StatisticsMessage07", RemovedDocumentsCount));
        }

        public void IncreaseSkippedDocumentsCount()
        {
            SkippedDocumentsCount++;
        }

        public void IncreaseUnchangedDocumentsCount()
        {
            UnchangedDocumentsCount++;
        }

        public void IncreaseFoundDocumentsCount()
        {
            FoundDocumentsCount++;
        }

        public void IncreaseIndexedDocumentsCount()
        {
            IndexedDocumentsCount++;
        }

        public void IncreaseRemovedDocumentsCount()
        {
            RemovedDocumentsCount++;
        }

        public void IncreaseAutostartElementsCount()
        {
            AutostartElementsCount++;
        }
    }
}
