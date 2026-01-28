using SQLite;

namespace JournalApps.Models
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; } // Foreign key to JournalEntry
        
        [Indexed]
        public int UserId { get; set; }
        
        public string TagName { get; set; } = string.Empty;
    }
}
