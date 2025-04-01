using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Prepares performance data for the performance.dll the system loads when working with performance counters.</summary>
/// <filterpriority>2</filterpriority>
[System.MonoTODO("not implemented")]
[Obsolete("use PerformanceCounter")]
[ComVisible(true)]
[Guid("82840be1-d273-11d2-b94a-00600893b17a")]
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class PerformanceCounterManager : ICollectData
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterManager" /> class.</summary>
	[Obsolete("use PerformanceCounter")]
	public PerformanceCounterManager()
	{
	}

	/// <summary>Called by the perf dll's close performance data </summary>
	void ICollectData.CloseData()
	{
		throw new NotImplementedException();
	}

	/// <summary>Performance data collection routine. Called by the PerfCount perf dll.</summary>
	/// <param name="callIdx">The call index. </param>
	/// <param name="valueNamePtr">A pointer to a Unicode string list with the requested Object identifiers.</param>
	/// <param name="dataPtr">A pointer to the data buffer.</param>
	/// <param name="totalBytes">A pointer to a number of bytes.</param>
	/// <param name="res">When this method returns, contains a <see cref="T:System.IntPtr" /> with a value of -1.</param>
	void ICollectData.CollectData(int callIdx, IntPtr valueNamePtr, IntPtr dataPtr, int totalBytes, out IntPtr res)
	{
		throw new NotImplementedException();
	}
}
