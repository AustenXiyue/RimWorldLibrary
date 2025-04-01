namespace System.Diagnostics;

/// <summary>Defines access levels used by <see cref="T:System.Diagnostics.PerformanceCounter" /> permission classes.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
public enum PerformanceCounterPermissionAccess
{
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> has no permissions.</summary>
	None = 0,
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> can read categories.</summary>
	[Obsolete]
	Browse = 1,
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> can read categories.</summary>
	Read = 1,
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> can write categories.</summary>
	Write = 2,
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> can read and write categories.</summary>
	[Obsolete]
	Instrument = 3,
	/// <summary>The <see cref="T:System.Diagnostics.PerformanceCounter" /> can read, write, and create categories.</summary>
	Administer = 7
}
