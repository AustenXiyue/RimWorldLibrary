namespace MS.Internal.Telemetry.PresentationFramework;

internal static class ControlsTraceLogger
{
	private static readonly string ControlsUsed = "ControlsUsed";

	private static TelemetryControls _telemetryControls = TelemetryControls.None;

	internal static void LogUsedControlsDetails()
	{
		TraceLoggingProvider.GetProvider()?.Write(ControlsUsed, TelemetryEventSource.MeasuresOptions(), new
		{
			ControlsUsedInApp = _telemetryControls
		});
	}

	internal static void AddControl(TelemetryControls control)
	{
		_telemetryControls |= control;
	}
}
