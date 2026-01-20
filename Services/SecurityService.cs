using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace JournalApps.Services
{
    public class SecurityService : INotifyPropertyChanged
    {
        private const string PinHashKey = "app_pin_hash";
        private const string PinSaltKey = "app_pin_salt";
        private const string IsLockedKey = "app_is_locked";

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isLocked = true;
        public bool IsLocked
        {
            get => _isLocked;
            private set
            {
                if (_isLocked != value)
                {
                    _isLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public SecurityService()
        {
            LoadLockState();
        }

        private void LoadLockState()
        {
            // Check if PIN is set
            var hasPin = Preferences.ContainsKey(PinHashKey);

            // If no PIN is set, app is unlocked by default
            if (!hasPin)
            {
                IsLocked = false;
                return;
            }

            // Load lock state
            IsLocked = Preferences.Get(IsLockedKey, true);
        }

        // Check if PIN is set
        public bool IsPinSet()
        {
            return Preferences.ContainsKey(PinHashKey);
        }

        // Set new PIN
        public async Task<bool> SetPinAsync(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
                return false;

            try
            {
                // Generate salt
                var salt = GenerateSalt();

                // Hash the PIN with salt
                var hash = HashPin(pin, salt);

                // Save to preferences
                Preferences.Set(PinHashKey, Convert.ToBase64String(hash));
                Preferences.Set(PinSaltKey, Convert.ToBase64String(salt));

                // Auto-unlock after setting PIN
                IsLocked = false;
                Preferences.Set(IsLockedKey, false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Verify PIN
        public async Task<bool> VerifyPinAsync(string pin)
        {
            if (!IsPinSet())
                return false;

            try
            {
                // Get stored hash and salt
                var storedHash = Convert.FromBase64String(Preferences.Get(PinHashKey, ""));
                var salt = Convert.FromBase64String(Preferences.Get(PinSaltKey, ""));

                // Hash the input PIN with the same salt
                var inputHash = HashPin(pin, salt);

                // Compare hashes
                if (inputHash.SequenceEqual(storedHash))
                {
                    // Correct PIN - unlock the app
                    IsLocked = false;
                    Preferences.Set(IsLockedKey, false);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Lock the app
        public void Lock()
        {
            IsLocked = true;
            Preferences.Set(IsLockedKey, true);
        }

        // Remove PIN (for testing/reset)
        public void RemovePin()
        {
            Preferences.Remove(PinHashKey);
            Preferences.Remove(PinSaltKey);
            Preferences.Remove(IsLockedKey);
            IsLocked = false;
        }

        // Change PIN
        public async Task<bool> ChangePinAsync(string currentPin, string newPin)
        {
            if (!await VerifyPinAsync(currentPin))
                return false;

            return await SetPinAsync(newPin);
        }

        // Helper methods for hashing
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] HashPin(string pin, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                // Combine PIN and salt
                var pinBytes = Encoding.UTF8.GetBytes(pin);
                var combined = new byte[pinBytes.Length + salt.Length];

                Buffer.BlockCopy(pinBytes, 0, combined, 0, pinBytes.Length);
                Buffer.BlockCopy(salt, 0, combined, pinBytes.Length, salt.Length);

                return sha256.ComputeHash(combined);
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}