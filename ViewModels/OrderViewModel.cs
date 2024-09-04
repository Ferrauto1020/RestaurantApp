using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
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
            //operation was successfull
            return true;
        }
    }
}