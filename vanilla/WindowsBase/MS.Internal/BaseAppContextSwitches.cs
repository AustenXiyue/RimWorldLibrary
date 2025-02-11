using System;
using System.Runtime.CompilerServices;

namespace MS.Internal;

internal static class BaseAppContextSwitches
{
	internal const string SwitchDoNotUseCulturePreservingDispatcherOperations = "Switch.MS.Internal.DoNotUseCulturePreservingDispatcherOperations";

	private static int _doNotUseCulturePreservingDispatcherOperations;

	internal const string SwitchUseSha1AsDefaultHashAlgorithmForDigitalSignatures = "Switch.MS.Internal.UseSha1AsDefaultHashAlgorithmForDigitalSignatures";

	private static int _useSha1AsDefaultHashAlgorithmForDigitalSignatures;

	internal const string SwitchDoNotInvokeInWeakEventTableShutdownListener = "Switch.MS.Internal.DoNotInvokeInWeakEventTableShutdownListener";

	private static int _doNotInvokeInWeakEventTableShutdownListener;

	internal const string SwitchEnableWeakEventMemoryImprovements = "Switch.MS.Internal.EnableWeakEventMemoryImprovements";

	private static int _enableWeakEventMemoryImprovements;

	internal const string SwitchEnableCleanupSchedulingImprovements = "Switch.MS.Internal.EnableCleanupSchedulingImprovements";

	private static int _enableCleanupSchedulingImprovements;

	public static bool DoNotUseCulturePreservingDispatcherOperations
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.DoNotUseCulturePreservingDispatcherOperations", ref _doNotUseCulturePreservingDispatcherOperations);
		}
	}

	public static bool UseSha1AsDefaultHashAlgorithmForDigitalSignatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.UseSha1AsDefaultHashAlgorithmForDigitalSignatures", ref _useSha1AsDefaultHashAlgorithmForDigitalSignatures);
		}
	}

	public static bool DoNotInvokeInWeakEventTableShutdownListener
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.DoNotInvokeInWeakEventTableShutdownListener", ref _doNotInvokeInWeakEventTableShutdownListener);
		}
	}

	public static bool EnableWeakEventMemoryImprovements
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.EnableWeakEventMemoryImprovements", ref _enableWeakEventMemoryImprovements);
		}
	}

	public static bool EnableCleanupSchedulingImprovements
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.MS.Internal.EnableCleanupSchedulingImprovements", ref _enableCleanupSchedulingImprovements);
		}
	}
}
