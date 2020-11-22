using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.db;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.jobs
{
    class JobScheduler
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public JobScheduler()
        {
        }

        public void ScheduleJobs()
        {
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage02", DateTime.Now));
            ConfigReader.Initialize();
            JobReader reader = new JobReader();
            JobConfig[] jobConfigs = reader.ReadAllJobConfigs();
            if (jobConfigs == null) { return; }

            int jobCount = jobConfigs.Length;
            Task[] tasks = new Task[jobCount];
            for (int i = 0; i < jobCount; i++)
            {
                var jobConfig = jobConfigs[i];
                if (jobConfig == null) { continue; }

                tasks[i] = Task.Run(delegate () { StartJob(jobConfig); });
            }
            Task.WaitAll(tasks);
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage04", DateTime.Now));

            
        }

        private void StartJob(JobConfig jobConfig)
        {
            PublicationCrawler crawler = new PublicationCrawler(jobConfig);
            crawler.StartCrawlingThreads();
            MailClient mail = new MailClient(jobConfig);
            mail.SendMail("");
        }


    }
}
