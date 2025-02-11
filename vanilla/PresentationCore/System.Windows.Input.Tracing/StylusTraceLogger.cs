using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using MS.Internal.Telemetry.PresentationCore;

namespace System.Windows.Input.Tracing;

internal static class StylusTraceLogger
{
	[Flags]
	internal enum FeatureFlags
	{
		None = 0,
		CustomTouchDeviceUsed = 1,
		StylusPluginsUsed = 2,
		FlickScrollingUsed = 4,
		PointerStackEnabled = 0x10000000,
		WispStackEnabled = 0x20000000
	}

	[EventData]
	internal class StylusStatistics
	{
		public FeatureFlags FeaturesUsed { get; set; }
	}

	[EventData]
	internal class ReentrancyEvent
	{
		public string FunctionName { get; set; } = string.Empty;
	}

	[EventData]
	internal class StylusSize
	{
		public double Width { get; set; } = double.NaN;

		public double Height { get; set; } = double.NaN;

		public StylusSize(Size size)
		{
			Width = size.Width;
			Height = size.Height;
		}
	}

	[EventData]
	internal class StylusDeviceInfo
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string PlugAndPlayId { get; set; }

		public string Capabilities { get; set; }

		public StylusSize TabletSize { get; set; }

		public StylusSize ScreenSize { get; set; }

		public string DeviceType { get; set; }

		public int MaxContacts { get; set; }

		public StylusDeviceInfo(int id, string name, string pnpId, TabletHardwareCapabilities capabilities, Size tabletSize, Size screenSize, TabletDeviceType deviceType, int maxContacts)
		{
			Id = id;
			Name = name;
			PlugAndPlayId = pnpId;
			Capabilities = capabilities.ToString("F");
			TabletSize = new StylusSize(tabletSize);
			ScreenSize = new StylusSize(screenSize);
			DeviceType = deviceType.ToString("F");
			MaxContacts = maxContacts;
		}
	}

	[EventData]
	internal class StylusDisconnectInfo
	{
		public int Id { get; set; } = -1;
	}

	[EventData]
	internal class StylusErrorEventData
	{
		public string Error { get; set; }
	}

	private const string StartupEventTag = "StylusStartup";

	private const string ShutdownEventTag = "StylusShutdown";

	private const string StatisticsTag = "StylusStatistics";

	private const string ErrorTag = "StylusError";

	private const string DeviceConnectTag = "StylusConnect";

	private const string DeviceDisconnectTag = "StylusDisconnect";

	private const string ReentrancyTag = "StylusReentrancy";

	private const string ReentrancyRetryLimitTag = "StylusReentrancyRetryLimitReached";

	internal static void LogStartup()
	{
		Log("StylusStartup");
	}

	internal static void LogStatistics(StylusStatistics stylusData)
	{
		Requires<ArgumentNullException>(stylusData != null);
		Log("StylusStatistics", stylusData);
	}

	internal static void LogReentrancyRetryLimitReached()
	{
		Log("StylusReentrancyRetryLimitReached");
	}

	internal static void LogError(string error)
	{
		Requires<ArgumentNullException>(error != null);
		Log("StylusError", new StylusErrorEventData
		{
			Error = error
		});
	}

	internal static void LogDeviceConnect(StylusDeviceInfo deviceInfo)
	{
		Requires<ArgumentNullException>(deviceInfo != null);
		Log("StylusConnect", deviceInfo);
	}

	internal static void LogDeviceDisconnect(int deviceId)
	{
		Log("StylusDisconnect", new StylusDisconnectInfo
		{
			Id = deviceId
		});
	}

	internal static void LogReentrancy([CallerMemberName] string functionName = "")
	{
		Log("StylusReentrancy", new ReentrancyEvent
		{
			FunctionName = functionName
		});
	}

	internal static void LogShutdown()
	{
		Log("StylusShutdown");
	}

	private static void Requires<T>(bool condition) where T : Exception, new()
	{
		if (!condition)
		{
			throw new T();
		}
	}

	private static void Log(string tag)
	{
		TraceLoggingProvider.GetProvider()?.Write(tag, TelemetryEventSource.MeasuresOptions());
	}

	private static void Log<T>(string tag, T data = null) where T : class
	{
		TraceLoggingProvider.GetProvider()?.Write(tag, TelemetryEventSource.MeasuresOptions(), data);
	}
}
