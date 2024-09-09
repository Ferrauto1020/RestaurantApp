using CommunityToolkit.Mvvm.Input;
using RestaurantApp.Data;
using RestaurantApp.Models;
using Windows.UI.Notifications;

namespace RestaurantApp.Controls;

public partial class SaveMenuItemFormControl : ContentView
{
	
	private const string DefaultIcon = "image_add.png";
	public SaveMenuItemFormControl()
	{
		InitializeComponent();
	}


	public static readonly BindableProperty ItemProperty = BindableProperty
	.Create(nameof(Item), typeof(MenuItemModel), typeof(CategoriesListControl), new MenuItemModel(), propertyChanged: OnItemChanged);

	private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (newValue is MenuItemModel menuItemModel)
		{
			if (bindable is SaveMenuItemFormControl thisControl)
			{
				if (menuItemModel.Id > 0)
				{
					thisControl.SetIconImage(false, menuItemModel.Icon, thisControl);
					thisControl.ExistingIcon = menuItemModel.Icon;
				}
				else
				{
					thisControl.SetIconImage(true, null, thisControl);
				}
			}
		}

	}
	public string? ExistingIcon { get; set; }
	public MenuItemModel Item
	{
		get => (MenuItemModel)GetValue(ItemProperty);
		set => SetValue(ItemProperty, value);
	}
	[RelayCommand]
	private void TogglecategorySelection(MenuCategoryModel category) =>
	category.IsSelected = !category.IsSelected;

	public event Action? OnCancel;

	[RelayCommand]
	private void Cancel() => OnCancel?.Invoke();

	private async void PickImageButton_Clicked(object sender, EventArgs e)
	{
		var fileResult = await MediaPicker.PickPhotoAsync();
		if (fileResult != null)
		{
			//upload or save the image
			var imageStream = await fileResult.OpenReadAsync();
			var localPath = Path.Combine(FileSystem.AppDataDirectory, fileResult.FileName);
			using var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write);
			await imageStream.CopyToAsync(fs);
			SetIconImage(isDefault: false, localPath);
			Item.Icon = localPath;
		}
		else
		{
			if (ExistingIcon != null)
			{
				SetIconImage(isDefault: false, ExistingIcon);
			}
			else
			{
				SetIconImage(isDefault: true);
			}
		}
	}
	public void SetIconImage(bool isDefault, string? iconSource = null, SaveMenuItemFormControl? control = null)
	{
		int size = 100;
		if (isDefault)
		{
			iconSource = DefaultIcon;
			size = 36;
		}
		control = control ?? this;
		control.itemIcon.Source = iconSource;
		control.itemIcon.HeightRequest = control.itemIcon.WidthRequest = size;
	}

public event Action<MenuItemModel>?OnSaveItem;

	[RelayCommand]
	private async Task SaveMenuItemAsync()
	{
		if (string.IsNullOrWhiteSpace(Item.Name) || string.IsNullOrWhiteSpace(Item.Description))
		{
			await ErrorAllertAsync("Item name and description are mandatory");
			return;
		}
		if (Item.SelectedCategories.Length==0)
		{
			await ErrorAllertAsync("Please select at least 1 category");
			return;
		}

		if (Item.Icon == DefaultIcon)
		{
			await ErrorAllertAsync("Icon image is Mandatory");
			return;
		}

		OnSaveItem?.Invoke(Item);
		static async Task ErrorAllertAsync(string message) =>
		await Shell.Current.DisplayAlert("Validation error", message, "OK");

	}
}