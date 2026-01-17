using SQLite;
using System;

namespace JournalApps.Models
{
    public class CreateHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int EntryId { get; set; } // Foreign key to JournalEntry
        public DateTime CreatedAt { get; set; }
    }
}