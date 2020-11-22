using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    class MetaFileReader : MetaReader
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        public MetaFileReader(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        public override async Task<JObject> GetMetaData(Element element, string fileType)
        {
            string entityPath = GetMetaUrl(element.Class, element.Id, fileType);
            if( !File.Exists(entityPath) ) { return null; }

            string jsonString = await File.ReadAllTextAsync(entityPath);
            if ( string.IsNullOrWhiteSpace(jsonString) ) { return null; }

            try
            {
                return JsonConvert.DeserializeObject<JObject>(jsonString);
            }
            catch
            {
                log.Warn(Util.GetFormattedResource("MetaFileReaderMessage01", element.Id, fileType));
                return null;
            }
        }

        public override string GetMetaUrl(string className, string id, string fileType)
        {
            log.Debug(Util.GetFormattedResource("FileMetaAccessMessage01"));
            return jobConfig.EntityPath.Replace("${Class}", classNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        public override string GetContentUrl(string className, string id)
        {
            log.Debug(Util.GetFormattedResource("FileMetaAccessMessage02"));
            return jobConfig.ContentPath.Replace("${Class}", classNames[className]).Replace("${Id}", id);
        }
    }
}
