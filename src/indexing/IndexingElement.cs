using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.src.indexing
{
    public class IndexingElement
    {
        public string id { get; set; }
        public Dictionary<string, List<string>> fields { get; set; }
    }
}
