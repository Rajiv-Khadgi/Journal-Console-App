using Microsoft.JSInterop;

namespace JournalApps.Services
{
    public class ThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isDarkMode;

        public bool IsDarkMode => _isDarkMode;

        public event Action? OnThemeChanged;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            // Simple initialization, could load from Preferences here
            // For now, default to System or Light
            await ApplyThemeAsync();
        }

        public async Task SetThemeAsync(bool isDark)
        {
            _isDarkMode = isDark;
            await ApplyThemeAsync();
            OnThemeChanged?.Invoke();
        }

        private async Task ApplyThemeAsync()
        {
            if (_isDarkMode)
            {
                await _jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", "dark");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("document.documentElement.removeAttribute", "data-theme");
            }
        }
    }
}
