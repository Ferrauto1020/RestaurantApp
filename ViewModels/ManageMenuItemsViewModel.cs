using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
//for set default icon

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
                HandleMenuItemChanged(model);
                //send the updated item details to the other part of the app
                WeakReferenceMessenger.Default.Send(MenuItemChangedMessage.From(model));
                await Toast.Make("Menu Item Saved").Show();
                Cancel();
            }
            //save item in the db
            IsLoading = false;
        }
        private void HandleMenuItemChanged(MenuItemModel model)
        {
            var menuItem = MenuItems.FirstOrDefault(m => m.Id == model.Id);
            if (menuItem != null)
            {
                //this menu item is not on the screen rn

                //check if the items still has a mapping to selected category
                if (!model.SelectedCategories.Any(c => c.Id == SelectedCategory.Id))
                {
                    //remove item from the current  UI category
                    MenuItems = [.. MenuItems.Where(m => m.Id != model.Id)];
                    return;
                }

                //update the details
                menuItem.Price = model.Price;
                menuItem.Description = model.Description;
                menuItem.Name = model.Name;
                menuItem.Icon = model.Icon;
                MenuItems = [.. MenuItems];
            }
            else if (model.SelectedCategories.Any(c => c.Id == SelectedCategory.Id))
            {
                //refresh the UI for display the item 
                var newMenuItem = new MenuItem
                {
                    Id = model.Id,
                    Description = model.Description,
                    Icon = model.Icon,
                    Name = model.Name,
                    Price = model.Price,
                };
                MenuItems = [.. MenuItems, newMenuItem];
            }
        }
    }
}