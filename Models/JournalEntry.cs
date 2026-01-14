using SQLite;
using System;
using System.Collections.Generic;

namespace JournalApps.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string PrimaryMood { get; set; } = string.Empty;

        public DateTime EntryDate { get; set; } = DateTime.Today;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Related data
        [Ignore] // Important: SQLite won't try to map this to a table column
        public List<Tag> Tags { get; set; } = new List<Tag>();

        [Ignore]
        public List<SecondaryMood> SecondaryMoods { get; set; } = new List<SecondaryMood>();
    }
}
