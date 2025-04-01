using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System;

/// <summary>Controls the system garbage collector, a service that automatically reclaims unused memory.</summary>
/// <filterpriority>2</filterpriority>
public static class GC
{
	private enum StartNoGCRegionStatus
	{
		Succeeded,
		NotEnoughMemory,
		AmountTooLarge,
		AlreadyInProgress
	}

	private enum EndNoGCRegionStatus
	{
		Succeeded,
		NotInProgress,
		GCInduced,
		AllocationExceeded
	}

	internal static readonly object EPHEMERON_TOMBSTONE = get_ephemeron_tombstone();

	/// <summary>Gets the maximum number of generations that the system currently supports.</summary>
	/// <returns>A value that ranges from zero to the maximum number of supported generations.</returns>
	/// <filterpriority>1</filterpriority>
	public static int MaxGeneration
	{
		[SecuritySafeCritical]
		get
		{
			return GetMaxGeneration();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int GetCollectionCount(int generation);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int GetMaxGeneration();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void InternalCollect(int generation);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RecordPressure(long bytesAllocated);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void register_ephemeron_array(Ephemeron[] array);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern object get_ephemeron_tombstone();

	/// <summary>Informs the runtime of a large allocation of unmanaged memory that should be taken into account when scheduling garbage collection.</summary>
	/// <param name="bytesAllocated">The incremental amount of unmanaged memory that has been allocated. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bytesAllocated" /> is less than or equal to 0.-or-On a 32-bit computer, <paramref name="bytesAllocated" /> is larger than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void AddMemoryPressure(long bytesAllocated)
	{
		if (bytesAllocated <= 0)
		{
			throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("Positive number required."));
		}
		if (4 == IntPtr.Size && bytesAllocated > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("pressure", Environment.GetResourceString("Value must be non-negative and less than or equal to Int32.MaxValue."));
		}
		RecordPressure(bytesAllocated);
	}

	/// <summary>Informs the runtime that unmanaged memory has been released and no longer needs to be taken into account when scheduling garbage collection.</summary>
	/// <param name="bytesAllocated">The amount of unmanaged memory that has been released. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="bytesAllocated" /> is less than or equal to 0. -or- On a 32-bit computer, <paramref name="bytesAllocated" /> is larger than <see cref="F:System.Int32.MaxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[SecurityCritical]
	public static void RemoveMemoryPressure(long bytesAllocated)
	{
		if (bytesAllocated <= 0)
		{
			throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("Positive number required."));
		}
		if (4 == IntPtr.Size && bytesAllocated > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("Value must be non-negative and less than or equal to Int32.MaxValue."));
		}
		RecordPressure(-bytesAllocated);
	}

	/// <summary>Returns the current generation number of the specified object.</summary>
	/// <returns>The current generation number of <paramref name="obj" />.</returns>
	/// <param name="obj">The object that generation information is retrieved for. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public static extern int GetGeneration(object obj);

	/// <summary>Forces an immediate garbage collection from generation 0 through a specified generation.</summary>
	/// <param name="generation">The number of the oldest generation that garbage collection can be performed on. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="generation" /> is not valid. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Collect(int generation)
	{
		Collect(generation, GCCollectionMode.Default);
	}

	/// <summary>Forces an immediate garbage collection of all generations. </summary>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void Collect()
	{
		InternalCollect(MaxGeneration);
	}

	/// <summary>Forces a garbage collection from generation 0 through a specified generation, at a time specified by a <see cref="T:System.GCCollectionMode" /> value.</summary>
	/// <param name="generation">The number of the oldest generation that garbage collection can be performed on. </param>
	/// <param name="mode">One of the enumeration values that specifies the behavior for the garbage collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="generation" /> is not valid.-or-<paramref name="mode" /> is not one of the <see cref="T:System.GCCollectionMode" /> values.</exception>
	[SecuritySafeCritical]
	public static void Collect(int generation, GCCollectionMode mode)
	{
		Collect(generation, mode, blocking: true);
	}

	/// <summary>Forces a garbage collection from generation 0 through a specified generation, at a time specified by a <see cref="T:System.GCCollectionMode" /> value, with a value specifying whether the collection should be blocking.</summary>
	/// <param name="generation">The number of the oldest generation that garbage collection can be performed on.</param>
	/// <param name="mode">One of the enumeration values that specifies whether the garbage collection is forced or optimized.</param>
	/// <param name="blocking">true to perform a blocking garbage collection; false to perform a background garbage collection where possible.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="generation" /> is not valid.-or-<paramref name="mode" /> is not one of the <see cref="T:System.GCCollectionMode" /> values.</exception>
	[SecuritySafeCritical]
	public static void Collect(int generation, GCCollectionMode mode, bool blocking)
	{
		Collect(generation, mode, blocking, compacting: false);
	}

	[SecuritySafeCritical]
	public static void Collect(int generation, GCCollectionMode mode, bool blocking, bool compacting)
	{
		if (generation < 0)
		{
			throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("Value must be positive."));
		}
		if (mode < GCCollectionMode.Default || mode > GCCollectionMode.Optimized)
		{
			throw new ArgumentOutOfRangeException(Environment.GetResourceString("Enum value was out of legal range."));
		}
		int num = 0;
		if (mode == GCCollectionMode.Optimized)
		{
			num |= 4;
		}
		if (compacting)
		{
			num |= 8;
		}
		if (blocking)
		{
			num |= 2;
		}
		else if (!compacting)
		{
			num |= 1;
		}
		InternalCollect(generation);
	}

	/// <summary>Returns the number of times garbage collection has occurred for the specified generation of objects.</summary>
	/// <returns>The number of times garbage collection has occurred for the specified generation since the process was started.</returns>
	/// <param name="generation">The generation of objects for which the garbage collection count is to be determined. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="generation" /> is less than 0. </exception>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecuritySafeCritical]
	public static int CollectionCount(int generation)
	{
		if (generation < 0)
		{
			throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("Value must be positive."));
		}
		return GetCollectionCount(generation);
	}

	/// <summary>References the specified object, which makes it ineligible for garbage collection from the start of the current routine to the point where this method is called.</summary>
	/// <param name="obj">The object to reference. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void KeepAlive(object obj)
	{
	}

	/// <summary>Returns the current generation number of the target of a specified weak reference.</summary>
	/// <returns>The current generation number of the target of <paramref name="wo" />.</returns>
	/// <param name="wo">A <see cref="T:System.WeakReference" /> that refers to the target object whose generation number is to be determined. </param>
	/// <exception cref="T:System.ArgumentException">Garbage collection has already been performed on <paramref name="wo" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static int GetGeneration(WeakReference wo)
	{
		return GetGeneration(wo.Target ?? throw new ArgumentException());
	}

	/// <summary>Suspends the current thread until the thread that is processing the queue of finalizers has emptied that queue.</summary>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void WaitForPendingFinalizers();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	private static extern void _SuppressFinalize(object o);

	/// <summary>Requests that the system not call the finalizer for the specified object.</summary>
	/// <param name="obj">The object that a finalizer must not be called for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void SuppressFinalize(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		_SuppressFinalize(obj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void _ReRegisterForFinalize(object o);

	/// <summary>Requests that the system call the finalizer for the specified object for which <see cref="M:System.GC.SuppressFinalize(System.Object)" /> has previously been called.</summary>
	/// <param name="obj">The object that a finalizer must be called for. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void ReRegisterForFinalize(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		_ReRegisterForFinalize(obj);
	}

	/// <summary>Retrieves the number of bytes currently thought to be allocated. A parameter indicates whether this method can wait a short interval before returning, to allow the system to collect garbage and finalize objects.</summary>
	/// <returns>A number that is the best available approximation of the number of bytes currently allocated in managed memory.</returns>
	/// <param name="forceFullCollection">true to indicate that this method can wait for garbage collection to occur before returning; otherwise, false.</param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long GetTotalMemory(bool forceFullCollection);

	private static bool _RegisterForFullGCNotification(int maxGenerationPercentage, int largeObjectHeapPercentage)
	{
		throw new NotImplementedException();
	}

	private static bool _CancelFullGCNotification()
	{
		throw new NotImplementedException();
	}

	private static int _WaitForFullGCApproach(int millisecondsTimeout)
	{
		throw new NotImplementedException();
	}

	private static int _WaitForFullGCComplete(int millisecondsTimeout)
	{
		throw new NotImplementedException();
	}

	/// <summary>Specifies that a garbage collection notification should be raised when conditions favor full garbage collection and when the collection has been completed.</summary>
	/// <param name="maxGenerationThreshold">A number between 1 and 99 that specifies when the notification should be raised based on the objects surviving in generation 2. </param>
	/// <param name="largeObjectHeapThreshold">A number between 1 and 99 that specifies when the notification should be raised based on objects allocated in the large object heap. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxGenerationThreshold " />or <paramref name="largeObjectHeapThreshold " />is not between 1 and 99.</exception>
	[SecurityCritical]
	public static void RegisterForFullGCNotification(int maxGenerationThreshold, int largeObjectHeapThreshold)
	{
		if (maxGenerationThreshold <= 0 || maxGenerationThreshold >= 100)
		{
			throw new ArgumentOutOfRangeException("maxGenerationThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument must be between {0} and {1}."), 1, 99));
		}
		if (largeObjectHeapThreshold <= 0 || largeObjectHeapThreshold >= 100)
		{
			throw new ArgumentOutOfRangeException("largeObjectHeapThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument must be between {0} and {1}."), 1, 99));
		}
		if (!_RegisterForFullGCNotification(maxGenerationThreshold, largeObjectHeapThreshold))
		{
			throw new InvalidOperationException(Environment.GetResourceString("This API is not available when the concurrent GC is enabled."));
		}
	}

	/// <summary>Cancels the registration of a garbage collection notification.</summary>
	/// <exception cref="T:System.InvalidOperationException">This member is not available when concurrent garbage collection is enabled. See the &lt;gcConcurrent&gt; runtime setting for information about how to disable concurrent garbage collection.</exception>
	[SecurityCritical]
	public static void CancelFullGCNotification()
	{
		if (!_CancelFullGCNotification())
		{
			throw new InvalidOperationException(Environment.GetResourceString("This API is not available when the concurrent GC is enabled."));
		}
	}

	/// <summary>Returns the status of a registered notification for determining whether a full, blocking garbage collection by the common langauge runtime is imminent.</summary>
	/// <returns>The status of the registered garbage collection notification.</returns>
	[SecurityCritical]
	public static GCNotificationStatus WaitForFullGCApproach()
	{
		return (GCNotificationStatus)_WaitForFullGCApproach(-1);
	}

	/// <summary>Returns, in a specified time-out period, the status of a registered notification for determining whether a full, blocking garbage collection by the common language runtime is imminent.</summary>
	/// <returns>The status of the registered garbage collection notification.</returns>
	/// <param name="millisecondsTimeout">The length of time to wait before a notification status can be obtained. Specify -1 to wait indefinitely.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="millisecondsTimeout" /> must be either non-negative or less than or equal to <see cref="F:System.Int32.MaxValue" /> or -1.</exception>
	[SecurityCritical]
	public static GCNotificationStatus WaitForFullGCApproach(int millisecondsTimeout)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		return (GCNotificationStatus)_WaitForFullGCApproach(millisecondsTimeout);
	}

	/// <summary>Returns the status of a registered notification for determining whether a full, blocking garbage collection by the common language runtime has completed.</summary>
	/// <returns>The status of the registered garbage collection notification.</returns>
	[SecurityCritical]
	public static GCNotificationStatus WaitForFullGCComplete()
	{
		return (GCNotificationStatus)_WaitForFullGCComplete(-1);
	}

	/// <summary>Returns, in a specified time-out period, the status of a registered notification for determining whether a full, blocking garbage collection by common language the runtime has completed.</summary>
	/// <returns>The status of the registered garbage collection notification.</returns>
	/// <param name="millisecondsTimeout">The length of time to wait before a notification status can be obtained. Specify -1 to wait indefinitely.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="millisecondsTimeout" /> must be either non-negative or less than or equal to <see cref="F:System.Int32.MaxValue" /> or -1.</exception>
	[SecurityCritical]
	public static GCNotificationStatus WaitForFullGCComplete(int millisecondsTimeout)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		return (GCNotificationStatus)_WaitForFullGCComplete(millisecondsTimeout);
	}

	[SecurityCritical]
	private static bool StartNoGCRegionWorker(long totalSize, bool hasLohSize, long lohSize, bool disallowFullBlockingGC)
	{
		throw new NotImplementedException();
	}

	[SecurityCritical]
	public static bool TryStartNoGCRegion(long totalSize)
	{
		return StartNoGCRegionWorker(totalSize, hasLohSize: false, 0L, disallowFullBlockingGC: false);
	}

	[SecurityCritical]
	public static bool TryStartNoGCRegion(long totalSize, long lohSize)
	{
		return StartNoGCRegionWorker(totalSize, hasLohSize: true, lohSize, disallowFullBlockingGC: false);
	}

	[SecurityCritical]
	public static bool TryStartNoGCRegion(long totalSize, bool disallowFullBlockingGC)
	{
		return StartNoGCRegionWorker(totalSize, hasLohSize: false, 0L, disallowFullBlockingGC);
	}

	[SecurityCritical]
	public static bool TryStartNoGCRegion(long totalSize, long lohSize, bool disallowFullBlockingGC)
	{
		return StartNoGCRegionWorker(totalSize, hasLohSize: true, lohSize, disallowFullBlockingGC);
	}

	[SecurityCritical]
	private static EndNoGCRegionStatus EndNoGCRegionWorker()
	{
		throw new NotImplementedException();
	}

	[SecurityCritical]
	public static void EndNoGCRegion()
	{
		EndNoGCRegionWorker();
	}
}
