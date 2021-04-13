using System;
using System.Collections.Generic;
using System.Text;

namespace kpmg_worker
{
    public class MongoConfig
    {
        public string Endpoint { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
