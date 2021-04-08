using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.indexing
{
    internal abstract class Indexer
    {
        protected JobConfig jobConfig;

        internal static Indexer GetIndexer(JobConfig jobConfig)
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

        internal abstract Task<bool> AddElementToIndexAsync(Element element);

        internal abstract Task<bool> RemoveElementFromIndexAsync(Element element);

        internal abstract Task<bool> RemoveElementFromIndexAsync(string id);
    }
}
