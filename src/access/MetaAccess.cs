using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.access
{
    abstract class MetaAccess
    {
        public static string EntityFile = "entity.txt";
        public static string LessonFile = "lesson.js";
        public static string SlideFile = "slide.js";

        protected readonly Dictionary<string, string> classNames = new Dictionary<string, string>() {
            { "GR", "group" },
            { "SL", "slide" },
            { "PR", "project" },
            { "M", "media" }
        };

        public static MetaAccess GetMetaAccess(JobConfig jobConfig)
        {
            switch (jobConfig.PublicationSource) {
                case "File":
                    {
                        return new FileMetaAccess(jobConfig);
                    }
                case "Http":
                default:
                    {
                        return new HttpMetaAccess(jobConfig);
                    }
            }
        }

        public abstract Task<JObject> GetMetaData(Element element, string fileType);

        public abstract string GetMetaUrl(string className, string id, string fileType);

        public abstract string GetContentUrl(string className, string id);
    }
}
