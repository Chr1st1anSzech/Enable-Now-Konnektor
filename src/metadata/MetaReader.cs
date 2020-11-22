using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    internal abstract class MetaReader
    {
        internal static string EntityFile = "entity.txt";
        internal static string LessonFile = "lesson.js";
        internal static string SlideFile = "slide.js";

        protected readonly Dictionary<string, string> classNames = new Dictionary<string, string>() {
            { "GR", "group" },
            { "SL", "slide" },
            { "PR", "project" },
            { "M", "media" }
        };

        internal static MetaReader GetMetaAccess(JobConfig jobConfig)
        {
            switch (jobConfig.PublicationSource.ToLower()) {
                case "file":
                    {
                        return new MetaFileReader(jobConfig);
                    }
                case "website":
                default:
                    {
                        return new MetaWebsiteReader(jobConfig);
                    }
            }
        }

        internal abstract Task<JObject> GetMetaData(Element element, string fileType);

        internal abstract string GetMetaUrl(string className, string id, string fileType);

        internal abstract string GetContentUrl(string className, string id);
    }
}
