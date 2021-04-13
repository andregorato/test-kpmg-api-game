using System;
using System.Collections.Generic;
using System.Text;

namespace kpmg_worker
{
    public class RedisConfig
    {
        public string Endpoint { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
    }
}
