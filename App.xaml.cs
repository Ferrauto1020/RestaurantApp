using RestaurantApp.Data;

namespace RestaurantApp;

public partial class App : Application
{
	private readonly DatabaseService _databaseService;
	public App(DatabaseService databaseService)
	{
		InitializeComponent();
		_databaseService = databaseService;
		MainPage = new AppShell();
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await _databaseService.InitializeDatabaseAsync();
		
	}
}
