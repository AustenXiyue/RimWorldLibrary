using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a collection of drawings that can be operated upon as a single drawing. </summary>
[ContentProperty("Children")]
public sealed class DrawingGroup : Drawing
{
	private bool _openedForAppend;

	private bool _open;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.ClipGeometry" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.ClipGeometry" /> dependency property.</returns>
	public static readonly DependencyProperty ClipGeometryProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.Opacity" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.Opacity" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.OpacityMask" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.OpacityMask" /> dependency property.</returns>
	public static readonly DependencyProperty OpacityMaskProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.Transform" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.Transform" /> dependency property.</returns>
	public static readonly DependencyProperty TransformProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.GuidelineSet" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.GuidelineSet" /> dependency property.</returns>
	public static readonly DependencyProperty GuidelineSetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" /> dependency property.</returns>
	public static readonly DependencyProperty BitmapEffectProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffectInput" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffectInput" /> dependency property.</returns>
	public static readonly DependencyProperty BitmapEffectInputProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static DrawingCollection s_Children;

	internal const double c_Opacity = 1.0;

	internal const EdgeMode c_EdgeMode = EdgeMode.Unspecified;

	internal const BitmapScalingMode c_BitmapScalingMode = BitmapScalingMode.Unspecified;

	internal const ClearTypeHint c_ClearTypeHint = ClearTypeHint.Auto;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Drawing" /> objects that are contained in this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>A collection of the <see cref="T:System.Windows.Media.Drawing" /> objects in this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is an empty <see cref="T:System.Windows.Media.DrawingCollection" />.</returns>
	public DrawingCollection Children
	{
		get
		{
			return (DrawingCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	/// <summary>Gets or sets the clip region of this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> that is used to clip this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is null.</returns>
	public Geometry ClipGeometry
	{
		get
		{
			return (Geometry)GetValue(ClipGeometryProperty);
		}
		set
		{
			SetValueInternal(ClipGeometryProperty, value);
		}
	}

	/// <summary>Gets or sets the opacity of this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>A value between 0 and 1, inclusive, that describes the opacity of this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is 1.</returns>
	public double Opacity
	{
		get
		{
			return (double)GetValue(OpacityProperty);
		}
		set
		{
			SetValueInternal(OpacityProperty, value);
		}
	}

	/// <summary>Gets or sets the brush used to alter the opacity of select regions of this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> that describes the opacity of this <see cref="T:System.Windows.Media.DrawingGroup" />; null indicates that no opacity mask exists and the opacity is uniform. The default is null.</returns>
	public Brush OpacityMask
	{
		get
		{
			return (Brush)GetValue(OpacityMaskProperty);
		}
		set
		{
			SetValueInternal(OpacityMaskProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Transform" /> that is applied to this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>The transformation to apply to this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is null.</returns>
	public Transform Transform
	{
		get
		{
			return (Transform)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.GuidelineSet" /> to apply to this <see cref="T:System.Windows.Media.DrawingGroup" />.   </summary>
	/// <returns>The <see cref="T:System.Windows.Media.GuidelineSet" /> to apply to this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is null.</returns>
	public GuidelineSet GuidelineSet
	{
		get
		{
			return (GuidelineSet)GetValue(GuidelineSetProperty);
		}
		set
		{
			SetValueInternal(GuidelineSetProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> to apply to this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> to apply to this <see cref="T:System.Windows.Media.DrawingGroup" />. The default is null.</returns>
	public BitmapEffect BitmapEffect
	{
		get
		{
			return (BitmapEffect)GetValue(BitmapEffectProperty);
		}
		set
		{
			SetValueInternal(BitmapEffectProperty, value);
		}
	}

	/// <summary>Gets or sets the region where the <see cref="T:System.Windows.Media.DrawingGroup" /> applies its <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" /> and, optionally, a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> to use as input for its <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" />.  </summary>
	/// <returns>The region where the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" /> of the <see cref="T:System.Windows.Media.DrawingGroup" /> is applied and, optionally, the <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> to use as input; or null if the <see cref="P:System.Windows.Media.DrawingGroup.BitmapEffect" /> applies to the whole <see cref="T:System.Windows.Media.DrawingGroup" /> and uses the <see cref="T:System.Windows.Media.DrawingGroup" /> as its input. The default is null.</returns>
	public BitmapEffectInput BitmapEffectInput
	{
		get
		{
			return (BitmapEffectInput)GetValue(BitmapEffectInputProperty);
		}
		set
		{
			SetValueInternal(BitmapEffectInputProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingGroup" /> class. </summary>
	public DrawingGroup()
	{
	}

	/// <summary>Opens the <see cref="T:System.Windows.Media.DrawingGroup" /> in order to populate its <see cref="P:System.Windows.Media.DrawingGroup.Children" /> and clears any existing <see cref="P:System.Windows.Media.DrawingGroup.Children" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.DrawingContext" /> that can be used to describe the contents of this <see cref="T:System.Windows.Media.DrawingGroup" /> object. </returns>
	public DrawingContext Open()
	{
		VerifyOpen();
		_openedForAppend = false;
		return new DrawingGroupDrawingContext(this);
	}

	/// <summary>Opens the <see cref="T:System.Windows.Media.DrawingGroup" /> in order to populate its <see cref="P:System.Windows.Media.DrawingGroup.Children" />. This method enables you to append additional <see cref="P:System.Windows.Media.DrawingGroup.Children" /> to this <see cref="T:System.Windows.Media.DrawingGroup" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.DrawingContext" /> that you can use to describe the contents of this <see cref="T:System.Windows.Media.DrawingGroup" /> object.</returns>
	public DrawingContext Append()
	{
		VerifyOpen();
		_openedForAppend = true;
		return new DrawingGroupDrawingContext(this);
	}

	internal void Close(DrawingCollection rootDrawingGroupChildren)
	{
		WritePreamble();
		if (!_openedForAppend)
		{
			Children = rootDrawingGroupChildren;
		}
		else
		{
			DrawingCollection obj = Children ?? throw new InvalidOperationException(SR.DrawingGroup_CannotAppendToNullCollection);
			if (obj.IsFrozen)
			{
				throw new InvalidOperationException(SR.DrawingGroup_CannotAppendToFrozenCollection);
			}
			obj.TransactionalAppend(rootDrawingGroupChildren);
		}
		_open = false;
	}

	internal override void WalkCurrentValue(DrawingContextWalker ctx)
	{
		int num = 0;
		if (!IsBaseValueDefault(TransformProperty) || AnimationStorage.GetStorage(this, TransformProperty) != null)
		{
			ctx.PushTransform(Transform);
			num++;
		}
		if (!IsBaseValueDefault(ClipGeometryProperty) || AnimationStorage.GetStorage(this, ClipGeometryProperty) != null)
		{
			ctx.PushClip(ClipGeometry);
			num++;
		}
		if (!IsBaseValueDefault(OpacityProperty) || AnimationStorage.GetStorage(this, OpacityProperty) != null)
		{
			ctx.PushOpacity(Opacity);
			num++;
		}
		if (OpacityMask != null)
		{
			ctx.PushOpacityMask(OpacityMask);
			num++;
		}
		if (BitmapEffect != null)
		{
			ctx.PushEffect(BitmapEffect, BitmapEffectInput);
			num++;
		}
		DrawingCollection children = Children;
		if (children != null)
		{
			for (int i = 0; i < children.Count; i++)
			{
				Drawing drawing = children.Internal_GetItem(i);
				if (drawing != null)
				{
					drawing.WalkCurrentValue(ctx);
					if (ctx.ShouldStopWalking)
					{
						break;
					}
				}
			}
		}
		for (int j = 0; j < num; j++)
		{
			ctx.Pop();
		}
	}

	private void VerifyOpen()
	{
		WritePreamble();
		if (_open)
		{
			throw new InvalidOperationException(SR.DrawingGroup_AlreadyOpen);
		}
		_open = true;
	}

	/// <summary>Creates a modifiable deep copy of this <see cref="T:System.Windows.Media.DrawingGroup" /> and makes deep copies of its values. </summary>
	/// <returns>A modifiable clone of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new DrawingGroup Clone()
	{
		return (DrawingGroup)base.Clone();
	}

	/// <summary>Creates a modifiable deep copy of this <see cref="T:System.Windows.Media.DrawingGroup" /> object and makes deep copies of its current values. </summary>
	/// <returns>A modifiable clone of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new DrawingGroup CloneCurrentValue()
	{
		return (DrawingGroup)base.CloneCurrentValue();
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingGroup drawingGroup = (DrawingGroup)d;
		DrawingCollection drawingCollection = null;
		DrawingCollection drawingCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			drawingCollection = (DrawingCollection)e.OldValue;
			if (drawingCollection != null && !drawingCollection.IsFrozen)
			{
				drawingCollection.ItemRemoved -= drawingGroup.ChildrenItemRemoved;
				drawingCollection.ItemInserted -= drawingGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			drawingCollection2 = (DrawingCollection)e.NewValue;
			if (drawingCollection2 != null && !drawingCollection2.IsFrozen)
			{
				drawingCollection2.ItemInserted += drawingGroup.ChildrenItemInserted;
				drawingCollection2.ItemRemoved += drawingGroup.ChildrenItemRemoved;
			}
		}
		if (drawingCollection != drawingCollection2 && drawingGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = drawingGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (drawingCollection2 != null)
					{
						int count = drawingCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)drawingCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (drawingCollection != null)
					{
						int count2 = drawingCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)drawingCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		drawingGroup.PropertyChanged(ChildrenProperty);
	}

	private static void ClipGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingGroup drawingGroup = (DrawingGroup)d;
		Geometry resource = (Geometry)e.OldValue;
		Geometry resource2 = (Geometry)e.NewValue;
		if (drawingGroup.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingGroup;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingGroup.ReleaseResource(resource, channel);
					drawingGroup.AddRefResource(resource2, channel);
				}
			}
		}
		drawingGroup.PropertyChanged(ClipGeometryProperty);
	}

	private static void OpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(OpacityProperty);
	}

	private static void OpacityMaskPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingGroup drawingGroup = (DrawingGroup)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (drawingGroup.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingGroup;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingGroup.ReleaseResource(resource, channel);
					drawingGroup.AddRefResource(resource2, channel);
				}
			}
		}
		drawingGroup.PropertyChanged(OpacityMaskProperty);
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingGroup drawingGroup = (DrawingGroup)d;
		Transform resource = (Transform)e.OldValue;
		Transform resource2 = (Transform)e.NewValue;
		if (drawingGroup.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingGroup;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingGroup.ReleaseResource(resource, channel);
					drawingGroup.AddRefResource(resource2, channel);
				}
			}
		}
		drawingGroup.PropertyChanged(TransformProperty);
	}

	private static void GuidelineSetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingGroup drawingGroup = (DrawingGroup)d;
		GuidelineSet resource = (GuidelineSet)e.OldValue;
		GuidelineSet resource2 = (GuidelineSet)e.NewValue;
		if (drawingGroup.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingGroup;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingGroup.ReleaseResource(resource, channel);
					drawingGroup.AddRefResource(resource2, channel);
				}
			}
		}
		drawingGroup.PropertyChanged(GuidelineSetProperty);
	}

	private static void EdgeModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(RenderOptions.EdgeModeProperty);
	}

	private static void BitmapEffectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(BitmapEffectProperty);
	}

	private static void BitmapEffectInputPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(BitmapEffectInputProperty);
	}

	private static void BitmapScalingModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(RenderOptions.BitmapScalingModeProperty);
	}

	private static void ClearTypeHintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DrawingGroup)d).PropertyChanged(RenderOptions.ClearTypeHintProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DrawingGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DrawingCollection children = Children;
			Geometry clipGeometry = ClipGeometry;
			Brush opacityMask = OpacityMask;
			Transform transform = Transform;
			GuidelineSet guidelineSet = GuidelineSet;
			DUCE.ResourceHandle hClipGeometry = ((DUCE.IResource)clipGeometry)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hOpacityMask = ((DUCE.IResource)opacityMask)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hGuidelineSet = ((DUCE.IResource)guidelineSet)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(OpacityProperty, channel);
			int num = children?.Count ?? 0;
			DUCE.MILCMD_DRAWINGGROUP mILCMD_DRAWINGGROUP = default(DUCE.MILCMD_DRAWINGGROUP);
			mILCMD_DRAWINGGROUP.Type = MILCMD.MilCmdDrawingGroup;
			mILCMD_DRAWINGGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_DRAWINGGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			mILCMD_DRAWINGGROUP.hClipGeometry = hClipGeometry;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_DRAWINGGROUP.Opacity = Opacity;
			}
			mILCMD_DRAWINGGROUP.hOpacityAnimations = animationResourceHandle;
			mILCMD_DRAWINGGROUP.hOpacityMask = hOpacityMask;
			mILCMD_DRAWINGGROUP.hTransform = hTransform;
			mILCMD_DRAWINGGROUP.hGuidelineSet = hGuidelineSet;
			mILCMD_DRAWINGGROUP.EdgeMode = (EdgeMode)GetValue(RenderOptions.EdgeModeProperty);
			mILCMD_DRAWINGGROUP.bitmapScalingMode = (BitmapScalingMode)GetValue(RenderOptions.BitmapScalingModeProperty);
			mILCMD_DRAWINGGROUP.ClearTypeHint = (ClearTypeHint)GetValue(RenderOptions.ClearTypeHintProperty);
			channel.BeginCommand((byte*)(&mILCMD_DRAWINGGROUP), sizeof(DUCE.MILCMD_DRAWINGGROUP), (int)mILCMD_DRAWINGGROUP.ChildrenSize);
			for (int i = 0; i < num; i++)
			{
				DUCE.ResourceHandle handle = ((DUCE.IResource)children.Internal_GetItem(i)).GetHandle(channel);
				channel.AppendCommandData((byte*)(&handle), sizeof(DUCE.ResourceHandle));
			}
			channel.EndCommand();
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DRAWINGGROUP))
		{
			((DUCE.IResource)ClipGeometry)?.AddRefOnChannel(channel);
			((DUCE.IResource)OpacityMask)?.AddRefOnChannel(channel);
			((DUCE.IResource)Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)GuidelineSet)?.AddRefOnChannel(channel);
			DrawingCollection children = Children;
			if (children != null)
			{
				int count = children.Count;
				for (int i = 0; i < count; i++)
				{
					((DUCE.IResource)children.Internal_GetItem(i)).AddRefOnChannel(channel);
				}
			}
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (!_duceResource.ReleaseOnChannel(channel))
		{
			return;
		}
		((DUCE.IResource)ClipGeometry)?.ReleaseOnChannel(channel);
		((DUCE.IResource)OpacityMask)?.ReleaseOnChannel(channel);
		((DUCE.IResource)Transform)?.ReleaseOnChannel(channel);
		((DUCE.IResource)GuidelineSet)?.ReleaseOnChannel(channel);
		DrawingCollection children = Children;
		if (children != null)
		{
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				((DUCE.IResource)children.Internal_GetItem(i)).ReleaseOnChannel(channel);
			}
		}
		ReleaseOnChannelAnimations(channel);
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

	private void ChildrenItemInserted(object sender, object item)
	{
		if (base.Dispatcher == null)
		{
			return;
		}
		using (CompositionEngineLock.Acquire())
		{
			int channelCount = ((DUCE.IResource)this).GetChannelCount();
			for (int i = 0; i < channelCount; i++)
			{
				DUCE.Channel channel = ((DUCE.IResource)this).GetChannel(i);
				if (item is DUCE.IResource resource)
				{
					resource.AddRefOnChannel(channel);
				}
				UpdateResource(channel, skipOnChannelCheck: true);
			}
		}
	}

	private void ChildrenItemRemoved(object sender, object item)
	{
		if (base.Dispatcher == null)
		{
			return;
		}
		using (CompositionEngineLock.Acquire())
		{
			int channelCount = ((DUCE.IResource)this).GetChannelCount();
			for (int i = 0; i < channelCount; i++)
			{
				DUCE.Channel channel = ((DUCE.IResource)this).GetChannel(i);
				UpdateResource(channel, skipOnChannelCheck: true);
				if (item is DUCE.IResource resource)
				{
					resource.ReleaseOnChannel(channel);
				}
			}
		}
	}

	static DrawingGroup()
	{
		s_Children = DrawingCollection.Empty;
		RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(DrawingGroup), new UIPropertyMetadata(EdgeMode.Unspecified, EdgeModePropertyChanged));
		RenderOptions.BitmapScalingModeProperty.OverrideMetadata(typeof(DrawingGroup), new UIPropertyMetadata(BitmapScalingMode.Unspecified, BitmapScalingModePropertyChanged));
		RenderOptions.ClearTypeHintProperty.OverrideMetadata(typeof(DrawingGroup), new UIPropertyMetadata(ClearTypeHint.Auto, ClearTypeHintPropertyChanged));
		Type typeFromHandle = typeof(DrawingGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(DrawingCollection), typeFromHandle, new FreezableDefaultValueFactory(DrawingCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
		ClipGeometryProperty = Animatable.RegisterProperty("ClipGeometry", typeof(Geometry), typeFromHandle, null, ClipGeometryPropertyChanged, null, isIndependentlyAnimated: false, null);
		OpacityProperty = Animatable.RegisterProperty("Opacity", typeof(double), typeFromHandle, 1.0, OpacityPropertyChanged, null, isIndependentlyAnimated: true, null);
		OpacityMaskProperty = Animatable.RegisterProperty("OpacityMask", typeof(Brush), typeFromHandle, null, OpacityMaskPropertyChanged, null, isIndependentlyAnimated: false, null);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform), typeFromHandle, null, TransformPropertyChanged, null, isIndependentlyAnimated: false, null);
		GuidelineSetProperty = Animatable.RegisterProperty("GuidelineSet", typeof(GuidelineSet), typeFromHandle, null, GuidelineSetPropertyChanged, null, isIndependentlyAnimated: false, null);
		BitmapEffectProperty = Animatable.RegisterProperty("BitmapEffect", typeof(BitmapEffect), typeFromHandle, null, BitmapEffectPropertyChanged, null, isIndependentlyAnimated: false, null);
		BitmapEffectInputProperty = Animatable.RegisterProperty("BitmapEffectInput", typeof(BitmapEffectInput), typeFromHandle, null, BitmapEffectInputPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
