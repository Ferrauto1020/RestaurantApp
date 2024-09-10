using CommunityToolkit.Maui.Views;
using RestaurantApp.Controls;
using RestaurantApp.Pages;

namespace RestaurantApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		var helpPopup = new HelpPopup();
		await this.ShowPopupAsync(helpPopup);
    }
}
