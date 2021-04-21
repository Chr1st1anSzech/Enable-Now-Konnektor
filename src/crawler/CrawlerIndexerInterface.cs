using Enable_Now_Konnektor.src.db;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.indexing;
using Enable_Now_Konnektor.src.service;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.crawler
{
    internal class CrawlerIndexerInterface
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig _jobConfig;
        private readonly Indexer _indexer;


        #region constructors
        internal CrawlerIndexerInterface()
        {
            _jobConfig = JobManager.GetJobManager().SelectedJobConfig;
            _indexer = new JsonIndexer();
        }
        #endregion


        #region internal-methods
        /// <summary>
        /// Prüfe das Element zuvor, bevor man es zum Indexieren schickt.
        /// <para>Es werden mehrere Parameter geprüft.
        /// <list type="number">
        /// <item>Soll das Element überhaupt indexiert werden: Soll diese Klasse indexiert werden? Sind alle Pflichtfelder vorhanden? Ist irgendein Ausnahmewert vorhanden?</item>
        /// <item>Ist das Element bereits indexiert?</item>
        /// <item>Hat sich der Inhalt des Elements geändert? </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="element">Das Element, das indexiert werden soll.</param>
        internal async Task SendToIndexerAsync(Element element)
        {
            bool isAlreadyIndexed = IsAlreadyIndexed(element);
            bool isForbidden = RemoveWhenForbidden(element, isAlreadyIndexed);
            if (isForbidden) return;

            using ElementLogContext context = new();
            bool hasContentChanged = HasContentChanged(element);
            bool isIndexingNeeded = RemoveWhenChanged(element, isAlreadyIndexed, hasContentChanged, context);
            if (isIndexingNeeded == false) return;

            _log.Info(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage03", element.Id));

            bool isIndexingSuccess = await _indexer.AddElementToIndexAsync(element);
            MarkElementFound(element, context, isIndexingSuccess);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        internal void RemoveElementCompletly(Element element)
        {
            RemoveElementCompletly(element.Id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveElementCompletly(string id)
        {
            _log.Debug(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage04", id));
            using ElementLogContext context = new();
            context.RemoveElementLog(id, _jobConfig.Id);
            _indexer.RemoveElementFromIndexAsync(id);
            StatisticService.GetService(_jobConfig.Id).IncreaseRemovedDocumentsCount();
        }
        #endregion


        #region private-methods
        /// <summary>
        /// Prüft, ob das Element bereits im Index ist.
        /// </summary>
        /// <param name="element">Das Element, das auf Vorhandensein im Index geprüft werden soll.</param>
        /// <returns>Wahr, wenn es bereits vorhanden ist, ansonsten falsch.</returns>
        private bool IsAlreadyIndexed(Element element)
        {
            using ElementLogContext context = new ElementLogContext();
            var elementLog = context.GetElementLog(element, _jobConfig.Id);
            return (elementLog != null);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="isAlreadyIndexed"></param>
        private bool RemoveWhenForbidden(Element element, bool isAlreadyIndexed)
        {
            if (!ShouldBeIndexed(element))
            {
                _log.Info(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage01", element.Id));
                StatisticService.GetService(_jobConfig.Id).IncreaseSkippedDocumentsCount();
                if (isAlreadyIndexed)
                {
                    RemoveElementCompletly(element);
                };
                return true;
            }
            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="isAlreadyIndexed"></param>
        /// <param name="hasContentChanged"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool RemoveWhenChanged(Element element, bool isAlreadyIndexed, bool hasContentChanged, ElementLogContext context)
        {
            if (isAlreadyIndexed)
            {
                if (hasContentChanged)
                {
                    _log.Info(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage02", element.Id));
                    RemoveElementCompletly(element);
                    return true;
                }
                else
                {
                    _log.Info(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage07", element.Id));
                    StatisticService.GetService(_jobConfig.Id).IncreaseUnchangedDocumentsCount();
                    context.SetElementFound(element, _jobConfig.Id, true);
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        /// <param name="isIndexingSuccess"></param>
        private void MarkElementFound(Element element, ElementLogContext context, bool isIndexingSuccess)
        {
            if (isIndexingSuccess)
            {
                context.SetElementFound(element, _jobConfig.Id, true);
                _log.Info(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage06", element.Id));
                StatisticService.GetService(_jobConfig.Id).IncreaseIndexedDocumentsCount();
            }
            else
            {
                _log.Error(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage08", element.Id));
                ErrorControlService.GetService().IncreaseErrorCount();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsAllowedClassForIndexing(Element element)
        {
            switch (element.Class)
            {
                case Element.Slide:
                    {
                        return _jobConfig.IndexSlides;
                    }
                case Element.Group:
                    {
                        return _jobConfig.IndexGroups;
                    }
                case Element.Project:
                    {
                        return _jobConfig.IndexProjects;
                    }
                case Element.Book:
                    {
                        return _jobConfig.IndexBooks;
                    }
                case Element.Text:
                    {
                        return _jobConfig.IndexText;
                    }
                default:
                    break;
            }
            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool ShouldBeIndexed(Element element)
        {
            return !HasBlacklistedValue(element)
                && HasAllMustHaveFields(element)
                && IsAllowedClassForIndexing(element);
        }



        /// <summary>
        /// Prüft, ob ein Element veraltet ist und deswegen indexiert werden muss.
        /// <para>Das Element kann veraltet sein, indem es entweder noch gar nicht vorhanden ist oder wenn sich der Hash-Wert - also der Inhalt - verändert hat.
        /// Andernfalls muss man keine Anfrage an den Index-Service senden.</para>
        /// </summary>
        /// <param name="element">Das zu prüfende Element.</param>
        /// <returns>Gibt wahr zurück, wenn es veraltet ist und indexiert werden muss, ansonsten falsch.</returns>
        private bool HasContentChanged(Element element)
        {
            using ElementLogContext context = new ElementLogContext();
            var elementLog = context.GetElementLog(element, _jobConfig.Id);
            return elementLog == null || !elementLog.Hash.Equals(element.Hash);
        }



        /// <summary>
        /// Prüft, ob das Element indexiert oder ausgeschlossen werden soll.
        /// <para>Es wird durch alle Feldnamen in der Ausnahmeliste iteriert. Falls das Element ebenfalls ein Feld mit diesen Namen enthält,
        /// wird durch dessen Werte iteriert. Es wird geprüft, ob der Wert mit dem regulären Ausdruck in der Ausnahmeliste übereinstimmt.</para>
        /// </summary>
        /// <param name="element">Das Objekt des Elements, das geprüft werden soll.</param>
        /// <returns>Gibt wahr zurück, wenn ein Wert auf der Ausnahmeliste erscheint, ansonsten falsch.</returns>
        private bool HasBlacklistedValue(Element element)
        {
            // Werte, die in beiden Listen sind
            var fieldnames = from fieldName in _jobConfig.BlacklistFields.Keys
                             join key in element.Fields.Keys
                             on fieldName equals key
                             select fieldName;
            foreach (var fieldName in fieldnames)
            {
                var values = element.Fields[fieldName];
                foreach (var value in values)
                {
                    try
                    {
                        if (Regex.IsMatch(value, _jobConfig.BlacklistFields[fieldName]))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        _log.Debug(LocalizationService.FormatResourceString("CrawlerIndexerInterfaceMessage05", element.Id));
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// Prüft, ob es mindestens einen nichtleeren Wert bei den Pflichtfelder gibt.
        /// <para>Es wird durch alle Feldnamen in der Ausnahmeliste iteriert. Falls das Element ebenfalls ein Feld mit diesen Namen enthält,
        /// wird durch dessen Werte iteriert. Es wird geprüft, ob der Wert nicht leer ist. Falls alle Werte eines Pflichtfeld leer sind,
        /// wird falsch zurückgegeben.</para>
        /// </summary>
        /// <param name="element">Das Objekt des Elements, das geprüft werden soll.</param>
        /// <returns>Gibt wahr zurück, wenn alle Pflichtfelder mindestens einen nichtleeren Wert haben, ansonsten falsch.</returns>
        private bool HasAllMustHaveFields(Element element)
        {
            var mustHaveFieldNames = from fieldName in _jobConfig.MustHaveFields
                                     join key in element.Fields.Keys
                                     on fieldName equals key
                                     select fieldName;

            foreach (var fieldName in mustHaveFieldNames)
            {
                bool isAnyValueNotEmpty = false;
                var values = element.Fields[fieldName];
                foreach (var value in values)
                {
                    isAnyValueNotEmpty |= !string.IsNullOrWhiteSpace(value);
                }

                if (!isAnyValueNotEmpty)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
