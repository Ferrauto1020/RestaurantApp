using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantApp.Data;

namespace RestaurantApp.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        public MenuCategory[] _categories;

    }
}