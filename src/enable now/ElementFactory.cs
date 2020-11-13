using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Enable_Now_Konnektor.src.enable_now
{
    class ElementFactory
    {
        private readonly Config _connectorConfig;
        private readonly UrlFormatter _formatter;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ElementFactory(UrlFormatter formatter)
        {
            _connectorConfig = ConfigReader.LoadConnectorConfig();
            _formatter = formatter;
        }

        /// <summary>
        /// Erstellt ein neues Objekt, das einem Element in Enable Now entspricht. 
        /// <para>Die grundlegenen Werte ID, Klasse und URL werden vergeben.</para>
        /// </summary>
        /// <param name="id">Die ID es Enable Now Elements.</param>
        /// <exception cref="ArgumentException">Wirft eine Ausnahme, falls die ID ungültig ist.</exception>
        /// <returns></returns>
        public Element CreateENObject(string id)
        {
            Element obj = new Element(id);
            Initialize(obj);
            return obj;
        }

        private void Initialize(Element obj)
        {
            obj.AddValues(_connectorConfig.UidFieldName, obj.Id);
            obj.AddValues(_connectorConfig.ClassFieldName, obj.Class);
            obj.AddValues(_connectorConfig.UrlFieldName, _formatter.GetContentUrl(obj.Class, obj.Id));
        }
    }
}
