namespace System.Diagnostics;

/// <summary>Provides a set of utility functions for interpreting performance counter data.</summary>
/// <filterpriority>2</filterpriority>
public static class CounterSampleCalculator
{
	/// <summary>Computes the calculated value of a single raw counter sample.</summary>
	/// <returns>A floating-point representation of the performance counter's calculated value.</returns>
	/// <param name="newSample">A <see cref="T:System.Diagnostics.CounterSample" /> that indicates the most recent sample the system has taken. </param>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static float ComputeCounterValue(CounterSample newSample)
	{
		switch (newSample.CounterType)
		{
		case PerformanceCounterType.NumberOfItemsHEX32:
		case PerformanceCounterType.NumberOfItemsHEX64:
		case PerformanceCounterType.NumberOfItems32:
		case PerformanceCounterType.NumberOfItems64:
		case PerformanceCounterType.RawFraction:
			return newSample.RawValue;
		default:
			return 0f;
		}
	}

	/// <summary>Computes the calculated value of two raw counter samples.</summary>
	/// <returns>A floating-point representation of the performance counter's calculated value.</returns>
	/// <param name="oldSample">A <see cref="T:System.Diagnostics.CounterSample" /> that indicates a previous sample the system has taken. </param>
	/// <param name="newSample">A <see cref="T:System.Diagnostics.CounterSample" /> that indicates the most recent sample the system has taken. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="oldSample" /> uses a counter type that is different from <paramref name="newSample" />. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">
	///   <paramref name="newSample" /> counter type has a Performance Data Helper (PDH) error. For more information, see "Checking PDH Interface Return Values" in the Win32 and COM Development section of this documentation.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO("What's the algorithm?")]
	public static float ComputeCounterValue(CounterSample oldSample, CounterSample newSample)
	{
		if (newSample.CounterType != oldSample.CounterType)
		{
			throw new Exception("The counter samples must be of the same type");
		}
		switch (newSample.CounterType)
		{
		case PerformanceCounterType.NumberOfItemsHEX32:
		case PerformanceCounterType.NumberOfItemsHEX64:
		case PerformanceCounterType.NumberOfItems32:
		case PerformanceCounterType.NumberOfItems64:
		case PerformanceCounterType.RawFraction:
			return newSample.RawValue;
		case PerformanceCounterType.AverageCount64:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.BaseValue - oldSample.BaseValue);
		case PerformanceCounterType.AverageTimer32:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)newSample.SystemFrequency / (float)(newSample.BaseValue - oldSample.BaseValue);
		case PerformanceCounterType.CounterDelta32:
		case PerformanceCounterType.CounterDelta64:
			return newSample.RawValue - oldSample.RawValue;
		case PerformanceCounterType.CounterMultiTimer:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 100f / (float)newSample.BaseValue;
		case PerformanceCounterType.CounterMultiTimer100Ns:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec) * 100f / (float)newSample.BaseValue;
		case PerformanceCounterType.CounterMultiTimerInverse:
			return ((float)newSample.BaseValue - (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100f;
		case PerformanceCounterType.CounterMultiTimer100NsInverse:
			return ((float)newSample.BaseValue - (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100f;
		case PerformanceCounterType.CountPerTimeInterval32:
		case PerformanceCounterType.CountPerTimeInterval64:
		case PerformanceCounterType.CounterTimer:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp);
		case PerformanceCounterType.CounterTimerInverse:
			return (1f - (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100f;
		case PerformanceCounterType.ElapsedTime:
			return 0f;
		case PerformanceCounterType.Timer100Ns:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 100f;
		case PerformanceCounterType.Timer100NsInverse:
			return (1f - (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100f;
		case PerformanceCounterType.RateOfCountsPerSecond32:
		case PerformanceCounterType.RateOfCountsPerSecond64:
			return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 10000000f;
		default:
			Console.WriteLine("Counter type {0} not handled", newSample.CounterType);
			return 0f;
		}
	}
}
