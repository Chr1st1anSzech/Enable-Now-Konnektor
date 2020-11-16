using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.jobs;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace Enable_Now_Konnektor.src.http
{
    class UrlFormatter
    {
        public static string EntityFile = "entity.txt";
        public static string LessonFile = "lesson.js";
        public static string SlideFile = "slide.js";

        private readonly Dictionary<string, string> classNames = new Dictionary<string, string>() {
            { "GR", "group" },
            { "SL", "slide" },
            { "PR", "project" },
            { "M", "media" }
        };

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _entityUrlPattern;
        private readonly string _demoUrlPattern;
        private readonly string _contentUrlPattern;


        public UrlFormatter(JobConfig jobConfig)
        {
            _entityUrlPattern = jobConfig.EntityUrl;
            _contentUrlPattern = jobConfig.ContentUrl;
            _demoUrlPattern = jobConfig.DemoUrl;
        }

        public string GetEntityUrl(string className, string id, string fileType)
        {
            _log.Debug($"Erstelle ContentUrl mit Parametern className='{className}', id='{id}', fileType='{fileType}'");
            return _entityUrlPattern.Replace("${Class}", classNames[className]).Replace("${Id}", id).Replace("${File}", fileType);
        }

        public string GetDemoUrl(string id)
        {
            _log.Debug($"Erstelle ContentUrl mit Parametern id='{id}'");
            return _demoUrlPattern.Replace("${Id}", id);
        }

        public string GetContentUrl(string className, string id)
        {
            _log.Debug($"Erstelle ContentUrl mit Parametern className='{className}', id='{id}'");
            return _contentUrlPattern.Replace("${Class}", classNames[className]).Replace("${Id}", id);
        }
    }
}
