using JournalApps.Data;
using JournalApps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JournalApps.Services
{
    public class JournalService
    {
        private readonly AppDatabase _db;
        private readonly DailyLimitService _limit;
        private readonly UserService _userService;

        public JournalService(AppDatabase db, DailyLimitService limit, UserService userService)
        {
            _db = db;
            _limit = limit;
            _userService = userService;
        }

        private int CurrentUserId => _userService.CurrentUser?.Id ?? 0;

        // Fetch all entries
        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            if (!_userService.IsAuthenticated) return new List<JournalEntry>();

            return await _db.Connection.Table<JournalEntry>()
                .Where(j => j.UserId == CurrentUserId && !j.IsDeleted)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync();
        }

        // Fetch single entry
        public async Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            if (!_userService.IsAuthenticated) return null;

            return await _db.Connection.Table<JournalEntry>()
                .Where(j => j.Id == id && j.UserId == CurrentUserId && !j.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // Fetch tags for an entry
        public async Task<List<Tag>> GetTagsByEntryIdAsync(int entryId)
        {
            return await _db.Connection.Table<Tag>()
                .Where(t => t.JournalEntryId == entryId)
                .ToListAsync();
        }

        // Fetch secondary moods for an entry
        public async Task<List<SecondaryMood>> GetSecondaryMoodsByEntryIdAsync(int entryId)
        {
            return await _db.Connection.Table<SecondaryMood>()
                .Where(m => m.JournalEntryId == entryId)
                .ToListAsync();
        }

        // Fetch entries with related data
        public async Task<List<JournalEntry>> GetAllEntriesWithDetailsAsync()
        {
            var entries = await GetAllEntriesAsync();

            foreach (var entry in entries)
            {
                entry.Tags = await GetTagsByEntryIdAsync(entry.Id);
                entry.SecondaryMoods = await GetSecondaryMoodsByEntryIdAsync(entry.Id);
            }

            return entries;
        }

        public async Task<JournalEntry?> GetEntryWithDetailsAsync(int id)
        {
            var entry = await GetEntryByIdAsync(id);
            if (entry != null)
            {
                entry.Tags = await GetTagsByEntryIdAsync(id);
                entry.SecondaryMoods = await GetSecondaryMoodsByEntryIdAsync(id);
            }
            return entry;
        }

        // Create entry
        public async Task<bool> CreateEntryAsync(JournalEntry entry, List<Tag> tags, List<SecondaryMood> moods, bool bypassLimits = false)
        {
            if (!_userService.IsAuthenticated) return false;

            if (!bypassLimits && !await _limit.CanCreateAsync())
                return false;

            entry.UserId = CurrentUserId;
            entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;

            // Insert the entry
            await _db.Connection.InsertAsync(entry);

            // Insert tags
            foreach (var tag in tags)
            {
                tag.JournalEntryId = entry.Id;
                tag.UserId = CurrentUserId;
                await _db.Connection.InsertAsync(tag);
            }

            // Insert secondary moods
            foreach (var mood in moods)
            {
                mood.JournalEntryId = entry.Id;
                mood.UserId = CurrentUserId;
                await _db.Connection.InsertAsync(mood);
            }

            // Record the creation in history ONLY if not bypassing limits (assuming real user action)
            // OR we can decide to always log it. 
            // If we bypass limits for seeding, we usually don't want to block the user from creating a real entry today.
            if (!bypassLimits)
            {
                await _limit.MarkCreatedAsync(entry.Id);
            }
            
            return true;
        }

        // Update entry
        public async Task<bool> UpdateEntryAsync(JournalEntry entry, List<Tag> tags, List<SecondaryMood> moods, bool bypassLimits = false)
        {
            if (!_userService.IsAuthenticated) return false;
            // Verify ownership
            if (entry.UserId != CurrentUserId) return false;

            if (!bypassLimits && !await _limit.CanUpdateAsync())
                return false;

            entry.UpdatedAt = DateTime.Now;
            await _db.Connection.UpdateAsync(entry);

            // Remove old tags/moods
            await _db.Connection.Table<Tag>().Where(t => t.JournalEntryId == entry.Id).DeleteAsync();
            await _db.Connection.Table<SecondaryMood>().Where(m => m.JournalEntryId == entry.Id).DeleteAsync();

            // Insert new tags/moods
            foreach (var tag in tags)
            {
                tag.JournalEntryId = entry.Id;
                tag.UserId = CurrentUserId;
                await _db.Connection.InsertAsync(tag);
            }

            foreach (var mood in moods)
            {
                mood.JournalEntryId = entry.Id;
                mood.UserId = CurrentUserId;
                await _db.Connection.InsertAsync(mood);
            }

            if (!bypassLimits)
            {
                await _limit.MarkUpdatedAsync(entry.Id);
            }
            return true;
        }

        // Delete entry
        public async Task<bool> DeleteEntryAsync(JournalEntry entry, bool bypassLimits = false)
        {
            if (!_userService.IsAuthenticated) return false;
            if (entry.UserId != CurrentUserId) return false;

            if (!bypassLimits && !await _limit.CanDeleteAsync())
                return false;

            entry.IsDeleted = true;
            entry.UpdatedAt = DateTime.Now;

            await _db.Connection.UpdateAsync(entry);

            if (!bypassLimits)
            {
                await _limit.MarkDeletedAsync(entry.Id);
            }
            return true;
        }
    }
}
