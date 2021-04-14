using System.Collections.Generic;

namespace kpmg_core.Interfaces.Repository
{
    public interface IGameRepository<T>
    {
        void AddGame(T game);
        T FindGame(string key);
        long UpdateGame(string key, T game);
        List<T> GetRanking();
    }
}
