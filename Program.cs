using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using log4net.Config;
using System;
using System.IO;

namespace Enable_Now_Konnektor
{
    class Program
    {
        private static ILog _log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            ConfigLogging();

            JobScheduler scheduler = new JobScheduler();
            scheduler.ScheduleJobs();
        }

        private static void ConfigLogging()
        {
            var logFile = Path.Combine(Util.GetApplicationRoot(), "log4net.xml");
            var logDirectory = Path.Combine(Util.GetApplicationRoot(), "logs");
            if ( Util.IsDirectoryWritable(logDirectory))
            {
                _log.Warn(Util.GetFormattedResource("ProgramMessage01"));
            }
            XmlConfigurator.Configure(new FileInfo(logFile));
        }
    }
}
