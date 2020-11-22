using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;

namespace Enable_Now_Konnektor.src.statistic
{
    class StatisticService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, StatisticService> statisticServices = new Dictionary<string, StatisticService>();
        internal int ErrorCount { get; private set; } = 0;
        internal int IndexedDocumentsCount { get; private set; } = 0;
        internal int RemovedDocumentsCount { get; private set; } = 0;

        internal static void Initialize(string jobId)
        {
            if ( string.IsNullOrWhiteSpace(jobId))
            {
                string message = Util.GetFormattedResource("StatisticServiceMessage01");
                log.Error(message);
                throw new ArgumentNullException(message);
            }
            statisticServices.Add(jobId, new StatisticService());
        }

        internal static StatisticService GetService(string jobId)
        {
            if( !statisticServices.ContainsKey(jobId))
            {
                Initialize(jobId);
            }
            return statisticServices[jobId];
        }

        internal void IncreaseErrorCount()
        {
            ErrorCount++;
        }

        internal void IncreaseIndexedDocumentsCount()
        {
            IndexedDocumentsCount++;
        }

        internal void IncreaseRemovedDocumentsCount()
        {
            RemovedDocumentsCount++;
        }
    }
}
