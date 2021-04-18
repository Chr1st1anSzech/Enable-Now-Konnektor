using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.jobs;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.indexing
{
    internal abstract class Indexer
    {
        protected JobConfig jobConfig;



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static Indexer GetIndexer()
        {
            switch (JobManager.GetJobManager().SelectedJobConfig.IndexerType.ToLower())
            {
                case "hessian":
                    {
                        return null;
                    }
                case "json":
                default:
                    return new JsonIndexer();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal abstract Task<bool> AddElementToIndexAsync(Element element);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal abstract Task<bool> RemoveElementFromIndexAsync(Element element);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal abstract Task<bool> RemoveElementFromIndexAsync(string id);
    }
}
