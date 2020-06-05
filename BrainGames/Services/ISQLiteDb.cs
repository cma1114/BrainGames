using SQLite;

namespace BrainGames
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetAsyncConnection();
        SQLiteConnection GetConnection();
    }
}
