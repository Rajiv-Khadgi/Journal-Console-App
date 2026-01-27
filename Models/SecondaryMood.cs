using SQLite;

namespace JournalApps.Models
{
    public class SecondaryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; } // Foreign key to JournalEntry

        [Indexed]
        public int UserId { get; set; }

        public string MoodName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
