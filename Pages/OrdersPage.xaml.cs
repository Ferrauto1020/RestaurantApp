using RestaurantApp.ViewModels;

namespace RestaurantApp.Pages;

public partial class OrdersPage : ContentPage
{
	private readonly OrderViewModel _orderViewModel;
	public OrdersPage(OrderViewModel orderViewModel)
	{
		InitializeComponent();
		_orderViewModel = orderViewModel;
		BindingContext = _orderViewModel;
		InitializeViewModelAsync();
	}
	private async void InitializeViewModelAsync()=> await _orderViewModel.InitializeAsync(); 
}