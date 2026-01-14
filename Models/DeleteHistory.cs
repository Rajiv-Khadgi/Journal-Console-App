using SQLite;
using System;

namespace JournalApps.Models
{
    public class DeleteHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int EntryId { get; set; } // Foreign key to JournalEntry
        public DateTime DeletedAt { get; set; }
    }
}
