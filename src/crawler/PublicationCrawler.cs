using Enable_Now_Konnektor.src.db;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using Enable_Now_Konnektor.src.service;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.crawler
{
    internal class PublicationCrawler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;
        private readonly ElementCrawler elementCrawler;
        private readonly AttachementCrawler attachementCrawler;


        /// <summary>
        /// Die Warteschlange mit den IDs, die noch analysiert und indexiert werden müssen.
        /// </summary>
        private readonly ConcurrentQueue<string> idWorkQueue = new ConcurrentQueue<string>();


        /// <summary>
        /// Status der Tasks.
        /// </summary>
        private bool[] taskStatus;



        /// <summary>
        /// Konstruktor der Klasse PublicationCrawler.
        /// </summary>
        /// <param name="jobConfig">Die Konfiguration des Jobs.</param>
        internal PublicationCrawler(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
            elementCrawler = new ElementCrawler(this.jobConfig);
            attachementCrawler = new AttachementCrawler(this.jobConfig);
        }

        private void InitializeCrawlerDatabase()
        {
            using ElementLogContext context = new ElementLogContext(jobConfig.Id);
            context.Initialize();
            context.ResetAllFoundStatus();
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage09"));
        }

        private void RemoveAllUnfoundElements()
        {
            using ElementLogContext context = new ElementLogContext(jobConfig.Id);
            context.GetAllElementLogs(e => e.WasFound == false).ToList().ForEach(e =>
           {
               CrawlerIndexerInterface crawlerIndexerInterface = new CrawlerIndexerInterface(jobConfig);
               crawlerIndexerInterface.RemoveElementCompletly(e.Id);
           });
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage10"));
        }

        private void InitializeStatisticService()
        {
            Statistics.Initialize(jobConfig.Id);
        }

        internal void Initialize()
        {
            InitializeCrawlerDatabase();
            InitializeStatisticService();
        }

        internal void CompleteCrawling()
        {
            RemoveAllUnfoundElements();
            WriteStatistics();
        }

        internal void WriteStatistics()
        {
            Statistics statistic = Statistics.GetService(jobConfig.Id);
            ErrorControl errorControl = ErrorControl.GetService();
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage11", errorControl.ErrorCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage14", statistic.FoundDocumentsCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage15", statistic.AutostartElementsCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage16", statistic.UnchangedDocumentsCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage17", statistic.SkippedDocumentsCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage12", statistic.IndexedDocumentsCount));
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage13", statistic.RemovedDocumentsCount));
        }


        /// <summary>
        /// Startet alle Threads, die die Elemente in der Warteschlange crawlen.
        /// </summary>
        internal void StartCrawling()
        {
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage01"));
            int threadCount = jobConfig.ThreadCount;
            taskStatus = new bool[threadCount];
            Task[] tasks = new Task[threadCount];
            idWorkQueue.Enqueue(jobConfig.StartId);
            for (int threadNumber = 0; threadNumber < threadCount; threadNumber++)
            {
                taskStatus[threadNumber] = true;
                int i = threadNumber;
                tasks[threadNumber] = Task.Run(async delegate () { await EnterCrawlingLoopAsync(i); });
            }

            Task.WaitAll(tasks);
        }




        /// <summary>
        /// Nimmt eine ID aus der Warteschlange, analysiert das Element, erstellt daraus ein Objekt und schickt es zum Indexieren.
        /// </summary>
        /// <param name="threadNumber">Die Nummer des Threads</param>
        private async Task EnterCrawlingLoopAsync(int threadNumber)
        {
            CrawlerIndexerInterface crawlerIndexerInterface = new CrawlerIndexerInterface(jobConfig);
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage03"));
            while (IsAnyTaskActive())
            {
                if (idWorkQueue.TryDequeue(out string id))
                {
                    await CrawlElementAsync(crawlerIndexerInterface, id);
                }
                else
                {
                    log.Debug(Util.GetFormattedResource("PublicationCrawlerMessage04"));
                    Thread.Sleep(new Random().Next(50, 1000));
                }

                int countElementsInQueue = idWorkQueue.Count;
                log.Info(Util.GetFormattedResource("PublicationCrawlerMessage05", countElementsInQueue));
                if (countElementsInQueue == 0)
                {
                    taskStatus[threadNumber] = false;
                }
                else
                {
                    taskStatus[threadNumber] = true;
                }
            }

            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage08"));
        }

        private async Task CrawlElementAsync(CrawlerIndexerInterface crawlerIndexerInterface, string id)
        {
            log.Info(Util.GetFormattedResource("PublicationCrawlerMessage02", id));
            Element element;
            try
            {
                element = await elementCrawler.CrawlElement(id);
            }
            catch
            {
                log.Error(Util.GetFormattedResource("PublicationCrawlerMessage07", id));
                ErrorControl.GetService().IncreaseErrorCount();
                return;
            }

            foreach (var childId in element.ChildrenIds)
            {
                idWorkQueue.Enqueue(childId);
            }

            await crawlerIndexerInterface.SendToIndexerAsync(element);

            var attachements = await attachementCrawler.CrawlAttachementsAsync(element);
            //Task[] attachementTasks = new Task[attachements.Count];
            for (int i = 0; i < attachements.Count; i++)
            {
                await crawlerIndexerInterface.SendToIndexerAsync(attachements[i]);
            }
        }




        /// <summary>
        /// Prüft, ob mindestens ein Thread noch aktiv ist.
        /// </summary>
        /// <returns>Ist wahr, wenn noch irgendein Thread aktiv ist.</returns>
        private bool IsAnyTaskActive()
        {
            bool isAnyTaskActive = false;
            foreach (var isActive in taskStatus)
            {
                isAnyTaskActive |= isActive;
            }
            return isAnyTaskActive;
        }

    }
}
