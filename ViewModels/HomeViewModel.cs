using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantApp.Data;

namespace RestaurantApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        [ObservableProperty]
        public MenuCategory[] _categories;
        [ObservableProperty]
        private bool _isLoaded;

        public HomeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        private bool _isStarted;
        public async ValueTask InitializeAsync()
        {
            if (_isStarted)
            { return; }
            _isStarted = true;
            IsLoaded = true;
            Categories = await _databaseService.GetMenuCategoriesAsync();
            IsLoaded = false;
        }
    }
}