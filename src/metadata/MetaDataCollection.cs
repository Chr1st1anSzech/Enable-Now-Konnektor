using Newtonsoft.Json.Linq;

namespace Enable_Now_Konnektor.src.metadata
{
    class MetaDataCollection
    {
        public JObject Entity { get; set; }
        public JObject Slide { get; set; }
        public JObject Lesson { get; set; }
    }
}
