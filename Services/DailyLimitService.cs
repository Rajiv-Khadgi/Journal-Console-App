using JournalApps.Data;
using JournalApps.Models;
using System;
using System.Threading.Tasks;

namespace JournalApps.Services
{
    public class DailyLimitService
    {
        private readonly AppDatabase _db;

        public DailyLimitService(AppDatabase db)
        {
            _db = db;
        }

        public async Task<bool> CanCreateAsync()
        {
            var today = DateTime.Today;
            var entry = await _db.Connection.Table<JournalEntry>()
                .Where(e => e.EntryDate == today)
                .FirstOrDefaultAsync();
            return entry == null; // Can create only if no entry exists today
        }

        public async Task<bool> CanUpdateAsync()
        {
            var today = DateTime.Today;
            var count = await _db.Connection.Table<UpdateHistory>()
                .Where(u => u.UpdatedAt.Date == today)
                .CountAsync();
            return count == 0; // Only one update allowed per day
        }

        public async Task<bool> CanDeleteAsync()
        {
            var today = DateTime.Today;
            var count = await _db.Connection.Table<DeleteHistory>()
                .Where(d => d.DeletedAt.Date == today)
                .CountAsync();
            return count == 0; // Only one delete allowed per day
        }

        public Task MarkCreatedAsync()
        {
            // Creation is automatically limited by the entry table itself
            return Task.CompletedTask;
        }

        public async Task MarkUpdatedAsync(int entryId)
        {
            await _db.Connection.InsertAsync(new UpdateHistory
            {
                EntryId = entryId,
                UpdatedAt = DateTime.Now
            });
        }

        public async Task MarkDeletedAsync(int entryId)
        {
            await _db.Connection.InsertAsync(new DeleteHistory
            {
                EntryId = entryId,
                DeletedAt = DateTime.Now
            });
        }
    }
}
