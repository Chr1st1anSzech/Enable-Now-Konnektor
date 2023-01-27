using Enable_Now_Konnektor.src.db;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.service;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.crawler
{
    internal class PublicationCrawler
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig _jobConfig;
        private readonly ElementCrawler _elementCrawler;
        private readonly AttachementCrawler _attachementCrawler;
        /// <summary>
        /// Die Warteschlange mit den IDs, die noch analysiert und indexiert werden müssen.
        /// </summary>
        private readonly ConcurrentQueue<string> _idWorkQueue = new();



        /// <summary>
        /// Status der Tasks.
        /// </summary>
        private bool[] _taskStatus;


        #region constructors
        /// <summary>
        /// Konstruktor der Klasse PublicationCrawler.
        /// </summary>
        /// <param name="jobConfig">Die Konfiguration des Jobs.</param>
        internal PublicationCrawler()
        {
            _jobConfig = JobManager.GetJobManager().SelectedJobConfig;
            _elementCrawler = new ElementCrawler();
            _attachementCrawler = new AttachementCrawler();
        }
        #endregion

        #region internal-methods
        /// <summary>
        /// 
        /// </summary>
        internal void Initialize()
        {
            InitializeCrawlerDatabase();
            InitializeStatisticService();
        }



        /// <summary>
        /// 
        /// </summary>
        internal void CompleteCrawling()
        {
            RemoveAllUnfoundElements();
        }



        /// <summary>
        /// Startet alle Threads, die die Elemente in der Warteschlange crawlen.
        /// </summary>
        internal void StartCrawling()
        {
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage01"));
            int threadCount = _jobConfig.ThreadCount;
            _taskStatus = new bool[threadCount];
            Task[] tasks = new Task[threadCount];
            _idWorkQueue.Enqueue(_jobConfig.StartId);
            for (int threadNumber = 0; threadNumber < threadCount; threadNumber++)
            {
                _taskStatus[threadNumber] = true;
                int i = threadNumber;
                tasks[threadNumber] = Task.Run(async delegate () { await EnterCrawlingLoopAsync(i); });
            }

            Task.WaitAll(tasks);
        }
        #endregion

        #region private-methods
        /// <summary>
        /// Nimmt eine ID aus der Warteschlange, analysiert das Element, erstellt daraus ein Objekt und schickt es zum Indexieren.
        /// </summary>
        /// <param name="threadNumber">Die Nummer des Threads</param>
        private async Task EnterCrawlingLoopAsync(int threadNumber)
        {
            CrawlerIndexerInterface crawlerIndexerInterface = new();
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage03"));
            while (IsAnyTaskActive())
            {
                await DequeueJob(crawlerIndexerInterface);

                SetTaskStatus(threadNumber);
            }

            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage08"));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlerIndexerInterface"></param>
        /// <returns></returns>
        private async Task DequeueJob(CrawlerIndexerInterface crawlerIndexerInterface)
        {
            if (_idWorkQueue.TryDequeue(out string id))
            {
                await CrawlElementAsync(crawlerIndexerInterface, id);
            }
            else
            {
                _log.Debug(LocalizationService.FormatResourceString("PublicationCrawlerMessage04"));
                Thread.Sleep(new Random().Next(50, 1000));
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadNumber"></param>
        private void SetTaskStatus(int threadNumber)
        {
            int countElementsInQueue = _idWorkQueue.Count;
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage05", countElementsInQueue));
            if (countElementsInQueue == 0)
            {
                _taskStatus[threadNumber] = false;
            }
            else
            {
                _taskStatus[threadNumber] = true;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void InitializeCrawlerDatabase()
        {
            using ElementLogContext context = new();
            context.Initialize();
            context.ResetAllFoundStatus(_jobConfig.Id);
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage09"));
        }



        /// <summary>
        /// 
        /// </summary>
        private void RemoveAllUnfoundElements()
        {
            using ElementLogContext context = new();
            context.GetAllElementLogs(e => e.WasFound == false && e.JobId == _jobConfig.Id).ToList().ForEach(e =>
            {
                CrawlerIndexerInterface crawlerIndexerInterface = new();
                crawlerIndexerInterface.RemoveElementCompletly(e.Id);
            });
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage10"));
        }



        /// <summary>
        /// 
        /// </summary>
        private void InitializeStatisticService()
        {
            StatisticService.Initialize(_jobConfig.Id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlerIndexerInterface"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task CrawlElementAsync(CrawlerIndexerInterface crawlerIndexerInterface, string id)
        {
            _log.Info(LocalizationService.FormatResourceString("PublicationCrawlerMessage02", id));
            Element element;
            try
            {
                element = await _elementCrawler.CrawlElementAsync(id);
            }
            catch
            {
                _log.Error(LocalizationService.FormatResourceString("PublicationCrawlerMessage07", id));
                ErrorControlService.GetService().IncreaseErrorCount();
                return;
            }

            foreach (string childId in element.ChildrenIds)
            {
                _idWorkQueue.Enqueue(childId);
            }

            await crawlerIndexerInterface.SendToIndexerAsync(element);

            await CrawlAttachements(crawlerIndexerInterface, element);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlerIndexerInterface"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private async Task CrawlAttachements(CrawlerIndexerInterface crawlerIndexerInterface, Element element)
        {
            List<Element> attachements = await _attachementCrawler.CrawlAttachementsAsync(element);
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
            foreach (bool isActive in _taskStatus)
            {
                isAnyTaskActive |= isActive;
            }
            return isAnyTaskActive;
        }
        #endregion
    }
}
