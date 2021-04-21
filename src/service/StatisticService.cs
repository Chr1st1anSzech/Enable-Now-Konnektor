using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Collections.Generic;

namespace Enable_Now_Konnektor.src.service
{
    public class StatisticService
    {
        private static readonly ILog s_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, StatisticService> s_statisticServices = new();

        public int IndexedDocumentsCount { get; private set; } = 0;
        public int UnchangedDocumentsCount { get; private set; } = 0;
        public int SkippedDocumentsCount { get; private set; } = 0;
        public int RemovedDocumentsCount { get; private set; } = 0;
        public int FoundDocumentsCount { get; private set; } = 0;
        public int AutostartElementsCount { get; private set; } = 0;

        public static void Initialize(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId))
            {
                string message = LocalizationService.FormatResourceString("StatisticsMessage01");
                s_log.Error(message);
                throw new ArgumentNullException(message);
            }
            s_statisticServices.Add(jobId, new StatisticService());
        }

        public static StatisticService GetService(string jobId)
        {
            if (!s_statisticServices.ContainsKey(jobId))
            {
                Initialize(jobId);
            }
            return s_statisticServices[jobId];
        }

        public void PrintStatistic()
        {
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage02", FoundDocumentsCount));
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage03", AutostartElementsCount));
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage04", UnchangedDocumentsCount));
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage05", SkippedDocumentsCount));
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage06", IndexedDocumentsCount));
            s_log.Info(LocalizationService.FormatResourceString("StatisticsMessage07", RemovedDocumentsCount));
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
