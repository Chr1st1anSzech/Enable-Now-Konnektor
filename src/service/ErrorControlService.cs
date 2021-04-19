using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.service
{
    class ErrorControlService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ErrorControlService service = new ErrorControlService();
        private readonly int maxErrorCount;
        private readonly int maxMinutesRuntime;
        private readonly Stopwatch watch = new Stopwatch();

        internal int ErrorCount { get; private set; } = 0;


        internal static ErrorControlService GetService()
        {
            return service;
        }

        private ErrorControlService()
        {
            Config cfg = ConfigManager.GetConfigManager().ConnectorConfig;
            maxErrorCount = cfg.MaxErrorCount;
            maxMinutesRuntime = cfg.MaxMinutesRuntime;
        }

        internal void StartRuntimeStopwatch()
        {
            watch.Start();
            Task.Run( () => {
                while (true)
                {
                    CheckRuntime();
                    Thread.Sleep(60000);
                }
            });
        }

        internal void PrintErrorStatistic()
        {
            log.Info(LocalizationService.FormatResourceString("ErrorControlMessage3", ErrorCount));
        }

        internal void IncreaseErrorCount()
        {
            ErrorCount++;
            CheckErrorCount();
        }

        private void CheckRuntime()
        {
            int elapsedMinutes = (int) watch.ElapsedMilliseconds / 1000 / 60;
            if( elapsedMinutes > maxMinutesRuntime)
            {
                log.Fatal(LocalizationService.FormatResourceString("ErrorControlMessage01", elapsedMinutes));
                Environment.Exit(-1);
            }
        }

        private void CheckErrorCount()
        {
            if (ErrorCount > maxErrorCount)
            {
                log.Fatal(LocalizationService.FormatResourceString("ErrorControlMessage02", ErrorCount));
                Environment.Exit(-1);
            }
        }
    }
}
