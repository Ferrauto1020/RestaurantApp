using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RestaurantApp.Data;
using RestaurantApp.Models;

namespace RestaurantApp.ViewModels
{
    public partial class OrderViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        public OrderViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        public ObservableCollection<OrderModel> Orders { get; set; } = [];
        public async Task<bool> PlaceOrderAsync(CartModel[] cartItems, bool isPaidOnline)
        {
            var orderItems = cartItems.Select(c => new OrderItem
            {
                Icon = c.Icon,
                ItemId = c.ItemId,
                Name = c.Name,
                Price = c.Price,
                Quantity = c.Quantity
            }).ToArray();
            var orderModel = new OrderModel
            {
                OrderDate = DateTime.Now,
                PaymentMode = isPaidOnline ? "Online" : "Cash",
                TotalAmountPaid = cartItems.Sum(c => c.Amount),
                TotalItemsCount = cartItems.Length,
                Items = orderItems
            };
            var errorMessage = await _databaseService.PlaceOrderAsync(orderModel);
            if (!string.IsNullOrEmpty(errorMessage))
            {

                //order creation failed
                await Shell.Current.DisplayAlert("Error", errorMessage, "Ok");
                return false;
            }
            await Toast.Make("Order send").Show();
            //operation was successfull
            Orders.Add(orderModel); 
            return true;
        }

        private bool _isInitialized;
        [ObservableProperty]
        private bool _isLoaded;
        public async ValueTask InitializeAsync()
        {
            if (_isInitialized)
                return;
            _isInitialized = true;
            IsLoaded = true;
            var dbOrders = await _databaseService.GetOrdersAsync();
            var orders = dbOrders.Select(o => new OrderModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                PaymentMode = o.PaymentMode,
                TotalAmountPaid = o.TotalAmountPaid,
                TotalItemsCount = o.TotalItemsCount
            });
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
            IsLoaded = false;
        }

        [ObservableProperty]
        private OrderItem[] _orderItems = [];
        [RelayCommand]
        public async Task SelectOrderAsync(OrderModel? order)
        {
            var prevSelectedOrder = Orders.FirstOrDefault(o=>o.IsSelected);
            if (prevSelectedOrder != null)
            {
                prevSelectedOrder.IsSelected = false;
                if(prevSelectedOrder.Id == order?.Id)
                {
                    OrderItems = [];
                    return;
                }
            }

            if (order == null || order.Id == 0)
            {
                OrderItems = [];
                return;
            }
            IsLoaded = true;
            order.IsSelected = true;
            OrderItems = await _databaseService.GetOrderItemsAsync(order.Id);
            IsLoaded = false;
        }
    }
}