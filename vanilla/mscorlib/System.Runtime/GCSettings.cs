using System.Runtime.ConstrainedExecution;

namespace System.Runtime;

/// <summary>Specifies the garbage collection settings for the current process. </summary>
public static class GCSettings
{
	/// <summary>Gets a value that indicates whether server garbage collection is enabled.</summary>
	/// <returns>true if server garbage collection is enabled; otherwise, false.</returns>
	[MonoTODO("Always returns false")]
	public static bool IsServerGC => false;

	/// <summary>Gets or sets the current latency mode for garbage collection.</summary>
	/// <returns>One of the enumeration values that specifies the latency mode. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <see cref="T:System.Runtime.GCLatencyMode" /> is set to an invalid value.</exception>
	[MonoTODO("Always returns GCLatencyMode.Interactive and ignores set")]
	public static GCLatencyMode LatencyMode
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return GCLatencyMode.Interactive;
		}
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		set
		{
		}
	}

	/// <summary>Gets or sets a value that indicates whether a full blocking garbage collection compacts the large object heap (LOH). </summary>
	/// <returns>One of the enumeration values that indicates whether a full blocking garbage collection compacts the LOH. </returns>
	public static GCLargeObjectHeapCompactionMode LargeObjectHeapCompactionMode
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get;
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		set;
	}
}
