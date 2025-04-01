using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.PerformanceData;

/// <summary>Creates an instance of the logical counters defined in the <see cref="T:System.Diagnostics.PerformanceData.CounterSet" /> class.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CounterSetInstance : IDisposable
{
	/// <summary>Retrieves the collection of counter data for the counter set instance.</summary>
	/// <returns>A collection of the counter data contained in the counter set instance.</returns>
	/// <filterpriority>2</filterpriority>
	public CounterSetInstanceCounterDataSet Counters
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	internal CounterSetInstance()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases all unmanaged resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
