#if ANDROID
using Android.App;
using Android.Content;
#endif
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.ScreenRecording;

public static class AppBuilderExtensions
{
	public static MauiAppBuilder UseScreenRecording(this MauiAppBuilder builder)
	{
		builder.ConfigureLifecycleEvents(events =>
		{
#if ANDROID
			events.AddAndroid(android => android
				.OnActivityResult((activity, requestCode, resultCode, data) => LogEvent(resultCode, data)));

			static bool LogEvent(Result result, Intent? data)
			{
				ScreenRecordingImplementation.ScreenRecordingPermissionHandler.Invoke(null, new ScreenRecordingEventArgs
				{
					ResultCode = result,
					Data = data,
				});

				return true;
			}
#endif
		});

		return builder;
	}
}
