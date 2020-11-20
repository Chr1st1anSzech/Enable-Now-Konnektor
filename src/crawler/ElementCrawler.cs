using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.extension;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;
        private readonly MetaFileReader metaFileReader;

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
            this.jobConfig = jobConfig;
            metaFileReader = new MetaFileReader(jobConfig);
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
            if (!jobConfig.AutostartMetaMapping || !jobConfig.AutostartChildOverwrite)
            {
                return;
            }

            _projectMappingFields = new List<string>();
            _groupMappingFields = new List<string>();

            // alle globalen Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(jobConfig.GlobalMappings.Keys, jobConfig.AutoStartMappingBlacklist, _projectMappingFields, _groupMappingFields);

            // alle Projekt-Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(jobConfig.ProjectMappings.Keys, jobConfig.AutoStartMappingBlacklist, _projectMappingFields);

            // alle Gruppen-Felder hinzufügen außer sie stehen auf der Blacklist
            AddValueToEachList(jobConfig.GroupMappings.Keys, jobConfig.AutoStartMappingBlacklist, _groupMappingFields);
        }




        /// <summary>
        /// Die Daten eines Elements analysieren und daraus ein Objekt erstellen.
        /// </summary>
        /// <param name="id">Die ID des Element, zum Beispiel GR_389F860B088563B1.</param>
        /// <returns>Ein Objekt, das die Daten des Elements in Enable Now enthält</returns>
        public async Task<Element> CrawlElement(string id)
        {
            log.Debug($"Crawle das Objekt mit der ID '{id}'.");
            Element element = new Element(id);
            FillInitialFields(element);
            MetaFiles metaFiles = await metaFileReader.LoadMetaFiles(element);
            FillFields(element, metaFiles);
            AddAssets(element, metaFiles);
            string autostartId = GetAutostartId(metaFiles);
            if (autostartId != null)
            {
                try
                {
                    Element autostartElement = await CrawlElement(autostartId);
                    OverwriteValuesByAutostartElement(element, autostartElement);
                }
                catch
                {
                    log.Warn(Util.GetFormattedResource("ElementCrawlerMessage01"));
                }
            }
            element.Hash = element.GenerateHashCode();
            SetDateValue(element);
            return element;
        }

        private void SetDateValue(Element element)
        {
            Config config = ConfigReader.LoadConnectorConfig();
            string dateFieldName = $"{config.LongIdentifier}.{config.DateFieldName}";
            if (!element.Fields.ContainsKey(dateFieldName))
            {
                element.AddValues(dateFieldName, Util.ConvertToUnixTime(DateTime.Now));
            }
        }

        public void FillInitialFields(Element element)
        {
            Config cfg = ConfigReader.LoadConnectorConfig();
            element.AddValues(cfg.UidFieldName, element.Id);
            element.AddValues(cfg.ClassFieldName, element.Class);
            element.AddValues(cfg.UrlFieldName, new access.HttpMetaAccess(jobConfig).GetContentUrl(element.Class, element.Id));

            foreach (var mapping in jobConfig.GlobalMappings)
            {
                element.AddValues(mapping.Key, mapping.Value);
            }

            Dictionary<string, Dictionary<string, string[]>> mappings = new Dictionary<string, Dictionary<string, string[]>>()
            {
                { Element.Project, jobConfig.ProjectMappings },
                { Element.Slide, jobConfig.SlideMappings},
                { Element.Group, jobConfig.GroupMappings }

            };

            if( !mappings.ContainsKey(element.Class) ) { return; }

            foreach (var mapping in mappings[element.Class])
            {
                element.AddValues(mapping.Key, mapping.Value);
            }
        }




        private void AddAssets(Element element, MetaFiles metaFiles)
        {
            MetaFileReader metaFileReader = new MetaFileReader(jobConfig);
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

            if (!jobConfig.AutostartMetaMapping || _projectMappingFields == null || _groupMappingFields == null) { return; }

            List<string> mappingFieldList = GetMappingListForClass(element);
            if (mappingFieldList == null) { return; }

            bool overwriteValues = jobConfig.AutostartChildOverwrite;
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
            var keys = element.Fields.Keys.ToList();
            ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator();
            foreach (string fieldName in keys)
            {
                List<string> values = element.Fields[fieldName];
                int lastIndex = values.Count - 1;
                for (int valueIndex = lastIndex; valueIndex >= 0; valueIndex--)
                {
                    string temporaryValue = values[valueIndex];
                    string[] resultValues = EvaluateField(temporaryValue, expressionEvaluator, metaFiles);
                    AddOrRemoveFields(values, valueIndex, resultValues);
                }

                if (values.Count == 0)
                {
                    element.Fields.Remove(fieldName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temporaryValue"></param>
        /// <param name="expressionEvaluator"></param>
        /// <param name="metaFiles"></param>
        /// <returns>Eine Liste mit den Werten oder null, wenn der Ausdruck kein Ergebnis liefert.</returns>
        private string[] EvaluateField(string temporaryValue, ExpressionEvaluator expressionEvaluator, MetaFiles metaFiles)
        {
            if (!expressionEvaluator.IsExpression(temporaryValue, out string expression))
            {
                log.Debug(Util.GetFormattedResource("ElementCrawlerMessage02", temporaryValue));
                return string.IsNullOrWhiteSpace(temporaryValue) ? null : new string[] { temporaryValue };
            }

            if (expressionEvaluator.IsVariableExpression(expression, out string variableName))
            {
                string value = Util.RemoveMarkup(metaFileReader.ExtractValue(metaFiles, variableName));
                log.Debug(Util.GetFormattedResource("ElementCrawlerMessage03", expression, value));
                return string.IsNullOrWhiteSpace(value) ? null : new string[] { value };
            }
            if (expressionEvaluator.IsConverterExpression(expression, out string converterClassName, out string converterVariableName))
            {
                string value = Util.RemoveMarkup(metaFileReader.ExtractValue(metaFiles, converterVariableName));
                log.Debug(Util.GetFormattedResource("ElementCrawlerMessage04", expression, converterClassName, converterVariableName));
                return expressionEvaluator.EvaluateAsConverter(value, converterClassName);
            }
            return null;
        }

        /// <summary>
        /// Fügt die Werte aus der Ergebnisliste der Werteliste des Feldes hinzu.
        /// <para>Falls die Ergebnisliste leer ist, wird der bisherige Feldwert entfernt.</para>
        /// <para>Ansonsten wird der Wert ersetzt und alle weiteren Werte hinzugefügt.</para>
        /// </summary>
        /// <param name="values"></param>
        /// <param name="valueIndex"></param>
        /// <param name="resultValues"></param>
        private void AddOrRemoveFields(List<string> values, int valueIndex, string[] resultValues)
        {
            if (resultValues == null || resultValues.Length == 0)
            {
                values.RemoveAt(valueIndex);
                return;
            }

            if (resultValues[0] == null)
            {
                values.RemoveAt(valueIndex);
            }
            else
            {
                values[valueIndex] = resultValues[0];
            }

            int length = resultValues.Length;
            for (int i = 1; i < length; i++)
            {
                if (resultValues[i] != null)
                {
                    values.Add(resultValues[i]);
                }

            }
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
