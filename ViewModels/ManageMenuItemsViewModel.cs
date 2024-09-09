using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantApp.Data;
using RestaurantApp.Models;
using MenuItem = RestaurantApp.Data.MenuItem;
namespace RestaurantApp.ViewModels
{
    public partial class ManageMenuItemsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ManageMenuItemsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [ObservableProperty]
        private MenuCategoryModel[] _categories = [];
        [ObservableProperty]
        private MenuItem[] _menuItems = [];
        [ObservableProperty]
        private MenuCategoryModel? _selectedCategory = null;

        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty]
        private MenuItemModel _menuItem = new();
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
            SetEmptyCategoriesToItem();
            IsLoading = false;

        }

        private void SetEmptyCategoriesToItem()
        {
            MenuItem.Categories.Clear();
            foreach (var category in Categories)
            {
                var categoryOfItem = new MenuCategoryModel
                {
                    Id = category.Id,
                    Icon = category.Icon,
                    Name = category.Name,
                    IsSelected = false
                };
                MenuItem.Categories.Add(categoryOfItem);
            }
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

        [RelayCommand]
        private async Task EditMenuItemAsync(MenuItem menuItem)
        {
            // await Shell.Current.DisplayAlert("Edit","Edit menu item","ok");
            var menuItemModel = new MenuItemModel
            {
                Description = menuItem.Description,
                Icon = menuItem.Icon,
                Name = menuItem.Name,
                Id = menuItem.Id,
                Price = menuItem.Price
            };
            var itemCategories = await _databaseService.GetCategoriesOfMenuItem(menuItem.Id);
            foreach (var category in Categories)
            {
                var categoryOfItem = new MenuCategoryModel
                {
                    Icon = category.Icon,
                    Id = category.Id,
                    Name = category.Name
                };
                if (itemCategories.Any(c => c.Id == category.Id))
                    categoryOfItem.IsSelected = true;
                else
                    categoryOfItem.IsSelected = false;
                menuItemModel.Categories.Add(categoryOfItem);
            }
            MenuItem = menuItemModel;

        }
        [RelayCommand]
        private void Cancel()
        {
            MenuItem = new();

            SetEmptyCategoriesToItem();
        }
        [RelayCommand]
        private async Task SaveItemMenuAsync(MenuItemModel model)
        {
            IsLoading = true;
            var errorMessage = await _databaseService.SaveMenuItemAsync(model);
            if (errorMessage != null)
            {
                await Shell.Current.DisplayAlert("Error", errorMessage, "ok");
            }
            else
            {
                //await Toast.Make("Menu Item Saved").Show();
                Cancel();
            }
            //save item in the db
            IsLoading = false;
        }
    }
}