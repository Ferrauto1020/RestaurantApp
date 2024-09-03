using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using RestaurantApp.Data;

namespace RestaurantApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
		
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
				fonts.AddFont("Poppins-Semibold.ttf", "PoppinsSemibold");
			})
			.UseMauiCommunityToolkit()
			;

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<DatabaseService>();
		return builder.Build();
		
	}
}
