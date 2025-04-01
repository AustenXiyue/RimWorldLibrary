using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.PerformanceData;

/// <summary>Contains the collection of counter values.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CounterSetInstanceCounterDataSet : IDisposable
{
	/// <summary>Accesses a counter value in the collection by using the specified counter name.</summary>
	/// <returns>The counter data.</returns>
	/// <param name="counterName">Name of the counter. This is the name that you used when you added the counter to the counter set.</param>
	public CounterData this[string counterName]
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	internal CounterSetInstanceCounterDataSet()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	[SpecialName]
	public CounterData get_Item(int counterId)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Releases all unmanaged resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
