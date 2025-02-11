namespace System.Diagnostics;

/// <summary>Indicates whether the performance counter category can have multiple instances.</summary>
/// <filterpriority>1</filterpriority>
public enum PerformanceCounterCategoryType
{
	/// <summary>The performance counter category can have only a single instance.</summary>
	SingleInstance = 0,
	/// <summary>The performance counter category can have multiple instances.</summary>
	MultiInstance = 1,
	/// <summary>The instance functionality for the performance counter category is unknown. </summary>
	Unknown = -1
}
