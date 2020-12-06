using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;

namespace Enable_Now_Konnektor.src.service
{
    internal class Statistics
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, Statistics> statisticServices = new Dictionary<string, Statistics>();
        internal int IndexedDocumentsCount { get; private set; } = 0;
        internal int UnchangedDocumentsCount { get; private set; } = 0;
        internal int SkippedDocumentsCount { get; private set; } = 0;
        internal int RemovedDocumentsCount { get; private set; } = 0;
        internal int FoundDocumentsCount { get; private set; } = 0;
        internal int AutostartElementsCount { get; private set; } = 0;

        internal static void Initialize(string jobId)
        {
            if ( string.IsNullOrWhiteSpace(jobId))
            {
                string message = Util.GetFormattedResource("StatisticServiceMessage01");
                log.Error(message);
                throw new ArgumentNullException(message);
            }
            statisticServices.Add(jobId, new Statistics());
        }

        internal static Statistics GetService(string jobId)
        {
            if( !statisticServices.ContainsKey(jobId))
            {
                Initialize(jobId);
            }
            return statisticServices[jobId];
        }

        internal void PrintStatistic()
        {
            log.Info(Util.GetFormattedResource("StatisticsMessage02", FoundDocumentsCount));
            log.Info(Util.GetFormattedResource("StatisticsMessage03", AutostartElementsCount));
            log.Info(Util.GetFormattedResource("StatisticsMessage04", UnchangedDocumentsCount));
            log.Info(Util.GetFormattedResource("StatisticsMessage05", SkippedDocumentsCount));
            log.Info(Util.GetFormattedResource("StatisticsMessage06", IndexedDocumentsCount));
            log.Info(Util.GetFormattedResource("StatisticsMessage07", RemovedDocumentsCount));
        }

        internal void IncreaseSkippedDocumentsCount()
        {
            SkippedDocumentsCount++;
        }

        internal void IncreaseUnchangedDocumentsCount()
        {
            UnchangedDocumentsCount++;
        }

        internal void IncreaseFoundDocumentsCount()
        {
            FoundDocumentsCount++;
        }

        internal void IncreaseIndexedDocumentsCount()
        {
            IndexedDocumentsCount++;
        }

        internal void IncreaseRemovedDocumentsCount()
        {
            RemovedDocumentsCount++;
        }

        internal void IncreaseAutostartElementsCount()
        {
            AutostartElementsCount++;
        }
    }
}
