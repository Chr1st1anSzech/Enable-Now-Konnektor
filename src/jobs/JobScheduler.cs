using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.misc;
using Enable_Now_Konnektor.src.service;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.jobs
{
    internal class JobScheduler
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal void ScheduleJobs(List<string> jobIdParameters)
        {
            DateTime startTime = DateTime.Now;
            log.Info(Util.GetFormattedResource("JobSchedulerMessage01", startTime));
            ConfigReader.Initialize();
            ErrorControl.GetService().StartRuntimeStopwatch();
            JobReader reader = new JobReader();
            List<JobConfig> jobConfigs = reader.ReadAllJobConfigs();
            if (jobConfigs == null) { return; }

            int jobCount = jobConfigs.Count;
            List<Task> tasks = new List<Task>();
            List<string> jobIds = new List<string>(jobCount);

            for (int i = 0; i < jobCount; i++)
            {
                if (jobConfigs[i] == null) { continue; }
                var jobConfig = jobConfigs[i];
                if (jobIds.Contains(jobConfig.Id) || string.IsNullOrWhiteSpace(jobConfig.Id) )
                {
                    log.Fatal(Util.GetFormattedResource("JobSchedulerMessage04", jobConfig.Id));
                    Environment.Exit(-1);
                }

                if(jobIdParameters.Count == 0 || jobIdParameters.Contains(jobConfig.Id))
                {
                    jobIds.Add(jobConfig.Id);
                    Task t = Task.Run(delegate () { RunJob(jobConfig); });
                    tasks.Add(t);
                }
                else
                {
                    log.Info(Util.GetFormattedResource("JobSchedulerMessage05", jobConfig.Id));
                }

            }
            
            Task.WaitAll(tasks.ToArray());
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            log.Info(Util.GetFormattedResource("JobSchedulerMessage02", duration));
            log.Info(Util.GetFormattedResource("JobSchedulerMessage03", endTime));

        }

        private void RunJob(JobConfig jobConfig)
        {
            PublicationCrawler crawler = new PublicationCrawler(jobConfig);
            crawler.Initialize();
            crawler.StartCrawling();
            crawler.CompleteCrawling();
            Statistics.GetService(jobConfig.Id).PrintStatistic();
            ErrorControl.GetService().PrintErrorStatistic();
            MailClient mail = new MailClient(jobConfig);
            mail.SendMail();
        }


    }
}
