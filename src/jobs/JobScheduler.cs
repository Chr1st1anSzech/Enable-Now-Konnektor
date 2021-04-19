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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        #region internal-methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobIdParameters"></param>
        internal static void ScheduleJobs(List<string> jobIdParameters)
        {
            DateTime startTime = DateTime.Now;
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage01", startTime));
            ErrorControlService.GetService().StartRuntimeStopwatch();
            JobManager manager = JobManager.GetJobManager();
            if (manager.AllJobs == null) { return; }

            int jobCount = manager.AllJobs.Count;
            List<string> jobIds = new(jobCount);

            for (int i = 0; i < jobCount; i++)
            {
                JobConfig jobConfig = manager.AllJobs[i];
                if (jobConfig == null) { continue; }

                ExitWhenInvalidId(jobIds, jobConfig.Id);

                InitNewThread(jobIdParameters, jobIds, jobConfig);

            }

            LogTime(startTime);

        }
        #endregion

        #region private-methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        private static void LogTime(DateTime startTime)
        {
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage02", duration));
            log.Info(LocalizationService.FormatResourceString("JobSchedulerMessage03", endTime));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobIdParameters"></param>
        /// <param name="jobIds"></param>
        /// <param name="jobConfig"></param>
        private static void InitNewThread(List<string> jobIdParameters, List<string> jobIds, JobConfig jobConfig)
        {
            if (jobIdParameters.Count == 0 || jobIdParameters.Contains(jobConfig.Id))
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



        /// <summary>
        /// Das Programm beenden, wenn die JobId leer ist oder doppelt auftritt.
        /// </summary>
        /// <param name="jobIds">Liste der JobIds, die bereits abgearbeitet sind</param>
        /// <param name="jobConfig"></param>
        private static void ExitWhenInvalidId(List<string> jobIds, string jobId)
        {
            if (jobIds.Contains(jobId) || string.IsNullOrWhiteSpace(jobId))
            {
                log.Fatal(LocalizationService.FormatResourceString("JobSchedulerMessage04", jobId));
                Environment.Exit(-1);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobConfig"></param>
        private static void RunJob(JobConfig jobConfig)
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
        #endregion
    }
}
