using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    internal abstract class MetaReader
    {
        internal static string EntityFile = "entity.txt";
        internal static string LessonFile = "lesson.js";
        internal static string SlideFile = "slide.js";

        protected Dictionary<string, string> ClassNames { get; } = new Dictionary<string, string>() {
            { Element.Group, "group" },
            { Element.Slide, "slide" },
            { Element.Project, "project" },
            { Element.Media, "media" },
            { Element.Book, "book" },
            { Element.Text, "cdoc" }
        };

        internal static MetaReader GetMetaReader()
        {
            switch (JobManager.GetJobManager().SelectedJobConfig.PublicationSource.ToLower()) {
                case "datei":
                    {
                        return new MetaFileReader();
                    }
                case "webseite":
                default:
                    {
                        return new MetaWebsiteReader();
                    }
            }
        }

        internal abstract Task<JObject> GetMetaData(Element element, string fileType);

        internal abstract string GetMetaUrl(string className, string id, string fileType);

        internal abstract string GetContentUrl(string className, string id);
    }
}
