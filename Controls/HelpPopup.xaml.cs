using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.Views;

namespace RestaurantApp.Controls;

public partial class HelpPopup : Popup
{
	public const string Email = "abc@gmail.com";
	public const string Phone = "+11 3335554488";
	public HelpPopup()
	{
		InitializeComponent();
	}

	private async void CloseLabel_Tapped(object sender, TappedEventArgs e)
	{
		await this.CloseAsync();
	}

	private async void Footer_Tapped(object sender, TappedEventArgs e)
	{
		await Launcher.Default.OpenAsync("https://github.com/Ferrauto1020?tab=repositories");
	}
	private string cpToClip = "Copy to clipboard";
	private async void CopyEmail_Tapped(object sender, TappedEventArgs e)
	{

		await Clipboard.Default.SetTextAsync(Email);

		copyEmailLabel.Text = "Copied";
		await Task.Delay(2000);
		copyEmailLabel.Text = cpToClip;
	}
	private async void CopyPhone_Tapped(object sender, TappedEventArgs e)
	{

		await Clipboard.Default.SetTextAsync(Phone);

		copyPhoneLabel.Text = "Copied";
		await Task.Delay(2000);
		copyPhoneLabel.Text = cpToClip;
	}

}