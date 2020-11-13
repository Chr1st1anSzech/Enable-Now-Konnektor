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
            return _entityUrlPattern.Replace("${Class}", className).Replace("${Id}", id).Replace("${File}", fileType);
        }

        public string GetDemoUrl(string id)
        {
            return _demoUrlPattern.Replace("${Id}", id);
        }

        public string GetContentUrl(string className, string id)
        {
            return _contentUrlPattern.Replace("${Class}", className).Replace("${Id}", id);
        }
    }
}
