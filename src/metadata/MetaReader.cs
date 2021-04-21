using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.metadata
{
    internal abstract class MetaReader
    {
        internal static readonly string s_entityFile = "entity.txt";
        internal static readonly string s_lessonFile = "lesson.js";
        internal static readonly string s_slideFile = "slide.js";

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
            switch (JobManager.GetJobManager().SelectedJobConfig.PublicationSource.ToLower())
            {
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

        internal abstract Task<JObject> GetMetaDataAsync(Element element, string fileType);

        internal abstract string GetMetaUrl(string className, string id, string fileType);

        internal abstract string GetContentUrl(string className, string id);
    }
}
