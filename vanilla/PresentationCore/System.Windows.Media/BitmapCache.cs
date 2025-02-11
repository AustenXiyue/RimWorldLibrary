using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Creates and caches a bitmap representation of a <see cref="T:System.Windows.UIElement" />.</summary>
public sealed class BitmapCache : CacheMode
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCache.RenderAtScale" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCache.RenderAtScale" /> dependency property.</returns>
	public static readonly DependencyProperty RenderAtScaleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCache.SnapsToDevicePixels" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCache.SnapsToDevicePixels" /> dependency property.</returns>
	public static readonly DependencyProperty SnapsToDevicePixelsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCache.EnableClearType" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCache.EnableClearType" /> dependency property.</returns>
	public static readonly DependencyProperty EnableClearTypeProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_RenderAtScale = 1.0;

	internal const bool c_SnapsToDevicePixels = false;

	internal const bool c_EnableClearType = false;

	/// <summary>Gets or sets a value that indicates the scale that is applied to the bitmap. </summary>
	/// <returns>The scale that is applied to the bitmap. The default is 1.</returns>
	public double RenderAtScale
	{
		get
		{
			return (double)GetValue(RenderAtScaleProperty);
		}
		set
		{
			SetValueInternal(RenderAtScaleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the bitmap is rendered with pixel snapping. </summary>
	/// <returns>true if pixel snapping is active; otherwise, false. The default is false.</returns>
	public bool SnapsToDevicePixels
	{
		get
		{
			return (bool)GetValue(SnapsToDevicePixelsProperty);
		}
		set
		{
			SetValueInternal(SnapsToDevicePixelsProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the bitmap is rendered with ClearType activated. </summary>
	/// <returns>true if ClearType is active; otherwise, false. The default is false.</returns>
	public bool EnableClearType
	{
		get
		{
			return (bool)GetValue(EnableClearTypeProperty);
		}
		set
		{
			SetValueInternal(EnableClearTypeProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BitmapCache" /> class. </summary>
	public BitmapCache()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BitmapCache" /> class with the specified scale. </summary>
	/// <param name="renderAtScale">A double that scales the bitmap. </param>
	public BitmapCache(double renderAtScale)
	{
		RenderAtScale = renderAtScale;
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Media.BitmapCache" />, making deep copies of the object's values. When copying the object's dependency properties, this method copies expressions (which might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapCache Clone()
	{
		return (BitmapCache)base.Clone();
	}

	/// <summary>Creates a modifiable clone (deep copy) of the <see cref="T:System.Windows.Media.BitmapCache" /> using its current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapCache CloneCurrentValue()
	{
		return (BitmapCache)base.CloneCurrentValue();
	}

	private static void RenderAtScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCache)d).PropertyChanged(RenderAtScaleProperty);
	}

	private static void SnapsToDevicePixelsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCache)d).PropertyChanged(SnapsToDevicePixelsProperty);
	}

	private static void EnableClearTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCache)d).PropertyChanged(EnableClearTypeProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapCache();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(RenderAtScaleProperty, channel);
			DUCE.MILCMD_BITMAPCACHE mILCMD_BITMAPCACHE = default(DUCE.MILCMD_BITMAPCACHE);
			mILCMD_BITMAPCACHE.Type = MILCMD.MilCmdBitmapCache;
			mILCMD_BITMAPCACHE.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_BITMAPCACHE.RenderAtScale = RenderAtScale;
			}
			mILCMD_BITMAPCACHE.hRenderAtScaleAnimations = animationResourceHandle;
			mILCMD_BITMAPCACHE.SnapsToDevicePixels = CompositionResourceManager.BooleanToUInt32(SnapsToDevicePixels);
			mILCMD_BITMAPCACHE.EnableClearType = CompositionResourceManager.BooleanToUInt32(EnableClearType);
			channel.SendCommand((byte*)(&mILCMD_BITMAPCACHE), sizeof(DUCE.MILCMD_BITMAPCACHE));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_BITMAPCACHE))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static BitmapCache()
	{
		Type typeFromHandle = typeof(BitmapCache);
		RenderAtScaleProperty = Animatable.RegisterProperty("RenderAtScale", typeof(double), typeFromHandle, 1.0, RenderAtScalePropertyChanged, null, isIndependentlyAnimated: true, null);
		SnapsToDevicePixelsProperty = Animatable.RegisterProperty("SnapsToDevicePixels", typeof(bool), typeFromHandle, false, SnapsToDevicePixelsPropertyChanged, null, isIndependentlyAnimated: false, null);
		EnableClearTypeProperty = Animatable.RegisterProperty("EnableClearType", typeof(bool), typeFromHandle, false, EnableClearTypePropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
