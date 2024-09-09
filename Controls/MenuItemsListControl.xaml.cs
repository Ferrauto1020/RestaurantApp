
using CommunityToolkit.Mvvm.Input;
using MenuItem = RestaurantApp.Data.MenuItem;
namespace RestaurantApp.Controls;

public partial class MenuItemsListControl : ContentView
{
	public MenuItemsListControl()
	{
		InitializeComponent();
	}
	public static readonly BindableProperty ItemsProperty =
	BindableProperty.Create(nameof(Items), typeof(MenuItem[]), typeof(MenuItemsListControl), Array.Empty<MenuItem>());
	public MenuItem[] Items
	{
		get => (MenuItem[])GetValue(ItemsProperty);
		set => SetValue(ItemsProperty, value);
	}
	public event Action<MenuItem> OnSelectItem;
	[RelayCommand]
	private void SelectItem(MenuItem item) => OnSelectItem?.Invoke(item);
}