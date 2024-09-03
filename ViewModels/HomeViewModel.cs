using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantApp.Data;
using RestaurantApp.Models;
using MenuItem = RestaurantApp.Data.MenuItem;
namespace RestaurantApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        [ObservableProperty]
        private MenuCategoryModel[] _categories = [];
        [ObservableProperty]
        private MenuItem[] _menuItems = [];
        [ObservableProperty]
        private MenuCategoryModel? _selectedCategory = null;
        [ObservableProperty]
        private bool _isLoading;

        public HomeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        private bool _isInitialized;
        public async ValueTask InitializeAsync()
        {
            if (_isInitialized)
            { return; }
            _isInitialized = true;
            IsLoading = true;
            Categories = (await _databaseService.GetMenuCategoriesAsync()).Select(MenuCategoryModel.FromEntity).ToArray();
            Categories[0].IsSelected = true;
            SelectedCategory = Categories[0];
            MenuItems = await _databaseService.GetMenuItemsByCategoryAsync(SelectedCategory.Id);
            IsLoading = false;
        }

        [RelayCommand]
        private async Task SelectCategoryAsync(int categoryId)
        {
            if (SelectedCategory.Id == categoryId)
                return;
            IsLoading = true;
            var existingSelectedCategory = Categories.First(c => c.IsSelected);
            existingSelectedCategory.IsSelected = false;
            var newSelectedCategory = Categories.First(c => c.Id == categoryId);
            newSelectedCategory.IsSelected = true;

            SelectedCategory = newSelectedCategory;
            MenuItems = await _databaseService.GetMenuItemsByCategoryAsync(SelectedCategory.Id);
            IsLoading = false;
        }
    }
}