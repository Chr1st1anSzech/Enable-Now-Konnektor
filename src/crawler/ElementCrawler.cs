using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Enable_Now_Konnektor.src.misc.MetaFileReader;

namespace Enable_Now_Konnektor.src.crawler
{
    /// <summary>
    /// Klasse, die sich um die Analyse eines Elements in Enable Now kümmmert.
    /// <para>Es wird ein Objekt erstellt, das dem Element in Enable Now entspricht. Das Objekt wird initial gefüllt.</para>
    /// </summary>
    class ElementCrawler
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _variablePattern = @"(?<=\${).*(?=})";
        private readonly JobConfig _jobConfig;

        /// <summary>
        /// Felder eines Projekts, die von einem Autostart-Element auf das andere Element abgebildet werden sollen
        /// </summary>
        private List<string> _projectMappingFields;
        /// <summary>
        /// Felder einer Gruppe, die von einem Autostart-Element auf das andere Element abgebildet werden sollen
        /// </summary>
        private List<string> _groupMappingFields;


        public ElementCrawler(JobConfig jobConfig)
        {
            _jobConfig = jobConfig;
            InitializeMappingFields();
        }




        /// <summary>
        /// Initialisiert die Felder, die vom Autostart-Element gemappt werden sollen, außer sie stehen auf der Blacklist.
        /// <para>Diese Liste ist für den gesamten Job immer gleich. Deswegen reicht es, das einmal zu machen, anstatt bei jedem 
        /// Mapping zu prüfen, ob der Wert auf der Blacklist steht.</para>
        /// </summary>
        private void InitializeMappingFields()
        {
            // Mapping ist komplett deaktiviert oder das Überschreiben ist deaktiviert
            if (!_jobConfig.AutostartMetaMapping || !_jobConfig.AutostartChildOverwrite)
            {
                return;
            }

            _projectMappingFields = new List<string>();
            _groupMappingFields = new List<string>();

            // alle globalen Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(_jobConfig.GlobalMappings.Keys, _jobConfig.AutoStartMappingBlacklist, _projectMappingFields, _groupMappingFields);

            // alle Projekt-Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(_jobConfig.ProjectMappings.Keys, _jobConfig.AutoStartMappingBlacklist, _projectMappingFields);

            // alle Gruppen-Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(_jobConfig.GroupMappings.Keys, _jobConfig.AutoStartMappingBlacklist, _groupMappingFields);
        }




        /// <summary>
        /// Die Daten eines Elements analysieren und daraus ein Objekt erstellen.
        /// </summary>
        /// <param name="id">Die ID des Element, zum Beispiel GR_389F860B088563B1.</param>
        /// <returns>Ein Objekt, das die Daten des Elements in Enable Now enthält</returns>
        public async Task<Element> CrawlElement(string id)
        {
            _log.Debug($"Crawle das Objekt mit der ID '{id}'.");
            ElementFactory factory = new ElementFactory( new UrlFormatter(_jobConfig) );
            Element element = factory.CreateENObject(id);
            FillInitialFields(element);
            MetaFileReader metaFileReader = new MetaFileReader(new UrlFormatter(_jobConfig));
            var metaFiles = await metaFileReader.LoadMetaFiles(element);
            FillFields(element, metaFiles);
            AddAssets(element, metaFiles);
            string autostartId = GetAutostartId(metaFiles);
            if( autostartId != null ) {
                try
                {
                    Element autostartElement = await CrawlElement(autostartId);
                    OverwriteValuesByAutostartElement(element, autostartElement);
                }
                catch
                {
                    _log.Warn(Util.GetFormattedResource("ElementCrawlerMessage01"));
                }
            }
            element.Hash = element.Fields.GetHashCode();
            SetDateValue(element);
            return element;
        }

        private void SetDateValue(Element element)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            string dateFieldName = $"{config.LongIdentifier}.{config.DateFieldName}";
            if ( !element.Fields.ContainsKey(dateFieldName))
            {
                element.AddValues(dateFieldName, Util.ConvertToUnixTime(DateTime.Now));
            }
        }

        public void FillInitialFields(Element element)
        {
            foreach (var mapping in _jobConfig.GlobalMappings)
            {
                element.AddValues(mapping.Key, mapping.Value);
            }
            switch (element.Class)
            {
                case Element.Project:
                    {
                        foreach (var mapping in _jobConfig.ProjectMappings)
                        {
                            element.AddValues(mapping.Key, mapping.Value);
                        }
                        break;
                    }
                case Element.Slide:
                    {
                        foreach (var mapping in _jobConfig.SlideMappings)
                        {
                            element.AddValues(mapping.Key, mapping.Value);
                        }
                        break;
                    }
                case Element.Group:
                    {
                        foreach (var mapping in _jobConfig.GroupMappings)
                        {
                            element.AddValues(mapping.Key, mapping.Value);
                        }
                        break;
                    }
            }
        }
        
        
        
        
        private void AddAssets(Element element, MetaFiles metaFiles)
        {
            MetaFileReader metaFileReader = new MetaFileReader(new UrlFormatter(_jobConfig));
            metaFileReader.ExtractAssets(metaFiles, out string[] childrenIds, out string[] attachementIds);
            element.ChildrenIds = childrenIds;
            element.AttachementNames = attachementIds;
        }



        /// <summary>
        /// Fügt die Werte einer Liste zu allen anderen Listen hinzu außer der Wert steht in einer Blacklist.
        /// </summary>
        /// <param name="values">Die Liste, die die Werte enthält, die in die anderen Listen übertragen werden sollen.</param>
        /// <param name="blacklist">Eine Verbotsliste, die Werte enthält, die nicht übertragen werden sollen.</param>
        /// <param name="lists">Eine beliebige Anzahl an Listen, die die Werte erhalten sollen.</param>
        private void AddValueToEachList(IEnumerable<string> values, IEnumerable<string> blacklist = null, params List<string>[] lists)
        {
            foreach (string field in values)
            {
                if (blacklist == null || blacklist.Contains(field))
                {
                    foreach (var list in lists)
                    {
                        list.Add(field);
                    }
                }
            }
        }




        /// <summary>
        /// Falls aktiviert, die Daten eines Elements durch die Daten eines anderen Elements (Autostart-Element) überschreiben oder ergänzen.
        /// </summary>
        /// <param name="element">Das Objekt, dessen Daten überschrieben werden sollen.</param>
        /// <param name="autostartElement">Das Objekt, das die Daten des anderen Objekts überschreibt.</param>
        private void OverwriteValuesByAutostartElement(Element element, Element autostartElement)
        {
            if (element.Class == Element.Slide) { return; }

            if (!_jobConfig.AutostartMetaMapping || _projectMappingFields == null || _groupMappingFields == null)
            {
                return;
            }

            bool overwriteValues = _jobConfig.AutostartChildOverwrite;
            string tmp = overwriteValues ? "" : "nicht";
            _log.Debug($"Die Werte des Element {element.Id} werden {tmp} von den Werten des Autostartelements {autostartElement.Id} überschrieben.");

            List<string> mappingFieldList = GetMappingListForClass(element);

            if (mappingFieldList == null) { return; }


            foreach (string field in mappingFieldList)
            {
                var autostartValues = autostartElement.GetValues(field);
                if (overwriteValues)
                {
                    element.ReplaceValues(field, autostartValues);
                }
                else
                {
                    element.AddValues(field, autostartValues);
                }
            }
        }




        private List<string> GetMappingListForClass(Element element)
        {
            List<string> mappingFieldList = null;
            if (element.Class == Element.Project)
            {
                mappingFieldList = _projectMappingFields;
            }
            else if (element.Class == Element.Group)
            {
                mappingFieldList = _groupMappingFields;
            }

            return mappingFieldList;
        }




        /// <summary>
        /// Analysiere die Metadateien, extrahiere die passenden Werte und fülle damit die Felder des Objekts.
        /// <para>Es wird durch alle Feldnamen, die indexiert werden sollen, iteriert. Den Feldern sind Listen zugeordnet. Sie enthalten
        /// statische Werte oder Variablen. Die Variablen werden durch die Inhalte in den Metadateien ersetzt.</para>
        /// </summary>
        /// <param name="element">Das Objekt, dessen Felder gefüllt werden sollen.</param>
        /// <param name="metaFiles">Die Metadateien mit dem Inhalt, zum Beispiel entity.txt, lesson.js und slide.js.</param>
        public void FillFields(Element element, MetaFiles metaFiles)
        {
            var keys = element.Fields.Keys;

            foreach (string fieldName in keys)
            {
                List<string> values = element.Fields[fieldName];
                int valuesCount = values.Count;
                for (int valueIndex = 0; valueIndex < valuesCount; valueIndex++)
                {
                    bool fieldWasRemoved = FillOrRemoveField(metaFiles, values, valueIndex);
                    if( fieldWasRemoved)
                    {
                        valueIndex--;
                        valuesCount--;
                    }
                }
            }
        }




        /// <summary>
        /// Ersetzt ein Feld durch einen Wert.
        /// <para>Wenn der vorbelegte Wert ein statischer Wert ist (keine Variable), dann wird nichts gemacht. Falls der Wert aber dem regulären Ausdruck für
        /// Variablen entspricht, wird es durch einen Wert in einer Metadatei ersetzt. Falls der ermittelt Wert null ist, wird dieser Eintrag aus der Liste
        /// entfernt.</para>
        /// </summary>
        /// <param name="metaFiles">Die Metadateien mit dem Inhalt, zum Beispiel entity.txt, lesson.js und slide.js.</param>
        /// <param name="values">Die Liste der Werte für dieses Feld.</param>
        /// <param name="valueIndex">Index des Wertes in der Liste, der befüllt werden soll.</param>
        private bool FillOrRemoveField(MetaFiles metaFiles, List<string> values, int valueIndex)
        {
            var match = Regex.Match(values[valueIndex], _variablePattern);
            bool wasRemoved = false;
            if (match.Success)
            {
                string variable = match.Value;
                string variableName = variable[1..];
                MetaFileReader metaFileReader = new MetaFileReader(new UrlFormatter(_jobConfig));
                var val = Util.RemoveMarkup(metaFileReader.ExtractValue(metaFiles, variable[0], variableName));
                if( val != null )
                {
                    values[valueIndex] = val;
                }
                else
                {
                    values.RemoveAt(valueIndex);
                    wasRemoved = true;
                }
            }
            return wasRemoved;
        }




        /// <summary>
        /// Die Autostart-ID eines Elements bestimmen.
        /// </summary>
        /// <param name="metaFiles"></param>
        /// <returns>Gibt entweder die ID des Autostart-Elements zurück oder null, falls kein Autostart-Element definiert ist.</returns>
        private string GetAutostartId(MetaFiles metaFiles)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            if (metaFiles.EntityFile?[config.AutostartIdentifier] == null)
            {
                return null;
            }

            return metaFiles.EntityFile[config.AutostartIdentifier]?.Value<string>().Split('!')[1];
        }




    }
}
