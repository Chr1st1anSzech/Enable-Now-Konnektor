using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.jobs
{
    class JobScheduler
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public JobScheduler()
        {
        }

        public void ScheduleJobs()
        {
            DateTime startTime = DateTime.Now;
            log.Info(Util.GetFormattedResource("JobSchedulerMessage01", startTime));
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
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            log.Info(Util.GetFormattedResource("JobSchedulerMessage02", duration));
            log.Info(Util.GetFormattedResource("JobSchedulerMessage03", endTime));
            
        }

        private void StartJob(JobConfig jobConfig)
        {
            PublicationCrawler crawler = new PublicationCrawler(jobConfig);
            crawler.Initialize();
            crawler.StartCrawling();
            crawler.CompleteCrawling();
            MailClient mail = new MailClient(jobConfig);
            mail.SendMail("");
        }


    }
}
