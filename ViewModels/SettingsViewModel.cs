using CommunityToolkit.Mvvm.Messaging;
using RestaurantApp.Models;

namespace RestaurantApp.ViewModels
{
    public class SettingsViewModel
    {
        private const string NameKey = "name";
        private bool _isInitialized;

        public async ValueTask InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            var name = Preferences.Default.Get<string?>(NameKey, null);
            if (name is null)
            {
                do
                {
                    name = await Shell.Current.DisplayPromptAsync("Your name", "Enter your name");

                } while (string.IsNullOrWhiteSpace(name));
                Preferences.Default.Set<string>(NameKey, name);
            }
            WeakReferenceMessenger.Default.Send(NameChangedMessage.From(name));
            
        }
    }
}