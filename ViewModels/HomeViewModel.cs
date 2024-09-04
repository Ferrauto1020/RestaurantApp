using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        public ObservableCollection<CartModel> CartItems { get; set; } = new();
        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(TaxAmount)), NotifyPropertyChangedFor(nameof(Total))]
        private decimal _subtotal;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(TaxAmount)), NotifyPropertyChangedFor(nameof(Total))]
        private int _taxPercentage;
        public decimal TaxAmount => (Subtotal * TaxPercentage) / 100;
        public decimal Total => Subtotal + TaxAmount;
        public HomeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            CartItems.CollectionChanged += CartItems_CollectionChanged;
        }

        private void CartItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //it will be executed anytime we are adding any items to the cart
            //removing items from the cart
            //clearing the cart
            RecalculateAmounts();
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
        [RelayCommand]
        private void AddToCart(MenuItem menuItem)
        {
            var cartItem = CartItems.FirstOrDefault(c => c.ItemId == menuItem.Id);
            if (cartItem == null)
            {
                cartItem = new CartModel
                {
                    ItemId = menuItem.Id,
                    Icon = menuItem.Icon,
                    Name = menuItem.Name,
                    Price = menuItem.Price,
                    Quantity = 1
                };
                CartItems.Add(cartItem);
            }
            else
            {
                //item already in cart, increase the quantity
                cartItem.Quantity++;
                RecalculateAmounts();
            }
        }

        [RelayCommand]
        private void IncreaseQuantity(CartModel cartItem)
        {
            cartItem.Quantity++;
            RecalculateAmounts();
        }

        [RelayCommand]
        private void DecreaseQuantity(CartModel cartItem)
        {
            cartItem.Quantity--;
            if (cartItem.Quantity == 0)
                CartItems.Remove(cartItem);
            else
                RecalculateAmounts();
        }
        [RelayCommand]
        private void RemoveItemFromCart(CartModel cartItem) => CartItems.Remove(cartItem);

        private void RecalculateAmounts()
        {
            Subtotal = CartItems.Sum(c => c.Amount);
        }
    }
}