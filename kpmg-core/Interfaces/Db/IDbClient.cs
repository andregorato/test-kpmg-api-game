using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace kpmg_core.Interfaces.Db
{
    public interface IDbClient<T>
    {
        IMongoCollection<T> GetCollection();
    }
}
