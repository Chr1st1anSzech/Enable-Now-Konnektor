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
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public JobScheduler()
        {
        }

        public void ScheduleJobs()
        {
            JobReader reader = new JobReader();
            JobConfig[] jobConfigs = reader.ReadAllJobConfigs();
            if (jobConfigs == null) { return; }

            int jobCount = jobConfigs.Length;
            Action[] actions = new Action[jobCount];
            for (int i = 0; i < jobCount; i++)
            {
                var jobConfig = jobConfigs[i];
                if (jobConfig == null) { continue; }

                actions[i] = delegate () { StartJob(jobConfig); };
            }
            Parallel.Invoke(actions);

        }

        private void StartJob(JobConfig jobConfig)
        {
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage01", jobConfig.Id));
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage02", DateTime.Now));
            PublicationCrawler crawler = new PublicationCrawler(jobConfig);
            crawler.StartCrawlingThreads();
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage03", jobConfig.Id));
            _log.Info(Util.GetFormattedResource("JobSchedulerMessage04", DateTime.Now));

            MailClient mail = new MailClient(jobConfig);
            mail.SendMail("");
        }


    }
}
