using System;
using System.IO;
using SQLite;
using BrainGames.Android;
using Android.App;

[assembly: Xamarin.Forms.Dependency(typeof(SQLiteDb))]

namespace BrainGames.Android
{
    public class SQLiteDb : ISQLiteDb
    {
        public SQLiteConnection GetConnection()
        {
            var fileName = "braingames.db3";
            var documentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentPath, fileName);
            var connection = new SQLiteConnection(path);
            return connection;
        }
        public SQLiteAsyncConnection GetAsyncConnection()
        {
            var fileName = "braingames.db3";
            var documentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentPath, fileName);
            var connection = new SQLiteAsyncConnection(path);
            return connection;
        }
    }
}