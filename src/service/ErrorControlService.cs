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
        private static readonly ILog s_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ErrorControlService s_service = new();
        private readonly int _maxErrorCount;
        private readonly int _maxMinutesRuntime;
        private readonly Stopwatch _watch = new();

        internal int ErrorCount { get; private set; } = 0;


        internal static ErrorControlService GetService()
        {
            return s_service;
        }

        private ErrorControlService()
        {
            Config cfg = ConfigManager.GetConfigManager().ConnectorConfig;
            _maxErrorCount = cfg.MaxErrorCount;
            _maxMinutesRuntime = cfg.MaxMinutesRuntime;
        }

        internal void StartRuntimeStopwatch()
        {
            _watch.Start();
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
            s_log.Info(LocalizationService.FormatResourceString("ErrorControlMessage3", ErrorCount));
        }

        internal void IncreaseErrorCount()
        {
            ErrorCount++;
            CheckErrorCount();
        }

        private void CheckRuntime()
        {
            int elapsedMinutes = (int) _watch.ElapsedMilliseconds / 1000 / 60;
            if( elapsedMinutes > _maxMinutesRuntime)
            {
                s_log.Fatal(LocalizationService.FormatResourceString("ErrorControlMessage01", elapsedMinutes));
                Environment.Exit(-1);
            }
        }

        private void CheckErrorCount()
        {
            if (ErrorCount > _maxErrorCount)
            {
                s_log.Fatal(LocalizationService.FormatResourceString("ErrorControlMessage02", ErrorCount));
                Environment.Exit(-1);
            }
        }
    }
}
