using Newtonsoft.Json.Linq;

namespace Enable_Now_Konnektor.src.metadata
{
    internal class MetaDataCollection
    {
        internal JObject Entity { get; set; }
        internal JObject Slide { get; set; }
        internal JObject Lesson { get; set; }
    }
}
