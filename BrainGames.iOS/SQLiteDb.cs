using System;
using System.IO;
using SQLite;
using Xamarin.Forms;
using BrainGames.iOS;
using Foundation;

[assembly: Dependency(typeof(SQLiteDb))]

namespace BrainGames.iOS
{
    public class SQLiteDb : ISQLiteDb
    {
        public SQLiteConnection GetConnection()
        {
            var fileName = "braingames.db3";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libraryPath = Path.Combine(documentsPath, "..", "Library");
            var path = Path.Combine(libraryPath, fileName);
            var connection = new SQLite.SQLiteConnection(path);
            return connection;
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            var fileName = "braingames.db3";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libraryPath = Path.Combine(documentsPath, "..", "Library");
            var path = Path.Combine(libraryPath, fileName);
            var connection = new SQLite.SQLiteAsyncConnection(path);
            return connection;
        }
    }
}