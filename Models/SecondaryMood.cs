using SQLite;

namespace JournalApps.Models
{
    public class SecondaryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; } // Foreign key to JournalEntry
        public string MoodName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
