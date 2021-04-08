using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Enable_Now_Konnektor
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            ConfigLogging();
            var jobIdParameters = GetJobParameter(args);
            JobScheduler scheduler = new JobScheduler();
            scheduler.ScheduleJobs(jobIdParameters);

            
        }

        private static List<string> GetJobParameter(string[] args)
        {
            if( args.Length == 0 ) { return new List<string>(); }
            List<string> parameters = args.Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
            foreach (string p in parameters)
            {
                log.Info(LocalizationService.GetFormattedResource("ProgramMessage02", p));
            }
            return parameters;
        }

        private static void ConfigLogging()
        {
            var logFile = Path.Combine(Util.GetApplicationRoot(), "log4net.xml");
            var logDirectory = Path.Combine(Util.GetApplicationRoot(), "logs");
            if ( Util.IsDirectoryWritable(logDirectory))
            {
                log.Warn(LocalizationService.GetFormattedResource("ProgramMessage01"));
            }
            XmlConfigurator.Configure(new FileInfo(logFile));
        }
    }
}
