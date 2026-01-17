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

        // CREATE: Can only create ONCE per day (even if deleted)
        public async Task<bool> CanCreateAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Check if already created ANY entry today (tracked separately)
            var createdToday = await _db.Connection.Table<CreateHistory>()
                .Where(c => c.CreatedAt >= today && c.CreatedAt < tomorrow)
                .FirstOrDefaultAsync();

            return createdToday == null; // Can create if no create history today
        }

        // EDIT: Can only edit ONCE per day
        public async Task<bool> CanUpdateAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var count = await _db.Connection.Table<UpdateHistory>()
                .Where(u => u.UpdatedAt >= today && u.UpdatedAt < tomorrow)
                .CountAsync();

            return count == 0;
        }

        // DELETE: Can only delete ONCE per day
        public async Task<bool> CanDeleteAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var count = await _db.Connection.Table<DeleteHistory>()
                .Where(d => d.DeletedAt >= today && d.DeletedAt < tomorrow)
                .CountAsync();

            return count == 0;
        }

        // MARK CREATED: Called when entry is created
        public async Task MarkCreatedAsync(int entryId)  // CHANGED: Takes entryId
        {
            await _db.Connection.InsertAsync(new CreateHistory
            {
                EntryId = entryId,
                CreatedAt = DateTime.Now
            });
        }

        // MARK UPDATED: Called when entry is updated
        public async Task MarkUpdatedAsync(int entryId)
        {
            await _db.Connection.InsertAsync(new UpdateHistory
            {
                EntryId = entryId,
                UpdatedAt = DateTime.Now
            });
        }

        // MARK DELETED: Called when entry is deleted
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