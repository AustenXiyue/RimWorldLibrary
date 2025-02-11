using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MS.Internal;

internal static class CoreAppContextSwitches
{
	internal const string DoNotScaleForDpiChangesSwitchName = "Switch.System.Windows.DoNotScaleForDpiChanges";

	private static int _doNotScaleForDpiChanges;

	internal const string DisableStylusAndTouchSupportSwitchName = "Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport";

	private static int _disableStylusAndTouchSupport;

	internal const string EnablePointerSupportSwitchName = "Switch.System.Windows.Input.Stylus.EnablePointerSupport";

	private static int _enablePointerSupport;

	internal const string OverrideExceptionWithNullReferenceExceptionName = "Switch.System.Windows.Media.ImageSourceConverter.OverrideExceptionWithNullReferenceException";

	private static int _overrideExceptionWithNullReferenceException;

	internal const string DisableDiagnosticsSwitchName = "Switch.System.Windows.Diagnostics.DisableDiagnostics";

	private static int _disableDiagnostics;

	internal const string AllowChangesDuringVisualTreeChangedSwitchName = "Switch.System.Windows.Diagnostics.AllowChangesDuringVisualTreeChanged";

	private static int _allowChangesDuringVisualTreeChanged;

	internal const string DisableImplicitTouchKeyboardInvocationSwitchName = "Switch.System.Windows.Input.Stylus.DisableImplicitTouchKeyboardInvocation";

	private static int _disableImplicitTouchKeyboardInvocation;

	internal const string ShouldRenderEvenWhenNoDisplayDevicesAreAvailableSwitchName = "Switch.System.Windows.Media.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable";

	private static int _shouldRenderEvenWhenNoDisplayDevicesAreAvailable;

	internal const string ShouldNotRenderInNonInteractiveWindowStationSwitchName = "Switch.System.Windows.Media.ShouldNotRenderInNonInteractiveWindowStation";

	private static int _shouldNotRenderInNonInteractiveWindowStation;

	internal const string DoNotUsePresentationDpiCapabilityTier2OrGreaterSwitchName = "Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater";

	private static int _doNotUsePresentationDpiCapabilityTier2OrGreater;

	internal const string DoNotUsePresentationDpiCapabilityTier3OrGreaterSwitchName = "Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier3OrGreater";

	private static int _doNotUsePresentationDpiCapabilityTier3OrGreater;

	internal const string AllowExternalProcessToBlockAccessToTemporaryFilesSwitchName = "Switch.System.Windows.AllowExternalProcessToBlockAccessToTemporaryFiles";

	private static int _allowExternalProcessToBlockAccessToTemporaryFiles;

	internal const string DisableDirtyRectanglesSwitchName = "Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles";

	private static int _DisableDirtyRectangles;

	internal const string EnableDynamicDirtyRectanglesSwitchName = "Switch.System.Windows.Media.MediaContext.EnableDynamicDirtyRectangles";

	private static int _EnableDynamicDirtyRectangles;

	internal const string EnableHardwareAccelerationInRdpSwitchName = "Switch.System.Windows.Media.EnableHardwareAccelerationInRdp";

	private static int _enableHardwareAccelerationInRdp;

	public static bool DoNotScaleForDpiChanges
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.DoNotScaleForDpiChanges", ref _doNotScaleForDpiChanges);
		}
	}

	public static bool DisableStylusAndTouchSupport
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", ref _disableStylusAndTouchSupport);
		}
	}

	public static bool EnablePointerSupport
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Input.Stylus.EnablePointerSupport", ref _enablePointerSupport);
		}
	}

	public static bool OverrideExceptionWithNullReferenceException
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.ImageSourceConverter.OverrideExceptionWithNullReferenceException", ref _overrideExceptionWithNullReferenceException);
		}
	}

	public static bool DisableDiagnostics
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Diagnostics.DisableDiagnostics", ref _disableDiagnostics);
		}
	}

	public static bool AllowChangesDuringVisualTreeChanged
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Diagnostics.AllowChangesDuringVisualTreeChanged", ref _allowChangesDuringVisualTreeChanged);
		}
	}

	public static bool DisableImplicitTouchKeyboardInvocation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Input.Stylus.DisableImplicitTouchKeyboardInvocation", ref _disableImplicitTouchKeyboardInvocation);
		}
	}

	public static bool UseNetFx47CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures;
		}
	}

	public static bool UseNetFx471CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return AccessibilitySwitches.UseNetFx471CompatibleAccessibilityFeatures;
		}
	}

	public static bool UseNetFx472CompatibleAccessibilityFeatures
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures;
		}
	}

	public static bool ShouldRenderEvenWhenNoDisplayDevicesAreAvailable
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable", ref _shouldRenderEvenWhenNoDisplayDevicesAreAvailable);
		}
	}

	public static bool ShouldNotRenderInNonInteractiveWindowStation => LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.ShouldNotRenderInNonInteractiveWindowStation", ref _shouldNotRenderInNonInteractiveWindowStation);

	public static bool DoNotUsePresentationDpiCapabilityTier2OrGreater
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater", ref _doNotUsePresentationDpiCapabilityTier2OrGreater);
		}
	}

	public static bool DoNotUsePresentationDpiCapabilityTier3OrGreater
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier3OrGreater", ref _doNotUsePresentationDpiCapabilityTier3OrGreater);
		}
	}

	public static bool AllowExternalProcessToBlockAccessToTemporaryFiles
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.AllowExternalProcessToBlockAccessToTemporaryFiles", ref _allowExternalProcessToBlockAccessToTemporaryFiles);
		}
	}

	public static bool DisableDirtyRectangles
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (EnableDynamicDirtyRectangles)
			{
				AppContext.TryGetSwitch("Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles", out var isEnabled);
				return isEnabled;
			}
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles", ref _DisableDirtyRectangles);
		}
	}

	public static bool EnableDynamicDirtyRectangles
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.MediaContext.EnableDynamicDirtyRectangles", ref _EnableDynamicDirtyRectangles);
		}
	}

	public static bool EnableHardwareAccelerationInRdp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return LocalAppContext.GetCachedSwitchValue("Switch.System.Windows.Media.EnableHardwareAccelerationInRdp", ref _enableHardwareAccelerationInRdp);
		}
	}
}
