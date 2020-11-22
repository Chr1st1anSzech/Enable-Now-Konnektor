using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.db;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.indexing;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using Enable_Now_Konnektor.src.statistic;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.crawler
{
    class PublicationCrawler
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;
        private readonly ElementCrawler elementCrawler;
        private readonly AttachementCrawler attachementCrawler;


        /// <summary>
        /// Die Warteschlange mit den IDs, die noch analysiert und indexiert werden müssen.
        /// </summary>
        private readonly ConcurrentQueue<string> _idWorkQueue = new ConcurrentQueue<string>();


        /// <summary>
        /// Status der Tasks.
        /// </summary>
        private bool[] _taskStatus;



        /// <summary>
        /// Konstruktor der Klasse PublicationCrawler.
        /// </summary>
        /// <param name="jobConfig">Die Konfiguration des Jobs.</param>
        public PublicationCrawler(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
            elementCrawler = new ElementCrawler(this.jobConfig);
            attachementCrawler = new AttachementCrawler(this.jobConfig);
            InitializeCrawlerDatabase();
            InitializeStatisticService();
        }

        private void InitializeCrawlerDatabase()
        {
            using ElementLogContext context = new ElementLogContext(jobConfig.Id);
            context.Initialize();
            context.ResetAllFoundStatus();
        }

        private void RemoveAllUnfoundElements()
        {
            using ElementLogContext context = new ElementLogContext(jobConfig.Id);
            context.GetAllElementLogs(e => e.WasFound == false).ToList().ForEach(e =>
           {
               CrawlerIndexerInterface crawlerIndexerInterface = new CrawlerIndexerInterface(jobConfig);
               crawlerIndexerInterface.RemoveElementCompletly(e.Id);
           });
        }

        private void InitializeStatisticService()
        {
            StatisticService.Initialize(jobConfig.Id);
        }




        /// <summary>
        /// Startet alle Threads, die die Elemente in der Warteschlange crawlen.
        /// </summary>
        public void StartCrawlingThreads()
        {
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage01"));
            int threadCount = jobConfig.ThreadCount;
            _taskStatus = new bool[threadCount];
            Task[] tasks = new Task[threadCount];
            _idWorkQueue.Enqueue(jobConfig.StartId);
            for (int threadNumber = 0; threadNumber < threadCount; threadNumber++)
            {
                _taskStatus[threadNumber] = true;
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
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage03"));
            while (IsAnyTaskActive())
            {
                if (TooMuchErrors())
                {
                    _log.Error(Util.GetFormattedResource("PublicationCrawlerMessage06"));
                    break;
                }

                if (_idWorkQueue.TryDequeue(out string id))
                {
                    await CrawlElementAsync(crawlerIndexerInterface, id);
                }
                else
                {
                    _log.Debug(Util.GetFormattedResource("PublicationCrawlerMessage04"));
                    Thread.Sleep(new Random().Next(50, 500));
                }

                int countElementsInQueue = _idWorkQueue.Count;
                _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage05", countElementsInQueue));
                if (countElementsInQueue == 0)
                {
                    _taskStatus[threadNumber] = false;
                }
                else
                {
                    _taskStatus[threadNumber] = true;
                }
            }

            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage08"));
        }

        private async Task CrawlElementAsync(CrawlerIndexerInterface crawlerIndexerInterface, string id)
        {
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage02", id));
            Element element;
            try
            {
                element = await elementCrawler.CrawlElement(id);
            }
            catch
            {
                _log.Error(Util.GetFormattedResource("PublicationCrawlerMessage07", id));
                StatisticService.GetService(jobConfig.Id).IncreaseErrorCount();
                return;
            }
            
            foreach (var childId in element.ChildrenIds)
            {
                _idWorkQueue.Enqueue(childId);
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
            foreach (var isActive in _taskStatus)
            {
                isAnyTaskActive |= isActive;
            }
            return isAnyTaskActive;
        }



        private bool TooMuchErrors()
        {
            return StatisticService.GetService(jobConfig.Id).ErrorCount > jobConfig.MaxErrorCount;
        }

    }
}
