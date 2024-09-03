using RestaurantApp.ViewModels;

namespace RestaurantApp.Pages;

public partial class MainPage : ContentPage
{
//private readonly HomeViewModel _homeViewModel;
	public MainPage(
	//	HomeViewModel homeViewModel
		)
	{
		InitializeComponent();
//		_homeViewModel = homeViewModel;
//		BindingContext = _homeViewModel;
//		Starting();
	}
	private async void Starting()
	{
//		await _homeViewModel.InitializeAsync();
	}

}

