namespace System.Diagnostics;

/// <summary>Specifies the lifetime of a performance counter instance.</summary>
public enum PerformanceCounterInstanceLifetime
{
	/// <summary>Remove the performance counter instance when no counters are using the process category.</summary>
	Global,
	/// <summary>Remove the performance counter instance when the process is closed.</summary>
	Process
}
