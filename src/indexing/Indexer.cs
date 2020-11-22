using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enable_Now_Konnektor.src.indexing
{
    abstract class Indexer
    {
        protected JobConfig _jobConfig;
        protected Config _config;

        public abstract Task<bool> AddElementToIndex(Element element);

        public abstract bool RemoveElementFromIndex(Element element);

        public abstract bool RemoveElementFromIndex(string id);
    }
}
