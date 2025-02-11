using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Paints an area with cached content.</summary>
public sealed class BitmapCacheBrush : Brush, ICyclicBrush
{
	private ContainerVisual _dummyVisual;

	private DispatcherOperation _DispatcherLayoutResult;

	private bool _pendingLayout;

	private bool _reentrancyFlag;

	private bool _isAsyncRenderRegistered;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCacheBrush.Target" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCacheBrush.Target" /> dependency property.</returns>
	public static readonly DependencyProperty TargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCacheBrush.BitmapCache" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCacheBrush.BitmapCache" /> dependency property.</returns>
	public static readonly DependencyProperty BitmapCacheProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.BitmapCacheBrush.AutoLayoutContent" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.BitmapCacheBrush.AutoLayoutContent" /> dependency property.</returns>
	public static readonly DependencyProperty AutoLayoutContentProperty;

	internal static readonly DependencyProperty InternalTargetProperty;

	internal static readonly DependencyProperty AutoWrapTargetProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const bool c_AutoLayoutContent = true;

	internal const bool c_AutoWrapTarget = false;

	private ContainerVisual AutoWrapVisual
	{
		get
		{
			if (_dummyVisual == null)
			{
				_dummyVisual = new ContainerVisual();
			}
			return _dummyVisual;
		}
	}

	/// <summary>Gets or sets the target visual to cache. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Visual" /> to cache and paint with. </returns>
	public Visual Target
	{
		get
		{
			return (Visual)GetValue(TargetProperty);
		}
		set
		{
			SetValueInternal(TargetProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.CacheMode" /> that represents cached content. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.BitmapCache" /> that represents cached content.</returns>
	public BitmapCache BitmapCache
	{
		get
		{
			return (BitmapCache)GetValue(BitmapCacheProperty);
		}
		set
		{
			SetValueInternal(BitmapCacheProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether layout is applied to the contents of this brush. </summary>
	/// <returns>true if layout is applied; otherwise, false. The default is false.</returns>
	public bool AutoLayoutContent
	{
		get
		{
			return (bool)GetValue(AutoLayoutContentProperty);
		}
		set
		{
			SetValueInternal(AutoLayoutContentProperty, BooleanBoxes.Box(value));
		}
	}

	internal Visual InternalTarget
	{
		get
		{
			return (Visual)GetValue(InternalTargetProperty);
		}
		set
		{
			SetValueInternal(InternalTargetProperty, value);
		}
	}

	internal bool AutoWrapTarget
	{
		get
		{
			return (bool)GetValue(AutoWrapTargetProperty);
		}
		set
		{
			SetValueInternal(AutoWrapTargetProperty, BooleanBoxes.Box(value));
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BitmapCacheBrush" /> class. </summary>
	public BitmapCacheBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.BitmapCacheBrush" /> class with the specified <see cref="T:System.Windows.Media.Visual" />. </summary>
	/// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> to cache and use as the <see cref="P:System.Windows.Media.BitmapCacheBrush.Target" />.</param>
	public BitmapCacheBrush(Visual visual)
	{
		if (base.Dispatcher != null)
		{
			MediaSystem.AssertSameContext(this, visual);
			Target = visual;
		}
	}

	void ICyclicBrush.FireOnChanged()
	{
		if (Enter())
		{
			try
			{
				FireChanged();
				RegisterForAsyncRenderForCyclicBrush();
			}
			finally
			{
				Exit();
			}
		}
	}

	private void RegisterForAsyncRenderForCyclicBrush()
	{
		if (this != null && base.Dispatcher != null && !_isAsyncRenderRegistered)
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			if (!((DUCE.IResource)this).GetHandle(mediaContext.Channel).IsNull)
			{
				mediaContext.ResourcesUpdated += ((ICyclicBrush)this).RenderForCyclicBrush;
				_isAsyncRenderRegistered = true;
			}
		}
	}

	void ICyclicBrush.RenderForCyclicBrush(DUCE.Channel channel, bool skipChannelCheck)
	{
		Visual internalTarget = InternalTarget;
		if (internalTarget != null && internalTarget.CheckFlagsAnd(VisualFlags.NodeIsCyclicBrushRoot))
		{
			internalTarget.Precompute();
			RenderContext renderContext = new RenderContext();
			renderContext.Initialize(channel, DUCE.ResourceHandle.Null);
			if (channel.IsConnected)
			{
				internalTarget.Render(renderContext, 0u);
			}
			else
			{
				((DUCE.IResource)internalTarget).ReleaseOnChannel(channel);
			}
		}
		_isAsyncRenderRegistered = false;
	}

	internal void AddRefResource(Visual visual, DUCE.Channel channel)
	{
		visual?.AddRefOnChannelForCyclicBrush(this, channel);
	}

	internal void ReleaseResource(Visual visual, DUCE.Channel channel)
	{
		visual?.ReleaseOnChannelForCyclicBrush(this, channel);
	}

	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (!e.IsAValueChange && !e.IsASubPropertyChange)
		{
			return;
		}
		if (e.Property == TargetProperty || e.Property == AutoLayoutContentProperty)
		{
			if (e.Property == TargetProperty && e.IsAValueChange)
			{
				if (AutoWrapTarget)
				{
					AutoWrapVisual.Children.Remove((Visual)e.OldValue);
					AutoWrapVisual.Children.Add((Visual)e.NewValue);
				}
				else
				{
					InternalTarget = Target;
				}
			}
			if (AutoLayoutContent && Target is UIElement uIElement && ((VisualTreeHelper.GetParent(uIElement) == null && !uIElement.IsRootElement) || VisualTreeHelper.GetParent(uIElement) is Visual3D || VisualTreeHelper.GetParent(uIElement) == InternalTarget))
			{
				uIElement.LayoutUpdated += OnLayoutUpdated;
				_DispatcherLayoutResult = base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(LayoutCallback), uIElement);
				_pendingLayout = true;
			}
		}
		else if (e.Property == AutoWrapTargetProperty)
		{
			if (AutoWrapTarget)
			{
				InternalTarget = AutoWrapVisual;
				AutoWrapVisual.Children.Add(Target);
			}
			else
			{
				AutoWrapVisual.Children.Remove(Target);
				InternalTarget = Target;
			}
		}
	}

	private void DoLayout(UIElement element)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(element);
		if (!element.IsRootElement && (parent == null || parent is Visual3D || parent == InternalTarget))
		{
			UIElement.PropagateResumeLayout(null, element);
			element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			element.Arrange(new Rect(element.DesiredSize));
		}
	}

	private void OnLayoutUpdated(object sender, EventArgs args)
	{
		UIElement uIElement = (UIElement)Target;
		uIElement.LayoutUpdated -= OnLayoutUpdated;
		_pendingLayout = false;
		_DispatcherLayoutResult.Abort();
		DoLayout(uIElement);
	}

	private object LayoutCallback(object arg)
	{
		UIElement uIElement = arg as UIElement;
		uIElement.LayoutUpdated -= OnLayoutUpdated;
		_pendingLayout = false;
		DoLayout(uIElement);
		return null;
	}

	internal bool Enter()
	{
		if (_reentrancyFlag)
		{
			return false;
		}
		_reentrancyFlag = true;
		return true;
	}

	internal void Exit()
	{
		_reentrancyFlag = false;
	}

	private static object CoerceOpacity(DependencyObject d, object value)
	{
		if ((double)value != (double)Brush.OpacityProperty.GetDefaultValue(typeof(BitmapCacheBrush)))
		{
			throw new InvalidOperationException(SR.BitmapCacheBrush_OpacityChanged);
		}
		return 1.0;
	}

	private static object CoerceTransform(DependencyObject d, object value)
	{
		if ((Transform)value != (Transform)Brush.TransformProperty.GetDefaultValue(typeof(BitmapCacheBrush)))
		{
			throw new InvalidOperationException(SR.BitmapCacheBrush_TransformChanged);
		}
		return null;
	}

	private static object CoerceRelativeTransform(DependencyObject d, object value)
	{
		if ((Transform)value != (Transform)Brush.RelativeTransformProperty.GetDefaultValue(typeof(BitmapCacheBrush)))
		{
			throw new InvalidOperationException(SR.BitmapCacheBrush_RelativeTransformChanged);
		}
		return null;
	}

	private static void StaticInitialize(Type typeofThis)
	{
		Brush.OpacityProperty.OverrideMetadata(typeofThis, new IndependentlyAnimatedPropertyMetadata(1.0, null, CoerceOpacity));
		Brush.TransformProperty.OverrideMetadata(typeofThis, new UIPropertyMetadata(null, null, CoerceTransform));
		Brush.RelativeTransformProperty.OverrideMetadata(typeofThis, new UIPropertyMetadata(null, null, CoerceRelativeTransform));
	}

	/// <summary>Creates a modifiable clone of the <see cref="T:System.Windows.Media.BitmapCacheBrush" />, making deep copies of the object's values. When copying the object's dependency properties, this method copies expressions (which might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapCacheBrush Clone()
	{
		return (BitmapCacheBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone (deep copy) of the <see cref="T:System.Windows.Media.BitmapCacheBrush" /> using its current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new BitmapCacheBrush CloneCurrentValue()
	{
		return (BitmapCacheBrush)base.CloneCurrentValue();
	}

	private static void TargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCacheBrush)d).PropertyChanged(TargetProperty);
	}

	private static void BitmapCachePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		BitmapCacheBrush bitmapCacheBrush = (BitmapCacheBrush)d;
		BitmapCache resource = (BitmapCache)e.OldValue;
		BitmapCache resource2 = (BitmapCache)e.NewValue;
		if (bitmapCacheBrush.Dispatcher != null)
		{
			DUCE.IResource resource3 = bitmapCacheBrush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					bitmapCacheBrush.ReleaseResource(resource, channel);
					bitmapCacheBrush.AddRefResource(resource2, channel);
				}
			}
		}
		bitmapCacheBrush.PropertyChanged(BitmapCacheProperty);
	}

	private static void AutoLayoutContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCacheBrush)d).PropertyChanged(AutoLayoutContentProperty);
	}

	private static void InternalTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BitmapCacheBrush bitmapCacheBrush = (BitmapCacheBrush)d;
		Visual visual = (Visual)e.OldValue;
		if (bitmapCacheBrush._pendingLayout)
		{
			((UIElement)visual).LayoutUpdated -= bitmapCacheBrush.OnLayoutUpdated;
			bitmapCacheBrush._DispatcherLayoutResult.Abort();
			bitmapCacheBrush._pendingLayout = false;
		}
		Visual visual2 = (Visual)e.NewValue;
		if (bitmapCacheBrush.Dispatcher != null)
		{
			DUCE.IResource resource = bitmapCacheBrush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					bitmapCacheBrush.ReleaseResource(visual, channel);
					bitmapCacheBrush.AddRefResource(visual2, channel);
				}
			}
		}
		bitmapCacheBrush.PropertyChanged(InternalTargetProperty);
	}

	private static void AutoWrapTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapCacheBrush)d).PropertyChanged(AutoWrapTargetProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapCacheBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			BitmapCache bitmapCache = BitmapCache;
			Visual internalTarget = InternalTarget;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hBitmapCache = ((DUCE.IResource)bitmapCache)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hInternalTarget = ((DUCE.IResource)internalTarget)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.MILCMD_BITMAPCACHEBRUSH mILCMD_BITMAPCACHEBRUSH = default(DUCE.MILCMD_BITMAPCACHEBRUSH);
			mILCMD_BITMAPCACHEBRUSH.Type = MILCMD.MilCmdBitmapCacheBrush;
			mILCMD_BITMAPCACHEBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_BITMAPCACHEBRUSH.Opacity = base.Opacity;
			}
			mILCMD_BITMAPCACHEBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_BITMAPCACHEBRUSH.hTransform = hTransform;
			mILCMD_BITMAPCACHEBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_BITMAPCACHEBRUSH.hBitmapCache = hBitmapCache;
			mILCMD_BITMAPCACHEBRUSH.hInternalTarget = hInternalTarget;
			channel.SendCommand((byte*)(&mILCMD_BITMAPCACHEBRUSH), sizeof(DUCE.MILCMD_BITMAPCACHEBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_BITMAPCACHEBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
			((DUCE.IResource)BitmapCache)?.AddRefOnChannel(channel);
			InternalTarget?.AddRefOnChannelForCyclicBrush(this, channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)BitmapCache)?.ReleaseOnChannel(channel);
			InternalTarget?.ReleaseOnChannelForCyclicBrush(this, channel);
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

	static BitmapCacheBrush()
	{
		Type typeFromHandle = typeof(BitmapCacheBrush);
		StaticInitialize(typeFromHandle);
		TargetProperty = Animatable.RegisterProperty("Target", typeof(Visual), typeFromHandle, null, TargetPropertyChanged, null, isIndependentlyAnimated: false, null);
		BitmapCacheProperty = Animatable.RegisterProperty("BitmapCache", typeof(BitmapCache), typeFromHandle, null, BitmapCachePropertyChanged, null, isIndependentlyAnimated: false, null);
		AutoLayoutContentProperty = Animatable.RegisterProperty("AutoLayoutContent", typeof(bool), typeFromHandle, true, AutoLayoutContentPropertyChanged, null, isIndependentlyAnimated: false, null);
		InternalTargetProperty = Animatable.RegisterProperty("InternalTarget", typeof(Visual), typeFromHandle, null, InternalTargetPropertyChanged, null, isIndependentlyAnimated: false, null);
		AutoWrapTargetProperty = Animatable.RegisterProperty("AutoWrapTarget", typeof(bool), typeFromHandle, false, AutoWrapTargetPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
