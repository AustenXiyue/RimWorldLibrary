using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.PerformanceData;

/// <summary>Contains the raw data for a counter.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class CounterData
{
	/// <summary>Sets or gets the raw counter data.</summary>
	/// <returns>The raw counter data.</returns>
	public long RawValue
	{
		[SecurityCritical]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
		[SecurityCritical]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Sets or gets the counter data.</summary>
	/// <returns>The counter data.</returns>
	/// <filterpriority>2</filterpriority>
	public long Value
	{
		[SecurityCritical]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
		[SecurityCritical]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	internal CounterData()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Decrements the counter value by 1.</summary>
	[SecurityCritical]
	public void Decrement()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Increments the counter value by 1.</summary>
	[SecurityCritical]
	public void Increment()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Increments the counter value by the specified amount.</summary>
	/// <param name="value">The amount by which to increment the counter value. The increment value can be positive or negative.</param>
	[SecurityCritical]
	public void IncrementBy(long value)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
