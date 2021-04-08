using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.metadata;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace Enable_Now_Konnektor.tst
{
    class TestElement
    {
        public async static void TestCreateProject() 
        { 
            JobReader jobReader = new JobReader();
            var jobConfigs = jobReader.ReadAllJobConfigs();
            JobConfig jobConfig = jobConfigs[0];
            ElementCrawler crawler = new ElementCrawler(jobConfig);

            var element = new Element("PR_CD031F4334BD65BE");
            crawler.FillInitialFields(element);

            var files = ReadFile(element);
            crawler.FillFields(element, files);

            CrawlerIndexerInterface crawlerIndexerInterface = new CrawlerIndexerInterface(jobConfig);
            await crawlerIndexerInterface.SendToIndexerAsync(element);
        }

        private static MetaDataCollection ReadFile(Element element)
        {
            string path = Path.Combine(@"C:\Users\Christian\Source\Repos\Enable Now Connector for iFinder5", "tst", "samples", element.Id + "_entity.txt");
            string jsonString = File.ReadAllText(path);
            MetaDataCollection metaData = new MetaDataCollection();
            metaData.Entity = JsonConvert.DeserializeObject<JObject>(jsonString);

            path = Path.Combine(@"C:\Users\Christian\Source\Repos\Enable Now Connector for iFinder5", "tst", "samples", element.Id + "_lesson.js");
            jsonString = File.ReadAllText(path);
            metaData.Lesson = JsonConvert.DeserializeObject<JObject>(jsonString);
            return metaData;
        }
    }
}
