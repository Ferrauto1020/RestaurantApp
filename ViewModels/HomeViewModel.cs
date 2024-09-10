using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RestaurantApp.Data;
using RestaurantApp.Models;
using MenuItem = RestaurantApp.Data.MenuItem;
namespace RestaurantApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject, IRecipient<MenuItemChangedMessage>
    {
        private readonly DatabaseService _databaseService;
        private readonly OrderViewModel _orderViewModel;
        private readonly SettingsViewModel _settingsViewModel;
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
        [ObservableProperty]
        private string _name = "Guest";
        public HomeViewModel(DatabaseService databaseService, OrderViewModel orderViewModel, SettingsViewModel settingsViewModel)
        {
            _databaseService = databaseService;
            _orderViewModel = orderViewModel;
            _settingsViewModel = settingsViewModel;
            CartItems.CollectionChanged += CartItems_CollectionChanged;
            WeakReferenceMessenger.Default.Register<MenuItemChangedMessage>(this);
            WeakReferenceMessenger.Default.Register<NameChangedMessage>(this, (recipient, message) => Name = message.Value);
         TaxPercentage = _settingsViewModel.GetTaxPercentage();
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

        [RelayCommand]
        private async Task ClearCartAsync()
        {
            if (await Shell.Current.DisplayAlert("Clear Cart?", "Do you really want to clear the cart?", "Yes", "No"))
                CartItems.Clear();
        }


        private void RecalculateAmounts()
        {
            Subtotal = CartItems.Sum(c => c.Amount);
        }

        [RelayCommand]
        private async Task TaxPercentageClickAsync()
        {
            var result = await Shell.Current.DisplayPromptAsync("Tax Percentage", "Enter the applicable tax percentage", placeholder: "10", initialValue: TaxPercentage.ToString());
            if (!string.IsNullOrWhiteSpace(result))
            {
                if (!int.TryParse(result, out int enteredTaxPercentage))
                {

                    await Shell.Current.DisplayAlert("invalid value", "Entered tax percentage is invalid", "Ok");
                    return;
                }
                //it was a valid numeric value
                if (enteredTaxPercentage > 100 || enteredTaxPercentage < 0)
                {
                    await Shell.Current.DisplayAlert("invalid value", "Tax percentage has to be between 0 and 100", "Ok");
                    return;
                }
                TaxPercentage = enteredTaxPercentage;
                _settingsViewModel.SetTaxPercentage(enteredTaxPercentage);
            }
        }

        [RelayCommand]
        private async Task PlaceOrderAsync(bool isPaidOnline)
        {
            IsLoading = true;
            if (await _orderViewModel.PlaceOrderAsync([.. CartItems], isPaidOnline))
            {
                CartItems.Clear();
            }
            IsLoading = false;
        }

        public void Receive(MenuItemChangedMessage message)
        {
            var model = message.Value;
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
            //check if the updated item is in the cart
            var cartItem = CartItems.FirstOrDefault(c => c.ItemId == model.Id);
            if (cartItem != null)
            {
                cartItem.Price = model.Price;
                cartItem.Name = model.Name;
                cartItem.Icon = model.Icon;

                var itemIndex = CartItems.IndexOf(cartItem);
                CartItems[itemIndex] = cartItem;
            }
        }
    }

}