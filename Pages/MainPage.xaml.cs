using RestaurantApp.ViewModels;
using MenuItem = RestaurantApp.Data.MenuItem;
namespace RestaurantApp.Pages;

public partial class MainPage : ContentPage
{
	private readonly HomeViewModel _homeViewModel;
	private readonly SettingsViewModel _settingsViewModel;

	public MainPage(
		HomeViewModel homeViewModel,
		SettingsViewModel settingsViewModel
		)
	{
		InitializeComponent();
		_homeViewModel = homeViewModel;
		_settingsViewModel = settingsViewModel;
		BindingContext = _homeViewModel;
		Starting();
	}
	private async void Starting()
	{
		await _homeViewModel.InitializeAsync();
	}


	protected override async void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);	
		await _settingsViewModel.InitializeAsync();

	}

	private async void CategoriesListControl_OnCategorySelected(Models.MenuCategoryModel category)
	{
		await _homeViewModel.SelectCategoryCommand.ExecuteAsync(category.Id);
	}

	private void MenuItemsListControl_OnSelectItem(MenuItem menuItem)
	{
		_homeViewModel.AddToCartCommand.Execute(menuItem);
	}

}

