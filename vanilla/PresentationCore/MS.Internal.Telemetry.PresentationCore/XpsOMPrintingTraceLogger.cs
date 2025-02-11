using System.Diagnostics.Tracing;

namespace MS.Internal.Telemetry.PresentationCore;

internal static class XpsOMPrintingTraceLogger
{
	[EventData]
	internal class XpsOMStatus
	{
		public bool Enabled { get; set; } = true;
	}

	private static readonly string XpsOMEnabled = "XpsOMEnabled";

	internal static void LogXpsOMStatus(bool enabled)
	{
		TraceLoggingProvider.GetProvider()?.Write(XpsOMEnabled, TelemetryEventSource.MeasuresOptions(), new XpsOMStatus
		{
			Enabled = enabled
		});
	}
}
