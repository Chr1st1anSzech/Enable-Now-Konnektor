using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.indexing
{
    abstract class Indexer
    {
        protected JobConfig jobConfig;

        public static Indexer GetIndexer(JobConfig jobConfig)
        {
            switch (jobConfig.IndexerType.ToLower())
            {
                case "hessian":
                    {
                        return null;
                    }
                case "json":
                default:
                    return new JsonIndexer(jobConfig);
            }
        }

        public abstract Task<bool> AddElementToIndexAsync(Element element);

        public abstract Task<bool> RemoveElementFromIndexAsync(Element element);

        public abstract Task<bool> RemoveElementFromIndexAsync(string id);
    }
}
