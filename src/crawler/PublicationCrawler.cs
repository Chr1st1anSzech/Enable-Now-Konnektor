using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.crawler
{
    class PublicationCrawler
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig _jobConfig;



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
            _jobConfig = jobConfig;
        }




        /// <summary>
        /// Startet alle Threads, die die Elemente in der Warteschlange crawlen.
        /// </summary>
        public void StartCrawlingThreads()
        {
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage01"));
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




        /// <summary>
        /// Nimmt eine ID aus der Warteschlange, analysiert das Element, erstellt daraus ein Objekt und schickt es zum Indexieren.
        /// </summary>
        /// <param name="threadNumber">Die Nummer des Threads</param>
        private async Task EnterCrawlingLoopAsync(int threadNumber)
        {
            CrawlerIndexerInterface crawlerIndexerInterface = new CrawlerIndexerInterface(_jobConfig);
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage03"));
            while (IsAnyTaskActive())
            {

                if (_idWorkQueue.TryDequeue(out string id))
                {
                    await CrawlElementAsync(crawlerIndexerInterface, id);
                }
                else
                {
                    _log.Debug(Util.GetFormattedResource("PublicationCrawlerMessage04"));
                    Thread.Sleep(new Random().Next(500, 2000));
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
                _log.Info($"Thread={threadNumber}, Status={_taskStatus[threadNumber]}");
            }

            _log.Info("Fertig mit der Arbeit");
        }

        private async Task CrawlElementAsync(CrawlerIndexerInterface crawlerIndexerInterface, string id)
        {
            _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage02", id));
            ElementCrawler elementCrawler = new ElementCrawler(_jobConfig);
            AttachementCrawler attachementCrawler = new AttachementCrawler(_jobConfig);

            Element element = await elementCrawler.CrawlElement(id);
            if (element == null)
            {
                _log.Info(Util.GetFormattedResource("PublicationCrawlerMessage07", id) );
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

    }
}
