
using JournalApps.Models; 
using SQLite;
using System.IO;
using System.Threading.Tasks;


namespace JournalApps.Data
{
    public class AppDatabase
    {
        
        public SQLiteAsyncConnection Connection { get; private set; }

        public AppDatabase()
        {
            // Create database in app's local storage
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        //System.Diagnostics.Debug.WriteLine($"SQLite database path: {dbPath}");
        //C: \Users\rajiv\AppData\Local\User Name\com.companyname.journalapps\Data\journal.db

        Connection = new SQLiteAsyncConnection(dbPath);
        }


        // Initialize database tables
        public async Task InitAsync()
        {
            await Connection.CreateTableAsync<JournalEntry>();
            await Connection.CreateTableAsync<Tag>();
            await Connection.CreateTableAsync<SecondaryMood>();
            await Connection.CreateTableAsync<UpdateHistory>();
            await Connection.CreateTableAsync<DeleteHistory>();
            await Connection.CreateTableAsync<CreateHistory>();
            
            // User table for authentication
            await Connection.CreateTableAsync<User>();
        }
    }
}
