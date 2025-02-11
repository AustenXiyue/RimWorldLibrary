using System.ComponentModel;
using System.Windows.Interop;
using MS.Win32.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides options for controlling the rendering behavior of objects.</summary>
public static class RenderOptions
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property. </returns>
	public static readonly DependencyProperty EdgeModeProperty = DependencyProperty.RegisterAttached("EdgeMode", typeof(EdgeMode), typeof(RenderOptions), new UIPropertyMetadata(EdgeMode.Unspecified), ValidateEnums.IsEdgeModeValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property.</returns>
	public static readonly DependencyProperty BitmapScalingModeProperty = DependencyProperty.RegisterAttached("BitmapScalingMode", typeof(BitmapScalingMode), typeof(RenderOptions), new UIPropertyMetadata(BitmapScalingMode.Unspecified), ValidateEnums.IsBitmapScalingModeValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property.</returns>
	public static readonly DependencyProperty ClearTypeHintProperty = DependencyProperty.RegisterAttached("ClearTypeHint", typeof(ClearTypeHint), typeof(RenderOptions), new UIPropertyMetadata(ClearTypeHint.Auto), ValidateEnums.IsClearTypeHintValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property.</returns>
	public static readonly DependencyProperty CachingHintProperty = DependencyProperty.RegisterAttached("CachingHint", typeof(CachingHint), typeof(RenderOptions), new UIPropertyMetadata(CachingHint.Unspecified), ValidateEnums.IsCachingHintValid);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property. </returns>
	public static readonly DependencyProperty CacheInvalidationThresholdMinimumProperty = DependencyProperty.RegisterAttached("CacheInvalidationThresholdMinimum", typeof(double), typeof(RenderOptions), new UIPropertyMetadata(0.707), null);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property.</returns>
	public static readonly DependencyProperty CacheInvalidationThresholdMaximumProperty = DependencyProperty.RegisterAttached("CacheInvalidationThresholdMaximum", typeof(double), typeof(RenderOptions), new UIPropertyMetadata(1.414), null);

	/// <summary>Specifies the render mode preference for the current process. </summary>
	/// <returns>The <see cref="T:System.Windows.Interop.RenderMode" /> preference for the current process. </returns>
	public static RenderMode ProcessRenderMode
	{
		get
		{
			if (!UnsafeNativeMethods.MilCoreApi.RenderOptions_IsSoftwareRenderingForcedForProcess())
			{
				return RenderMode.Default;
			}
			return RenderMode.SoftwareOnly;
		}
		set
		{
			if (value != 0 && value != RenderMode.SoftwareOnly)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(RenderMode));
			}
			UnsafeNativeMethods.MilCoreApi.RenderOptions_ForceSoftwareRenderingModeForProcess(value == RenderMode.SoftwareOnly);
		}
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property on the specified dependency object.</returns>
	/// <param name="target">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static EdgeMode GetEdgeMode(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (EdgeMode)target.GetValue(EdgeModeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> attached property on a specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the value of the <see cref="P:System.Windows.Media.RenderOptions.EdgeMode" /> property</param>
	/// <param name="edgeMode">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetEdgeMode(DependencyObject target, EdgeMode edgeMode)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(EdgeModeProperty, edgeMode);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property on the specified dependency object.</returns>
	/// <param name="target">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static BitmapScalingMode GetBitmapScalingMode(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (BitmapScalingMode)target.GetValue(BitmapScalingModeProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> attached property on a specified dependency object.</summary>
	/// <param name="target">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.Media.DrawingGroup" /> descendant on which to set the value of the <see cref="P:System.Windows.Media.RenderOptions.BitmapScalingMode" /> property.</param>
	/// <param name="bitmapScalingMode">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetBitmapScalingMode(DependencyObject target, BitmapScalingMode bitmapScalingMode)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(BitmapScalingModeProperty, bitmapScalingMode);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property of the specified element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property for <paramref name="target" />.</returns>
	/// <param name="target">The <see cref="T:System.Windows.DependencyObject" /> to retrieve the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property for. </param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static ClearTypeHint GetClearTypeHint(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (ClearTypeHint)target.GetValue(ClearTypeHintProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property of the specified element.</summary>
	/// <param name="target">The <see cref="T:System.Windows.DependencyObject" /> to set the <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> attached property on.</param>
	/// <param name="clearTypeHint">The new <see cref="P:System.Windows.Media.RenderOptions.ClearTypeHint" /> value. </param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetClearTypeHint(DependencyObject target, ClearTypeHint clearTypeHint)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(ClearTypeHintProperty, clearTypeHint);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property on the specified dependency object.</returns>
	/// <param name="target">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(TileBrush))]
	public static CachingHint GetCachingHint(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (CachingHint)target.GetValue(CachingHintProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> attached property on a specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the value of the <see cref="P:System.Windows.Media.RenderOptions.CachingHint" /> property.</param>
	/// <param name="cachingHint">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetCachingHint(DependencyObject target, CachingHint cachingHint)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(CachingHintProperty, cachingHint);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property on the specified dependency object.</returns>
	/// <param name="target">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(TileBrush))]
	public static double GetCacheInvalidationThresholdMinimum(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (double)target.GetValue(CacheInvalidationThresholdMinimumProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> attached property on a specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMinimum" /> property</param>
	/// <param name="cacheInvalidationThresholdMinimum">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetCacheInvalidationThresholdMinimum(DependencyObject target, double cacheInvalidationThresholdMinimum)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(CacheInvalidationThresholdMinimumProperty, cacheInvalidationThresholdMinimum);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property for a specified dependency object.</summary>
	/// <returns>The current value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property on the specified dependency object.</returns>
	/// <param name="target">The dependency object from which to retrieve the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(TileBrush))]
	public static double GetCacheInvalidationThresholdMaximum(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (double)target.GetValue(CacheInvalidationThresholdMaximumProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> attached property on a specified dependency object.</summary>
	/// <param name="target">The dependency object on which to set the value of the <see cref="P:System.Windows.Media.RenderOptions.CacheInvalidationThresholdMaximum" /> property</param>
	/// <param name="cacheInvalidationThresholdMaximum">The new value to set the property to.</param>
	/// <exception cref="T:System.ArgumentNullException">The specified <paramref name="target" /> is null.</exception>
	public static void SetCacheInvalidationThresholdMaximum(DependencyObject target, double cacheInvalidationThresholdMaximum)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(CacheInvalidationThresholdMaximumProperty, cacheInvalidationThresholdMaximum);
	}
}
