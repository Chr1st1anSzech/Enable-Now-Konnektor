using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.service;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
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
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage01", startTime));
            ErrorControlService.GetService().StartRuntimeStopwatch();
            JobManager manager = JobManager.GetJobManager();
            if (manager.AllJobs == null) { return; }

            int jobCount = manager.AllJobs.Count;
            List<Task> tasks = new();
            List<string> jobIds = new(jobCount);

            for (int i = 0; i < jobCount; i++)
            {
                if (manager.AllJobs[i] == null) { continue; }
                JobConfig jobConfig = manager.AllJobs[i];
                if (jobIds.Contains(jobConfig.Id) || string.IsNullOrWhiteSpace(jobConfig.Id) )
                {
                    log.Fatal(LocalizationService.FormatResourceString("JobSchedulerMessage04", jobConfig.Id));
                    Environment.Exit(-1);
                }

                if(jobIdParameters.Count == 0 || jobIdParameters.Contains(jobConfig.Id))
                {
                    jobIds.Add(jobConfig.Id);
                    Task t = Task.Run(delegate () { RunJob(jobConfig); });
                    t.Wait();
                }
                else
                {
                    log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage05", jobConfig.Id));
                }

            }
            
            //Task.WaitAll(tasks.ToArray());
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage02", duration));
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage03", endTime));

        }

        private void RunJob(JobConfig jobConfig)
        {
            JobManager.GetJobManager().SelectedJobConfig = jobConfig;
            PublicationCrawler crawler = new();
            crawler.Initialize();
            crawler.StartCrawling();
            crawler.CompleteCrawling();
            StatisticService.GetService(jobConfig.Id).PrintStatistic();
            ErrorControlService.GetService().PrintErrorStatistic();
            StatisticService service = StatisticService.GetService(jobConfig.Id);
            string text = LocalizationService.FormatResourceString("MailClientMessage01",
                jobConfig.Id,
                DateTime.Now,
                service.FoundDocumentsCount,
                service.IndexedDocumentsCount,
                service.RemovedDocumentsCount);
            new MailService(jobConfig).SendMail(text);
        }


    }
}
