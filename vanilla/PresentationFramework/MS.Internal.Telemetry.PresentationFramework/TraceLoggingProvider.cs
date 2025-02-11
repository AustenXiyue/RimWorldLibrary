using System;
using System.Diagnostics.Tracing;

namespace MS.Internal.Telemetry.PresentationFramework;

internal static class TraceLoggingProvider
{
	private static EventSource _logger;

	private static readonly object _lockObject = new object();

	private static readonly string ProviderName = "Microsoft.DOTNET.WPF.PresentationFramework";

	internal static EventSource GetProvider()
	{
		if (_logger == null)
		{
			lock (_lockObject)
			{
				if (_logger == null)
				{
					try
					{
						_logger = new TelemetryEventSource(ProviderName);
					}
					catch (ArgumentException)
					{
					}
				}
			}
		}
		return _logger;
	}
}
