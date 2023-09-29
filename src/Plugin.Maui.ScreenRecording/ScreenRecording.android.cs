using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Display;
using Android.Media;
using Android.Media.Projection;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Graphics.Drawable;
using Java.Util;
using Microsoft.Maui.ApplicationModel;
using static Android.Provider.ContactsContract.CommonDataKinds;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;

namespace Plugin.Maui.ScreenRecording;

//TODO see https://github.com/pkrieter/android-background-screen-recorder/blob/master/app/src/main/java/com/ifib/pkrieter/datarecorder/RecordService.java
partial class ScreenRecordingImplementation : IScreenRecording
{

	public static EventHandler<ScreenRecordingEventArgs> ScreenRecordingPermissionHandler;

#pragma warning disable IDE0040 // Add accessibility modifiers
#pragma warning disable IDE1006 // Naming Styles
	private const int REQUEST_MEDIA_PROJECTION = 1;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0040 // Add accessibility modifiers
	MediaProjectionManager mediaProjectionManager;

	public bool IsRecording { get; set; } = false;

	public bool IsSupported => true;

	public Task StartRecording(bool enableMicrophone)
	{
		mediaProjectionManager = (MediaProjectionManager)Platform.CurrentActivity.GetSystemService(Context.MediaProjectionService);

		//var recActivity = new RecorderActivity(Platform.CurrentActivity, mediaProjectionManager);
		//recActivity.RegisterForActivityResult(
		//new ActivityResultContracts.StartActivityForResult());
		
		Platform.CurrentActivity.StartActivityForResult(mediaProjectionManager.CreateScreenCaptureIntent(), REQUEST_MEDIA_PROJECTION);

		//var tcs = new TaskCompletionSource<bool>();
		EventHandler<ScreenRecordingEventArgs> handler = null;

		handler = (sender, args) =>
		{			
			var foo = Platform.CurrentActivity.StartForegroundService(new Intent(Platform.CurrentActivity, typeof(ScreenRecordingService)));
			
			if (args.ResultCode == Result.Ok)
			{
				var projection = mediaProjectionManager.GetMediaProjection((int)args.ResultCode, args.Data);
				var display = projection.CreateVirtualDisplay("screen", (int)DeviceDisplay.Current.MainDisplayInfo.Width, (int)DeviceDisplay.Current.MainDisplayInfo.Height, (int)DeviceDisplay.Current.MainDisplayInfo.Density, Android.Views.DisplayFlags.Presentation, null, null, null);
			}

			//var display = projection.CreateVirtualDisplay("screen", (int)DeviceDisplay.Current.MainDisplayInfo.Width, (int)DeviceDisplay.Current.MainDisplayInfo.Height,
			//(int)DeviceDisplay.Current.MainDisplayInfo.Density, Android.Views.DisplayFlags.Presentation, null, null, null);
			//tcs.SetResult(((RecorderActivity)sender).ContactName);
		};

		ScreenRecordingPermissionHandler += handler;
		//var projection = manager.GetMediaProjection(resultCode, data) as MediaProjection;
		return Task.CompletedTask;
	}

	public async Task<ScreenRecordingFile?> StopRecording(ScreenRecordingOptions? options)
	{

		return await Task.FromResult(new ScreenRecordingFile(""));
	}
}

public class ScreenRecordingEventArgs : EventArgs
{
	public Result ResultCode { get; set; }

	public Intent? Data { get; set; }
}

[Service(ForegroundServiceType = ForegroundService.TypeMediaProjection, Exported = false, Enabled = true)]
public class ScreenRecordingService : Service
{
	public override IBinder? OnBind(Intent? intent)
	{
		throw new NotImplementedException();
	}

	public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
	{

		var pendingIntent = PendingIntent.GetActivity(Platform.CurrentActivity, 0, intent, PendingIntentFlags.UpdateCurrent);
		NotificationCompat.Builder notification = new NotificationCompat.Builder(Platform.AppContext, "screenrecord")
			.SetForegroundServiceBehavior(NotificationCompat.ForegroundServiceImmediate)
			.SetSmallIcon(Resource.Drawable.abc_ic_menu_overflow_material)
			.SetContentTitle("Recording Screen")
			.SetContentText("Tap to cancel")
			.SetOngoing(true)
			.SetContentIntent(pendingIntent);

		var notif = notification.Build();

		NotificationChannel notificationChannel = new("screenrecord", "record", NotificationImportance.Default);
		notificationChannel.Description = "foo";
		NotificationManager notificationManager = (NotificationManager)Platform.CurrentActivity.GetSystemService(Context.NotificationService);
		notificationManager.CreateNotificationChannel(notificationChannel);

		StartForeground(1337, notif);

		return base.OnStartCommand(intent, flags, startId);
	}
}