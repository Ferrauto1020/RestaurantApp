using CommunityToolkit.Mvvm.Input;
using RestaurantApp.Data;
using RestaurantApp.Models;

namespace RestaurantApp.Controls;

public partial class SaveMenuItemFormControl : ContentView
{
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
					/* thisControl.itemIcon.Source = menuItemModel.Icon;
					thisControl.itemIcon.HeightRequest = thisControl.itemIcon.WidthRequest = 100;
				 */
					thisControl.SetIconImage(false, menuItemModel.Icon, thisControl);
				}
				else
				{/* 
					thisControl.itemIcon.Source = "image_add.png";
					thisControl.itemIcon.HeightRequest = thisControl.itemIcon.WidthRequest = 36;
				 */
					thisControl.SetIconImage(true, null, thisControl);
				}
			}
		}
	}

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
		}
		else
		{
			SetIconImage(isDefault: true);
		}
	}
	public void SetIconImage(bool isDefault, string? iconSource = null, SaveMenuItemFormControl? control = null)
	{
		int size = 100;
		if (isDefault)
		{
			iconSource = "image_add.png";
			size = 36;
		}
		control = control ?? this;
		control.itemIcon.Source = iconSource;
		control.itemIcon.HeightRequest = control.itemIcon.WidthRequest = size;
	}
}