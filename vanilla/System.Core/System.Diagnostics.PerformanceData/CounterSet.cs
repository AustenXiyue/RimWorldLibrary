using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.PerformanceData;

/// <summary>Defines a set of logical counters.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class CounterSet : IDisposable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceData.CounterSet" /> class.</summary>
	/// <param name="providerGuid">Guid that uniquely identifies the provider of the counter data. Use the Guid specified in the manifest.</param>
	/// <param name="counterSetGuid">Guid that uniquely identifies the counter set for a provider. Use the Guid specified in the manifest.</param>
	/// <param name="instanceType">Identifies the type of the counter set, for example, whether the counter set is a single or multiple instance counter set.</param>
	/// <exception cref="T:System.InsufficientMemoryException">Not enough memory is available to complete the operation.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">Not supported prior to Windows Vista.</exception>
	/// <exception cref="T:System.ArgumentException">One of the parameters is NULL or not valid.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">An underlying Win32 function call failed.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public CounterSet(Guid providerGuid, Guid counterSetGuid, CounterSetInstanceType instanceType)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Adds a counter to the counter set by using the specified counter identifier and type.</summary>
	/// <param name="counterId">Identifies the counter. Use the same value that you used in the manifest to define the counter.</param>
	/// <param name="counterType">Identifies the counter type. The counter type determines how the counter data is calculated, averaged, and displayed. </param>
	/// <exception cref="T:System.ArgumentException">The counter identifier already exists in the set or is negative, or the counter type is NULL or not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">You cannot add counters to the counter set after creating an instance of the counter set.</exception>
	/// <filterpriority>2</filterpriority>
	public void AddCounter(int counterId, CounterType counterType)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Adds a counter to the counter set by using the specified counter identifier and type and a display name for the counter.</summary>
	/// <param name="counterId">Identifies the counter. Use the same value that you used in the manifest to define the counter.</param>
	/// <param name="counterType">Identifies the counter type. The counter type determines how the counter data is calculated, averaged, and displayed. </param>
	/// <param name="counterName">Name of the counter. You can use this name to index the counter in the counter set instance. (See <see cref="P:System.Diagnostics.PerformanceData.CounterSetInstanceCounterDataSet.Item(System.String)" />.)</param>
	/// <exception cref="T:System.ArgumentException">The counter identifier already exists in the set or is negative, or the counter type is NULL or not valid.</exception>
	/// <exception cref="T:System.InvalidOperationException">You cannot add counters to the counter set after creating an instance of the counter set.</exception>
	/// <filterpriority>2</filterpriority>
	public void AddCounter(int counterId, CounterType counterType, string counterName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Creates an instance of the counter set.</summary>
	/// <returns>An instance of the counter set which will contain the counter data.</returns>
	/// <param name="instanceName">Name of the instance. The name must be unique.</param>
	/// <exception cref="T:System.ArgumentException">The instance name is NULL.</exception>
	/// <exception cref="T:System.InvalidOperationException">You must add counters to the counter set before creating an instance of the counter set.</exception>
	/// <filterpriority>2</filterpriority>
	[SecuritySafeCritical]
	[PermissionSet(SecurityAction.Demand, Unrestricted = true)]
	public CounterSetInstance CreateCounterSetInstance(string instanceName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	/// <summary>Releases all unmanaged resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases all unmanaged resources used by this object and optionally release the managed resources.</summary>
	/// <param name="disposing">True if this was called from the Dispose method, False if called from the finalizer.</param>
	[SecuritySafeCritical]
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
