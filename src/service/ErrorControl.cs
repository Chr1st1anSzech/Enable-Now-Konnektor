using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.service
{
    class ErrorControl
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ErrorControl service = new ErrorControl();
        private readonly int maxErrorCount;
        private readonly int maxMinutesRuntime;
        private readonly Stopwatch watch = new Stopwatch();

        internal int ErrorCount { get; private set; } = 0;


        internal static ErrorControl GetService()
        {
            return service;
        }

        private ErrorControl()
        {
            Config cfg = ConfigReader.LoadConnectorConfig();
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
                log.Fatal(Util.GetFormattedResource("ErrorControlMessage01", elapsedMinutes));
                Environment.Exit(-1);
            }
        }

        private void CheckErrorCount()
        {
            if (ErrorCount > maxErrorCount)
            {
                log.Fatal(Util.GetFormattedResource("ErrorControlMessage02", ErrorCount));
                Environment.Exit(-1);
            }
        }
    }
}
