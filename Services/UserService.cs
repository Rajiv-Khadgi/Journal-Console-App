using JournalApps.Data;
using JournalApps.Models;
using System.Security.Cryptography;
using System.Text;

namespace JournalApps.Services
{
    public class UserService
    {
        private readonly AppDatabase _db;
        public User? CurrentUser { get; private set; }

        public bool IsAuthenticated => CurrentUser != null;

        public event Action? OnAuthStateChanged;

        public UserService(AppDatabase db)
        {
            _db = db;
        }

        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            // Check if user exists
            var existingUser = await _db.Connection.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return false;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.Now
            };

            await _db.Connection.InsertAsync(user);
            return true;
        }

        public async Task<bool> LoginUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _db.Connection.Table<User>()
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            if (VerifyPassword(password, user.PasswordHash))
            {
                CurrentUser = user;
                NotifyAuthStateChanged();
                return true;
            }

            return false;
        }

        public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            if (CurrentUser == null) return false;

            // Verify old password
            if (!VerifyPassword(oldPassword, CurrentUser.PasswordHash))
                return false;

            // Update password
            CurrentUser.PasswordHash = HashPassword(newPassword);
            await _db.Connection.UpdateAsync(CurrentUser);
            return true;
        }

        public async Task<bool> DeleteAccountAsync(string password)
        {
            if (CurrentUser == null) return false;

            // Verify password
            if (!VerifyPassword(password, CurrentUser.PasswordHash))
                return false;

            // Delete all user data
            // 1. Delete Journal Entries
            var entries = await _db.Connection.Table<JournalEntry>()
                .Where(e => e.UserId == CurrentUser.Id)
                .ToListAsync();
                
            foreach (var entry in entries)
            {
                // Delete associated tags and moods
                await _db.Connection.Table<Models.Tag>()
                    .Where(t => t.JournalEntryId == entry.Id)
                    .DeleteAsync();

                await _db.Connection.Table<SecondaryMood>()
                    .Where(m => m.JournalEntryId == entry.Id)
                    .DeleteAsync();
            }

            await _db.Connection.Table<JournalEntry>()
                .Where(e => e.UserId == CurrentUser.Id)
                .DeleteAsync();

            // 2. Delete User
            await _db.Connection.DeleteAsync(CurrentUser);
            
            Logout();
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            NotifyAuthStateChanged();
        }

        private string HashPassword(string password)
        {
            // Simple SHA256 for demonstration. In production, use BCrypt or PBKDF2 with salt.
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var inputHash = HashPassword(inputPassword);
            return inputHash == storedHash;
        }

        private void NotifyAuthStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }
    }
}
