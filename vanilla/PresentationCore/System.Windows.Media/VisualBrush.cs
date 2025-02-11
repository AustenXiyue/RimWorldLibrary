using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Paints an area with a <see cref="P:System.Windows.Media.VisualBrush.Visual" />. </summary>
public sealed class VisualBrush : TileBrush, ICyclicBrush
{
	private DispatcherOperation _DispatcherLayoutResult;

	private bool _pendingLayout;

	private bool _reentrancyFlag;

	private bool _isAsyncRenderRegistered;

	private bool _isCacheDirty = true;

	private Rect _bbox = Rect.Empty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.VisualBrush.Visual" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.VisualBrush.Visual" /> dependency property.</returns>
	public static readonly DependencyProperty VisualProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.VisualBrush.AutoLayoutContent" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.VisualBrush.AutoLayoutContent" /> dependency property.</returns>
	public static readonly DependencyProperty AutoLayoutContentProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const bool c_AutoLayoutContent = true;

	/// <summary>Gets or sets the brush's content. </summary>
	/// <returns>The brush's content. The default is null. </returns>
	public Visual Visual
	{
		get
		{
			return (Visual)GetValue(VisualProperty);
		}
		set
		{
			SetValueInternal(VisualProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies whether this <see cref="T:System.Windows.Media.VisualBrush" /> will run layout its <see cref="P:System.Windows.Media.VisualBrush.Visual" />. </summary>
	/// <returns>true if this Brush should run layout on its <see cref="P:System.Windows.Media.VisualBrush.Visual" />; otherwise, false. The default is true.</returns>
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

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.VisualBrush" /> class.</summary>
	public VisualBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.VisualBrush" /> class that contains the specified <see cref="P:System.Windows.Media.VisualBrush.Visual" />.</summary>
	/// <param name="visual">The contents of the new <see cref="T:System.Windows.Media.VisualBrush" />.</param>
	public VisualBrush(Visual visual)
	{
		if (base.Dispatcher != null)
		{
			MediaSystem.AssertSameContext(this, visual);
			Visual = visual;
		}
	}

	void ICyclicBrush.FireOnChanged()
	{
		if (Enter())
		{
			try
			{
				_isCacheDirty = true;
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
		Visual visual = Visual;
		if (visual != null && visual.CheckFlagsAnd(VisualFlags.NodeIsCyclicBrushRoot))
		{
			visual.Precompute();
			RenderContext renderContext = new RenderContext();
			renderContext.Initialize(channel, DUCE.ResourceHandle.Null);
			if (channel.IsConnected)
			{
				visual.Render(renderContext, 0u);
			}
			else
			{
				((DUCE.IResource)visual).ReleaseOnChannel(channel);
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
		if ((e.IsAValueChange || e.IsASubPropertyChange) && (e.Property == VisualProperty || e.Property == AutoLayoutContentProperty) && AutoLayoutContent && Visual is UIElement uIElement && ((VisualTreeHelper.GetParent(uIElement) == null && !uIElement.IsRootElement) || VisualTreeHelper.GetParent(uIElement) is Visual3D))
		{
			uIElement.LayoutUpdated += OnLayoutUpdated;
			_DispatcherLayoutResult = base.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(LayoutCallback), uIElement);
			_pendingLayout = true;
		}
	}

	private void DoLayout(UIElement element)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(element);
		if (!element.IsRootElement && (parent == null || parent is Visual3D))
		{
			UIElement.PropagateResumeLayout(null, element);
			element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			element.Arrange(new Rect(element.DesiredSize));
		}
	}

	private void OnLayoutUpdated(object sender, EventArgs args)
	{
		UIElement uIElement = (UIElement)Visual;
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

	protected override void GetContentBounds(out Rect contentBounds)
	{
		if (_isCacheDirty)
		{
			_bbox = Visual.CalculateSubgraphBoundsOuterSpace();
			_isCacheDirty = false;
		}
		contentBounds = _bbox;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.VisualBrush" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new VisualBrush Clone()
	{
		return (VisualBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.VisualBrush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new VisualBrush CloneCurrentValue()
	{
		return (VisualBrush)base.CloneCurrentValue();
	}

	private static void VisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		VisualBrush visualBrush = (VisualBrush)d;
		Visual visual = (Visual)e.OldValue;
		if (visualBrush._pendingLayout)
		{
			((UIElement)visual).LayoutUpdated -= visualBrush.OnLayoutUpdated;
			visualBrush._DispatcherLayoutResult.Abort();
			visualBrush._pendingLayout = false;
		}
		Visual visual2 = (Visual)e.NewValue;
		if (visualBrush.Dispatcher != null)
		{
			DUCE.IResource resource = visualBrush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					visualBrush.ReleaseResource(visual, channel);
					visualBrush.AddRefResource(visual2, channel);
				}
			}
		}
		visualBrush.PropertyChanged(VisualProperty);
	}

	private static void AutoLayoutContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((VisualBrush)d).PropertyChanged(AutoLayoutContentProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new VisualBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			Visual visual = Visual;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hVisual = ((DUCE.IResource)visual)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(TileBrush.ViewportProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(TileBrush.ViewboxProperty, channel);
			DUCE.MILCMD_VISUALBRUSH mILCMD_VISUALBRUSH = default(DUCE.MILCMD_VISUALBRUSH);
			mILCMD_VISUALBRUSH.Type = MILCMD.MilCmdVisualBrush;
			mILCMD_VISUALBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_VISUALBRUSH.Opacity = base.Opacity;
			}
			mILCMD_VISUALBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_VISUALBRUSH.hTransform = hTransform;
			mILCMD_VISUALBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_VISUALBRUSH.ViewportUnits = base.ViewportUnits;
			mILCMD_VISUALBRUSH.ViewboxUnits = base.ViewboxUnits;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_VISUALBRUSH.Viewport = base.Viewport;
			}
			mILCMD_VISUALBRUSH.hViewportAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_VISUALBRUSH.Viewbox = base.Viewbox;
			}
			mILCMD_VISUALBRUSH.hViewboxAnimations = animationResourceHandle3;
			mILCMD_VISUALBRUSH.Stretch = base.Stretch;
			mILCMD_VISUALBRUSH.TileMode = base.TileMode;
			mILCMD_VISUALBRUSH.AlignmentX = base.AlignmentX;
			mILCMD_VISUALBRUSH.AlignmentY = base.AlignmentY;
			mILCMD_VISUALBRUSH.CachingHint = (CachingHint)GetValue(RenderOptions.CachingHintProperty);
			mILCMD_VISUALBRUSH.CacheInvalidationThresholdMinimum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMinimumProperty);
			mILCMD_VISUALBRUSH.CacheInvalidationThresholdMaximum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMaximumProperty);
			mILCMD_VISUALBRUSH.hVisual = hVisual;
			channel.SendCommand((byte*)(&mILCMD_VISUALBRUSH), sizeof(DUCE.MILCMD_VISUALBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VISUALBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
			Visual?.AddRefOnChannelForCyclicBrush(this, channel);
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
			Visual?.ReleaseOnChannelForCyclicBrush(this, channel);
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

	static VisualBrush()
	{
		Type typeFromHandle = typeof(VisualBrush);
		VisualProperty = Animatable.RegisterProperty("Visual", typeof(Visual), typeFromHandle, null, VisualPropertyChanged, null, isIndependentlyAnimated: false, null);
		AutoLayoutContentProperty = Animatable.RegisterProperty("AutoLayoutContent", typeof(bool), typeFromHandle, true, AutoLayoutContentPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
