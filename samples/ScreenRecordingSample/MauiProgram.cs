using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.ScreenRecording;

namespace ScreenRecordingSample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseScreenRecording()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<IScreenRecording>(ScreenRecording.Default);

		return builder.Build();
	}
}