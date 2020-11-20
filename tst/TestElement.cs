using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.crawler;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.http;
using Enable_Now_Konnektor.src.jobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using static Enable_Now_Konnektor.src.misc.MetaFileReader;

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

        private static MetaFiles ReadFile(Element element)
        {
            string path = Path.Combine(@"C:\Users\Christian\Source\Repos\Enable Now Connector for iFinder5", "tst", "samples", element.Id + "_entity.txt");
            string jsonString = File.ReadAllText(path);
            MetaFiles files = new MetaFiles();
            files.EntityFile = JsonConvert.DeserializeObject<JObject>(jsonString);

            path = Path.Combine(@"C:\Users\Christian\Source\Repos\Enable Now Connector for iFinder5", "tst", "samples", element.Id + "_lesson.js");
            jsonString = File.ReadAllText(path);
            files.LessonFile = JsonConvert.DeserializeObject<JObject>(jsonString);
            return files;
        }
    }
}
