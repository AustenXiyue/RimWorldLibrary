using System.Collections.Generic;
using System.Globalization;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.Media;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides rendering support in WPF, which includes hit testing, coordinate transformation, and bounding box calculations.</summary>
public abstract class Visual : DependencyObject, DUCE.IResource
{
	internal class TopMostHitResult
	{
		internal HitTestResult _hitResult;

		internal HitTestResultBehavior HitTestResult(HitTestResult result)
		{
			_hitResult = result;
			return HitTestResultBehavior.Stop;
		}

		internal HitTestFilterBehavior NoNested2DFilter(DependencyObject potentialHitTestTarget)
		{
			if (potentialHitTestTarget is Viewport2DVisual3D)
			{
				return HitTestFilterBehavior.ContinueSkipChildren;
			}
			return HitTestFilterBehavior.Continue;
		}
	}

	internal delegate void AncestorChangedEventHandler(object sender, AncestorChangedEventArgs e);

	private const VisualProxyFlags c_ProxyFlagsDirtyMask = VisualProxyFlags.IsSubtreeDirtyForRender | VisualProxyFlags.IsTransformDirty | VisualProxyFlags.IsClipDirty | VisualProxyFlags.IsContentDirty | VisualProxyFlags.IsOpacityDirty | VisualProxyFlags.IsOpacityMaskDirty | VisualProxyFlags.IsOffsetDirty | VisualProxyFlags.IsClearTypeHintDirty | VisualProxyFlags.IsGuidelineCollectionDirty | VisualProxyFlags.IsEdgeModeDirty | VisualProxyFlags.IsBitmapScalingModeDirty | VisualProxyFlags.IsEffectDirty | VisualProxyFlags.IsCacheModeDirty | VisualProxyFlags.IsScrollableAreaClipDirty | VisualProxyFlags.IsTextRenderingModeDirty | VisualProxyFlags.IsTextHintingModeDirty;

	private const VisualProxyFlags c_Viewport3DProxyFlagsDirtyMask = VisualProxyFlags.Viewport3DVisual_IsCameraDirty | VisualProxyFlags.Viewport3DVisual_IsViewportDirty;

	internal static readonly UncommonField<BitmapEffectState> BitmapEffectStateField = new UncommonField<BitmapEffectState>();

	internal int _parentIndex;

	internal DependencyObject _parent;

	internal VisualProxy _proxy;

	private Rect _bboxSubgraph = Rect.Empty;

	private static readonly UncommonField<Dictionary<ICyclicBrush, int>> CyclicBrushToChannelsMapField = new UncommonField<Dictionary<ICyclicBrush, int>>();

	private static readonly UncommonField<Dictionary<DUCE.Channel, int>> ChannelsToCyclicBrushMapField = new UncommonField<Dictionary<DUCE.Channel, int>>();

	internal static readonly UncommonField<int> DpiIndex = new UncommonField<int>();

	private static readonly UncommonField<Geometry> ClipField = new UncommonField<Geometry>();

	private static readonly UncommonField<double> OpacityField = new UncommonField<double>(1.0);

	private static readonly UncommonField<Brush> OpacityMaskField = new UncommonField<Brush>();

	private static readonly UncommonField<EdgeMode> EdgeModeField = new UncommonField<EdgeMode>();

	private static readonly UncommonField<BitmapScalingMode> BitmapScalingModeField = new UncommonField<BitmapScalingMode>();

	private static readonly UncommonField<ClearTypeHint> ClearTypeHintField = new UncommonField<ClearTypeHint>();

	private static readonly UncommonField<Transform> TransformField = new UncommonField<Transform>();

	private static readonly UncommonField<Effect> EffectField = new UncommonField<Effect>();

	private static readonly UncommonField<CacheMode> CacheModeField = new UncommonField<CacheMode>();

	private static readonly UncommonField<DoubleCollection> GuidelinesXField = new UncommonField<DoubleCollection>();

	private static readonly UncommonField<DoubleCollection> GuidelinesYField = new UncommonField<DoubleCollection>();

	private static readonly UncommonField<AncestorChangedEventHandler> AncestorChangedEventField = new UncommonField<AncestorChangedEventHandler>();

	private static readonly UncommonField<BitmapEffectState> UserProvidedBitmapEffectData = new UncommonField<BitmapEffectState>();

	private static readonly UncommonField<Rect?> ScrollableAreaClipField = new UncommonField<Rect?>(null);

	private static readonly UncommonField<TextRenderingMode> TextRenderingModeField = new UncommonField<TextRenderingMode>();

	private static readonly UncommonField<TextHintingMode> TextHintingModeField = new UncommonField<TextHintingMode>();

	private Vector _offset;

	private VisualFlags _flags;

	private const uint TreeLevelLimit = 2047u;

	internal bool IsVisualChildrenIterationInProgress
	{
		[FriendAccessAllowed]
		get
		{
			return CheckFlagsAnd(VisualFlags.IsVisualChildrenIterationInProgress);
		}
		[FriendAccessAllowed]
		set
		{
			SetFlags(value, VisualFlags.IsVisualChildrenIterationInProgress);
		}
	}

	internal bool IsRootElement
	{
		get
		{
			return CheckFlagsAnd(VisualFlags.ShouldPostRender);
		}
		set
		{
			SetFlags(value, VisualFlags.ShouldPostRender);
		}
	}

	internal Rect VisualContentBounds
	{
		get
		{
			VerifyAPIReadWrite();
			return GetContentBounds();
		}
	}

	internal Rect VisualDescendantBounds
	{
		get
		{
			VerifyAPIReadWrite();
			Rect rect = CalculateSubgraphBoundsInnerSpace();
			if (DoubleUtil.RectHasNaN(rect))
			{
				rect.X = double.NegativeInfinity;
				rect.Y = double.NegativeInfinity;
				rect.Width = double.PositiveInfinity;
				rect.Height = double.PositiveInfinity;
			}
			return rect;
		}
	}

	/// <summary>Gets the number of child elements for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The number of child elements.</returns>
	protected virtual int VisualChildrenCount => 0;

	internal int InternalVisualChildrenCount => VisualChildrenCount;

	internal virtual int InternalVisual2DOr3DChildrenCount => VisualChildrenCount;

	internal bool HasVisualChildren => (_flags & VisualFlags.HasChildren) != 0;

	internal uint TreeLevel
	{
		get
		{
			return ((uint)_flags & 0xFFE00000u) >> 21;
		}
		set
		{
			if (value > 2047)
			{
				throw new InvalidOperationException(SR.Format(SR.LayoutManager_DeepRecursion, 2047u));
			}
			_flags = (VisualFlags)((uint)(_flags & ~(VisualFlags.TreeLevelBit0 | VisualFlags.TreeLevelBit1 | VisualFlags.TreeLevelBit2 | VisualFlags.TreeLevelBit3 | VisualFlags.TreeLevelBit4 | VisualFlags.TreeLevelBit5 | VisualFlags.TreeLevelBit6 | VisualFlags.TreeLevelBit7 | VisualFlags.TreeLevelBit8 | VisualFlags.TreeLevelBit9 | VisualFlags.TreeLevelBit10)) | (value << 21));
		}
	}

	/// <summary>Gets the visual tree parent of the visual object.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Visual" /> parent.</returns>
	protected DependencyObject VisualParent
	{
		get
		{
			VerifyAPIReadOnly();
			return InternalVisualParent;
		}
	}

	internal DependencyObject InternalVisualParent => _parent;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Transform" /> value for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The transform value of the visual.</returns>
	protected internal Transform VisualTransform
	{
		get
		{
			VerifyAPIReadOnly();
			return TransformField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			Transform value2 = TransformField.GetValue(this);
			if (value2 == value)
			{
				return;
			}
			if (value != null && !value.IsFrozen)
			{
				value.Changed += TransformChangedHandler;
			}
			if (value2 != null)
			{
				if (!value2.IsFrozen)
				{
					value2.Changed -= TransformChangedHandler;
				}
				DisconnectAttachedResource(VisualProxyFlags.IsTransformDirty, value2);
			}
			TransformField.SetValue(this, value);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsTransformDirty);
			TransformChanged(null, null);
		}
	}

	/// <summary>Gets or sets the bitmap effect to apply to the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Media.Effects.Effect" /> that represents the bitmap effect.</returns>
	protected internal Effect VisualEffect
	{
		get
		{
			VerifyAPIReadOnly();
			return VisualEffectInternal;
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			if (UserProvidedBitmapEffectData.GetValue(this) != null)
			{
				if (value != null)
				{
					throw new Exception(SR.Effect_CombinedLegacyAndNew);
				}
			}
			else
			{
				VisualEffectInternal = value;
			}
		}
	}

	internal Effect VisualEffectInternal
	{
		get
		{
			if (NodeHasLegacyBitmapEffect)
			{
				return null;
			}
			return EffectField.GetValue(this);
		}
		set
		{
			Effect value2 = EffectField.GetValue(this);
			if (value2 == value)
			{
				return;
			}
			if (value != null && !value.IsFrozen)
			{
				value.Changed += EffectChangedHandler;
			}
			if (value2 != null)
			{
				if (!value2.IsFrozen)
				{
					value2.Changed -= EffectChangedHandler;
				}
				DisconnectAttachedResource(VisualProxyFlags.IsEffectDirty, value2);
			}
			SetFlags(value != null, VisualFlags.NodeHasEffect);
			EffectField.SetValue(this, value);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsEffectDirty);
			EffectChanged(null, null);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> value for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The bitmap effect for this visual object.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected internal BitmapEffect VisualBitmapEffect
	{
		get
		{
			VerifyAPIReadOnly();
			return UserProvidedBitmapEffectData.GetValue(this)?.BitmapEffect;
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			Effect value2 = EffectField.GetValue(this);
			BitmapEffectState bitmapEffectState = UserProvidedBitmapEffectData.GetValue(this);
			if (bitmapEffectState == null && value2 != null)
			{
				if (value != null)
				{
					throw new Exception(SR.Effect_CombinedLegacyAndNew);
				}
				return;
			}
			BitmapEffect bitmapEffect = bitmapEffectState?.BitmapEffect;
			if (bitmapEffect == value)
			{
				return;
			}
			if (value == null)
			{
				UserProvidedBitmapEffectData.SetValue(this, null);
			}
			else
			{
				if (bitmapEffectState == null)
				{
					bitmapEffectState = new BitmapEffectState();
					UserProvidedBitmapEffectData.SetValue(this, bitmapEffectState);
				}
				bitmapEffectState.BitmapEffect = value;
			}
			if (value != null && !value.IsFrozen)
			{
				value.Changed += BitmapEffectEmulationChanged;
			}
			if (bitmapEffect != null && !bitmapEffect.IsFrozen)
			{
				bitmapEffect.Changed -= BitmapEffectEmulationChanged;
			}
			BitmapEffectEmulationChanged(null, null);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> value for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The bitmap effect input value for this visual object.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected internal BitmapEffectInput VisualBitmapEffectInput
	{
		get
		{
			VerifyAPIReadOnly();
			return UserProvidedBitmapEffectData.GetValue(this)?.BitmapEffectInput;
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			Effect value2 = EffectField.GetValue(this);
			BitmapEffectState bitmapEffectState = UserProvidedBitmapEffectData.GetValue(this);
			if (bitmapEffectState == null && value2 != null)
			{
				if (value != null)
				{
					throw new Exception(SR.Effect_CombinedLegacyAndNew);
				}
				return;
			}
			BitmapEffectInput bitmapEffectInput = bitmapEffectState?.BitmapEffectInput;
			if (bitmapEffectInput != value)
			{
				if (bitmapEffectState == null)
				{
					bitmapEffectState = new BitmapEffectState();
					UserProvidedBitmapEffectData.SetValue(this, bitmapEffectState);
				}
				bitmapEffectState.BitmapEffectInput = value;
				if (value != null && !value.IsFrozen)
				{
					value.Changed += BitmapEffectEmulationChanged;
				}
				if (bitmapEffectInput != null && !bitmapEffectInput.IsFrozen)
				{
					bitmapEffectInput.Changed -= BitmapEffectEmulationChanged;
				}
				BitmapEffectEmulationChanged(null, null);
			}
		}
	}

	internal bool BitmapEffectEmulationDisabled
	{
		get
		{
			return CheckFlagsAnd(VisualFlags.BitmapEffectEmulationDisabled);
		}
		set
		{
			if (value != CheckFlagsAnd(VisualFlags.BitmapEffectEmulationDisabled))
			{
				SetFlags(value, VisualFlags.BitmapEffectEmulationDisabled);
				BitmapEffectEmulationChanged(null, null);
			}
		}
	}

	internal BitmapEffect VisualBitmapEffectInternal
	{
		get
		{
			VerifyAPIReadOnly();
			if (NodeHasLegacyBitmapEffect)
			{
				return BitmapEffectStateField.GetValue(this).BitmapEffect;
			}
			return null;
		}
		set
		{
			BitmapEffectState bitmapEffectState = BitmapEffectStateField.GetValue(this);
			if (bitmapEffectState?.BitmapEffect == value)
			{
				return;
			}
			if (value == null)
			{
				BitmapEffectStateField.SetValue(this, null);
				return;
			}
			if (bitmapEffectState == null)
			{
				bitmapEffectState = new BitmapEffectState();
				BitmapEffectStateField.SetValue(this, bitmapEffectState);
			}
			bitmapEffectState.BitmapEffect = value;
		}
	}

	internal BitmapEffectInput VisualBitmapEffectInputInternal
	{
		get
		{
			VerifyAPIReadOnly();
			return BitmapEffectStateField.GetValue(this)?.BitmapEffectInput;
		}
		set
		{
			VerifyAPIReadWrite();
			BitmapEffectState bitmapEffectState = BitmapEffectStateField.GetValue(this);
			if (bitmapEffectState?.BitmapEffectInput != value)
			{
				if (bitmapEffectState == null)
				{
					bitmapEffectState = new BitmapEffectState();
					BitmapEffectStateField.SetValue(this, bitmapEffectState);
				}
				bitmapEffectState.BitmapEffectInput = value;
			}
		}
	}

	/// <summary>Gets or sets a cached representation of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.CacheMode" /> that holds a cached representation of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	protected internal CacheMode VisualCacheMode
	{
		get
		{
			VerifyAPIReadOnly();
			return CacheModeField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			CacheMode value2 = CacheModeField.GetValue(this);
			if (value2 == value)
			{
				return;
			}
			if (value != null && !value.IsFrozen)
			{
				value.Changed += CacheModeChangedHandler;
			}
			if (value2 != null)
			{
				if (!value2.IsFrozen)
				{
					value2.Changed -= CacheModeChangedHandler;
				}
				DisconnectAttachedResource(VisualProxyFlags.IsCacheModeDirty, value2);
			}
			CacheModeField.SetValue(this, value);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsCacheModeDirty);
			CacheModeChanged(null, null);
		}
	}

	/// <summary>Gets or sets a clipped scrollable area for the <see cref="T:System.Windows.Media.Visual" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that represents the scrollable clipping area, or null if no clipping area is assigned. </returns>
	protected internal Rect? VisualScrollableAreaClip
	{
		get
		{
			VerifyAPIReadOnly();
			return ScrollableAreaClipField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite();
			if (ScrollableAreaClipField.GetValue(this) != value)
			{
				ScrollableAreaClipField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsScrollableAreaClipDirty);
				ScrollableAreaClipChanged(null, null);
			}
		}
	}

	/// <summary>Gets or sets the clip region of the <see cref="T:System.Windows.Media.Visual" /> as a <see cref="T:System.Windows.Media.Geometry" /> value.</summary>
	/// <returns>The clip region value of the visual as a <see cref="T:System.Windows.Media.Geometry" /> type.</returns>
	protected internal Geometry VisualClip
	{
		get
		{
			VerifyAPIReadOnly();
			return ClipField.GetValue(this);
		}
		protected set
		{
			ChangeVisualClip(value, dontSetWhenClose: false);
		}
	}

	/// <summary>Gets or sets the offset value of the visual object.</summary>
	/// <returns>A <see cref="T:System.Windows.Vector" /> that specifies the offset value.</returns>
	protected internal Vector VisualOffset
	{
		get
		{
			return _offset;
		}
		protected set
		{
			VerifyAPIReadWrite();
			if (value != _offset)
			{
				_offset = value;
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsOffsetDirty);
				VisualFlags flags = VisualFlags.IsSubtreeDirtyForPrecompute;
				PropagateFlags(this, flags, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the opacity of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The opacity value of the visual.</returns>
	protected internal double VisualOpacity
	{
		get
		{
			VerifyAPIReadOnly();
			return OpacityField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite();
			if (OpacityField.GetValue(this) != value)
			{
				OpacityField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsOpacityDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the edge mode of the <see cref="T:System.Windows.Media.Visual" /> as an <see cref="T:System.Windows.Media.EdgeMode" /> value.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.EdgeMode" /> value of the visual.</returns>
	protected internal EdgeMode VisualEdgeMode
	{
		get
		{
			VerifyAPIReadOnly();
			return EdgeModeField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite();
			if (EdgeModeField.GetValue(this) != value)
			{
				EdgeModeField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsEdgeModeDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.BitmapScalingMode" /> for the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.BitmapScalingMode" /> value for the <see cref="T:System.Windows.Media.Visual" />.</returns>
	protected internal BitmapScalingMode VisualBitmapScalingMode
	{
		get
		{
			VerifyAPIReadOnly();
			return BitmapScalingModeField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite();
			if (BitmapScalingModeField.GetValue(this) != value)
			{
				BitmapScalingModeField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsBitmapScalingModeDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.ClearTypeHint" /> that determines how ClearType is rendered in the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.ClearTypeHint" /> of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	protected internal ClearTypeHint VisualClearTypeHint
	{
		get
		{
			VerifyAPIReadOnly();
			return ClearTypeHintField.GetValue(this);
		}
		set
		{
			VerifyAPIReadWrite();
			if (ClearTypeHintField.GetValue(this) != value)
			{
				ClearTypeHintField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsClearTypeHintDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.TextRenderingMode" /> of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextRenderingMode" /> applied to the <see cref="T:System.Windows.Media.Visual" />.</returns>
	protected internal TextRenderingMode VisualTextRenderingMode
	{
		get
		{
			VerifyAPIReadOnly();
			return TextRenderingModeField.GetValue(this);
		}
		set
		{
			VerifyAPIReadWrite();
			if (TextRenderingModeField.GetValue(this) != value)
			{
				TextRenderingModeField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsTextRenderingModeDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.TextHintingMode" /> of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextHintingMode" /> applied to the <see cref="T:System.Windows.Media.Visual" />.</returns>
	protected internal TextHintingMode VisualTextHintingMode
	{
		get
		{
			VerifyAPIReadOnly();
			return TextHintingModeField.GetValue(this);
		}
		set
		{
			VerifyAPIReadWrite();
			if (TextHintingModeField.GetValue(this) != value)
			{
				TextHintingModeField.SetValue(this, value);
				SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsTextHintingModeDirty);
				PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			}
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> value that represents the opacity mask of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Brush" /> that represents the opacity mask value of the visual.</returns>
	protected internal Brush VisualOpacityMask
	{
		get
		{
			VerifyAPIReadOnly();
			return OpacityMaskField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			Brush value2 = OpacityMaskField.GetValue(this);
			if (value2 == value)
			{
				return;
			}
			if (value != null && !value.IsFrozen)
			{
				value.Changed += OpacityMaskChangedHandler;
			}
			if (value2 != null)
			{
				if (!value2.IsFrozen)
				{
					value2.Changed -= OpacityMaskChangedHandler;
				}
				DisconnectAttachedResource(VisualProxyFlags.IsOpacityMaskDirty, value2);
			}
			OpacityMaskField.SetValue(this, value);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsOpacityMaskDirty);
			OpacityMaskChanged(null, null);
		}
	}

	/// <summary>Gets or sets the x-coordinate (vertical) guideline collection.</summary>
	/// <returns>The x-coordinate guideline collection of the visual.</returns>
	protected internal DoubleCollection VisualXSnappingGuidelines
	{
		get
		{
			VerifyAPIReadOnly();
			return GuidelinesXField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			DoubleCollection value2 = GuidelinesXField.GetValue(this);
			if (value2 != value)
			{
				if (value != null && !value.IsFrozen)
				{
					value.Changed += GuidelinesChangedHandler;
				}
				if (value2 != null && !value2.IsFrozen)
				{
					value2.Changed -= GuidelinesChangedHandler;
				}
				GuidelinesXField.SetValue(this, value);
				GuidelinesChanged(null, null);
			}
		}
	}

	/// <summary>Gets or sets the y-coordinate (horizontal) guideline collection.</summary>
	/// <returns>The y-coordinate guideline collection of the visual.</returns>
	protected internal DoubleCollection VisualYSnappingGuidelines
	{
		get
		{
			VerifyAPIReadOnly();
			return GuidelinesYField.GetValue(this);
		}
		protected set
		{
			VerifyAPIReadWrite(value);
			DoubleCollection value2 = GuidelinesYField.GetValue(this);
			if (value2 != value)
			{
				if (value != null && !value.IsFrozen)
				{
					value.Changed += GuidelinesChangedHandler;
				}
				if (value2 != null && !value2.IsFrozen)
				{
					value2.Changed -= GuidelinesChangedHandler;
				}
				GuidelinesYField.SetValue(this, value);
				GuidelinesChanged(null, null);
			}
		}
	}

	internal EventHandler ClipChangedHandler => ClipChanged;

	internal EventHandler ScrollableAreaClipChangedHandler => ScrollableAreaClipChanged;

	internal EventHandler TransformChangedHandler => TransformChanged;

	internal EventHandler EffectChangedHandler => EffectChanged;

	internal EventHandler CacheModeChangedHandler => EffectChanged;

	internal EventHandler GuidelinesChangedHandler => GuidelinesChanged;

	internal EventHandler OpacityMaskChangedHandler => OpacityMaskChanged;

	internal EventHandler ContentsChangedHandler => ContentsChanged;

	private bool NodeHasLegacyBitmapEffect
	{
		get
		{
			if (CheckFlagsAnd(VisualFlags.NodeHasEffect))
			{
				return BitmapEffectStateField.GetValue(this) != null;
			}
			return false;
		}
	}

	internal event AncestorChangedEventHandler VisualAncestorChanged
	{
		add
		{
			AncestorChangedEventHandler value2 = AncestorChangedEventField.GetValue(this);
			value2 = ((value2 != null) ? ((AncestorChangedEventHandler)Delegate.Combine(value2, value)) : value);
			AncestorChangedEventField.SetValue(this, value2);
			SetTreeBits(this, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
		}
		remove
		{
			if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
			{
				ClearTreeBits(this, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
			}
			AncestorChangedEventHandler value2 = AncestorChangedEventField.GetValue(this);
			if (value2 != null)
			{
				value2 = (AncestorChangedEventHandler)Delegate.Remove(value2, value);
				if (value2 == null)
				{
					AncestorChangedEventField.ClearValue(this);
				}
				else
				{
					AncestorChangedEventField.SetValue(this, value2);
				}
			}
		}
	}

	internal Visual(DUCE.ResourceType resourceType)
	{
		if (resourceType != DUCE.ResourceType.TYPE_VISUAL && resourceType == DUCE.ResourceType.TYPE_VIEWPORT3DVISUAL)
		{
			SetFlags(value: true, VisualFlags.IsViewport3DVisual);
		}
	}

	/// <summary>Provides the base initialization for objects derived from the <see cref="T:System.Windows.Media.Visual" /> class.</summary>
	protected Visual()
		: this(DUCE.ResourceType.TYPE_VISUAL)
	{
	}

	internal bool IsOnChannel(DUCE.Channel channel)
	{
		return _proxy.IsOnChannel(channel);
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		return _proxy.GetHandle(channel);
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		throw new NotImplementedException();
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		return AddRefOnChannelCore(channel);
	}

	internal virtual DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		DUCE.ResourceType resourceType = DUCE.ResourceType.TYPE_VISUAL;
		if (CheckFlagsAnd(VisualFlags.IsViewport3DVisual))
		{
			resourceType = DUCE.ResourceType.TYPE_VIEWPORT3DVISUAL;
		}
		_proxy.CreateOrAddRefOnChannel(this, channel, resourceType);
		return _proxy.GetHandle(channel);
	}

	internal virtual void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		_proxy.ReleaseOnChannel(channel);
	}

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		DUCE.CompositionNode.RemoveChild(parent.GetHandle(channel), _proxy.GetHandle(channel), channel);
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _proxy.Count;
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _proxy.GetChannel(index);
	}

	internal virtual Rect GetContentBounds()
	{
		return Rect.Empty;
	}

	internal virtual void RenderContent(RenderContext ctx, bool isOnChannel)
	{
	}

	internal virtual void RenderClose(IDrawingContent newContent)
	{
	}

	internal Rect CalculateSubgraphBoundsInnerSpace()
	{
		return CalculateSubgraphBoundsInnerSpace(renderBounds: false);
	}

	internal Rect CalculateSubgraphRenderBoundsInnerSpace()
	{
		return CalculateSubgraphBoundsInnerSpace(renderBounds: true);
	}

	internal virtual Rect CalculateSubgraphBoundsInnerSpace(bool renderBounds)
	{
		Rect empty = Rect.Empty;
		int visualChildrenCount = VisualChildrenCount;
		for (int i = 0; i < visualChildrenCount; i++)
		{
			Visual visualChild = GetVisualChild(i);
			if (visualChild != null)
			{
				Rect rect = visualChild.CalculateSubgraphBoundsOuterSpace(renderBounds);
				empty.Union(rect);
			}
		}
		Rect bounds = GetContentBounds();
		if (renderBounds && IsEmptyRenderBounds(ref bounds))
		{
			bounds = Rect.Empty;
		}
		empty.Union(bounds);
		return empty;
	}

	internal Rect CalculateSubgraphBoundsOuterSpace()
	{
		return CalculateSubgraphBoundsOuterSpace(renderBounds: false);
	}

	internal Rect CalculateSubgraphRenderBoundsOuterSpace()
	{
		return CalculateSubgraphBoundsOuterSpace(renderBounds: true);
	}

	private Rect CalculateSubgraphBoundsOuterSpace(bool renderBounds)
	{
		Rect empty = Rect.Empty;
		empty = CalculateSubgraphBoundsInnerSpace(renderBounds);
		if (CheckFlagsAnd(VisualFlags.NodeHasEffect))
		{
			Effect value = EffectField.GetValue(this);
			if (value != null)
			{
				Rect rect = new Rect(0.0, 0.0, 1.0, 1.0);
				Rect rect2 = Effect.UnitToWorld(value.EffectMapping.TransformBounds(rect), empty);
				empty.Union(rect2);
			}
		}
		Geometry value2 = ClipField.GetValue(this);
		if (value2 != null)
		{
			empty.Intersect(value2.Bounds);
		}
		Transform value3 = TransformField.GetValue(this);
		if (value3 != null && !value3.IsIdentity)
		{
			Matrix matrix = value3.Value;
			MatrixUtil.TransformRect(ref empty, ref matrix);
		}
		if (!empty.IsEmpty)
		{
			empty.X += _offset.X;
			empty.Y += _offset.Y;
		}
		Rect? value4 = ScrollableAreaClipField.GetValue(this);
		if (value4.HasValue)
		{
			empty.Intersect(value4.Value);
		}
		if (DoubleUtil.RectHasNaN(empty))
		{
			empty.X = double.NegativeInfinity;
			empty.Y = double.NegativeInfinity;
			empty.Width = double.PositiveInfinity;
			empty.Height = double.PositiveInfinity;
		}
		return empty;
	}

	private bool IsEmptyRenderBounds(ref Rect bounds)
	{
		if (!(bounds.Width <= 0.0))
		{
			return bounds.Height <= 0.0;
		}
		return true;
	}

	internal virtual void FreeContent(DUCE.Channel channel)
	{
	}

	private bool IsCyclicBrushRootOnChannel(DUCE.Channel channel)
	{
		bool result = false;
		Dictionary<DUCE.Channel, int> value = ChannelsToCyclicBrushMapField.GetValue(this);
		if (value != null && value.TryGetValue(channel, out var value2))
		{
			result = value2 > 0;
		}
		return result;
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		if (!IsOnChannel(channel) || CheckFlagsAnd(channel, VisualProxyFlags.IsDeleteResourceInProgress))
		{
			return;
		}
		SetFlags(channel, value: true, VisualProxyFlags.IsDeleteResourceInProgress);
		try
		{
			SetFlags(channel, value: false, VisualProxyFlags.IsConnectedToParent);
			if (!CheckFlagsOr(VisualFlags.NodeIsCyclicBrushRoot) || !channel.IsConnected || channel.IsSynchronous || !IsCyclicBrushRootOnChannel(channel))
			{
				FreeContent(channel);
				Transform value = TransformField.GetValue(this);
				if (value != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsTransformDirty))
				{
					((DUCE.IResource)value).ReleaseOnChannel(channel);
				}
				Effect value2 = EffectField.GetValue(this);
				if (value2 != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsEffectDirty))
				{
					((DUCE.IResource)value2).ReleaseOnChannel(channel);
				}
				Geometry value3 = ClipField.GetValue(this);
				if (value3 != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsClipDirty))
				{
					((DUCE.IResource)value3).ReleaseOnChannel(channel);
				}
				Brush value4 = OpacityMaskField.GetValue(this);
				if (value4 != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsOpacityMaskDirty))
				{
					((DUCE.IResource)value4).ReleaseOnChannel(channel);
				}
				CacheMode value5 = CacheModeField.GetValue(this);
				if (value5 != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsCacheModeDirty))
				{
					((DUCE.IResource)value5).ReleaseOnChannel(channel);
				}
				ReleaseOnChannelCore(channel);
				int visualChildrenCount = VisualChildrenCount;
				for (int i = 0; i < visualChildrenCount; i++)
				{
					((DUCE.IResource)GetVisualChild(i))?.ReleaseOnChannel(channel);
				}
			}
		}
		finally
		{
			if (IsOnChannel(channel))
			{
				SetFlags(channel, value: false, VisualProxyFlags.IsDeleteResourceInProgress);
			}
		}
	}

	internal virtual void AddRefOnChannelForCyclicBrush(ICyclicBrush cyclicBrush, DUCE.Channel channel)
	{
		Dictionary<DUCE.Channel, int> dictionary = ChannelsToCyclicBrushMapField.GetValue(this);
		if (dictionary == null)
		{
			dictionary = new Dictionary<DUCE.Channel, int>();
			ChannelsToCyclicBrushMapField.SetValue(this, dictionary);
		}
		if (!dictionary.ContainsKey(channel))
		{
			SetFlags(value: true, VisualFlags.NodeIsCyclicBrushRoot);
			dictionary[channel] = 1;
		}
		else
		{
			dictionary[channel]++;
		}
		Dictionary<ICyclicBrush, int> dictionary2 = CyclicBrushToChannelsMapField.GetValue(this);
		if (dictionary2 == null)
		{
			dictionary2 = new Dictionary<ICyclicBrush, int>();
			CyclicBrushToChannelsMapField.SetValue(this, dictionary2);
		}
		if (!dictionary2.ContainsKey(cyclicBrush))
		{
			dictionary2[cyclicBrush] = 1;
		}
		else
		{
			dictionary2[cyclicBrush]++;
		}
		cyclicBrush.RenderForCyclicBrush(channel, skipChannelCheck: false);
	}

	internal virtual void ReleaseOnChannelForCyclicBrush(ICyclicBrush cyclicBrush, DUCE.Channel channel)
	{
		Dictionary<ICyclicBrush, int> value = CyclicBrushToChannelsMapField.GetValue(this);
		if (value[cyclicBrush] == 1)
		{
			value.Remove(cyclicBrush);
		}
		else
		{
			value[cyclicBrush]--;
		}
		Dictionary<DUCE.Channel, int> value2 = ChannelsToCyclicBrushMapField.GetValue(this);
		value2[channel]--;
		if (value2[channel] == 0)
		{
			value2.Remove(channel);
			SetFlags(value: false, VisualFlags.NodeIsCyclicBrushRoot);
			PropagateFlags(this, VisualFlags.None, VisualProxyFlags.IsSubtreeDirtyForRender);
			if ((_parent == null || !CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent)) && !IsRootElement)
			{
				((DUCE.IResource)this).ReleaseOnChannel(channel);
			}
		}
	}

	internal void VerifyAPIReadOnly()
	{
		VerifyAccess();
	}

	internal void VerifyAPIReadOnly(DependencyObject value)
	{
		VerifyAPIReadOnly();
		MediaSystem.AssertSameContext(this, value);
	}

	internal void VerifyAPIReadWrite()
	{
		VerifyAPIReadOnly();
		MediaContext.From(base.Dispatcher).VerifyWriteAccess();
	}

	internal void VerifyAPIReadWrite(DependencyObject value)
	{
		VerifyAPIReadWrite();
		MediaSystem.AssertSameContext(this, value);
	}

	internal void Precompute()
	{
		if (!CheckFlagsAnd(VisualFlags.IsSubtreeDirtyForPrecompute))
		{
			return;
		}
		using (base.Dispatcher.DisableProcessing())
		{
			MediaContext mediaContext = MediaContext.From(base.Dispatcher);
			try
			{
				mediaContext.PushReadOnlyAccess();
				PrecomputeRecursive(out var _);
			}
			finally
			{
				mediaContext.PopReadOnlyAccess();
			}
		}
	}

	internal virtual void PrecomputeContent()
	{
		_bboxSubgraph = GetHitTestBounds();
		if (DoubleUtil.RectHasNaN(_bboxSubgraph))
		{
			_bboxSubgraph.X = double.NegativeInfinity;
			_bboxSubgraph.Y = double.NegativeInfinity;
			_bboxSubgraph.Width = double.PositiveInfinity;
			_bboxSubgraph.Height = double.PositiveInfinity;
		}
	}

	internal void PrecomputeRecursive(out Rect bboxSubgraph)
	{
		if (Enter())
		{
			try
			{
				if (CheckFlagsAnd(VisualFlags.IsSubtreeDirtyForPrecompute))
				{
					PrecomputeContent();
					int visualChildrenCount = VisualChildrenCount;
					for (int i = 0; i < visualChildrenCount; i++)
					{
						Visual visualChild = GetVisualChild(i);
						if (visualChild != null)
						{
							visualChild.PrecomputeRecursive(out var bboxSubgraph2);
							_bboxSubgraph.Union(bboxSubgraph2);
						}
					}
					SetFlags(value: false, VisualFlags.IsSubtreeDirtyForPrecompute);
				}
				bboxSubgraph = _bboxSubgraph;
				Geometry value = ClipField.GetValue(this);
				if (value != null)
				{
					bboxSubgraph.Intersect(value.Bounds);
				}
				Transform value2 = TransformField.GetValue(this);
				if (value2 != null && !value2.IsIdentity)
				{
					Matrix matrix = value2.Value;
					MatrixUtil.TransformRect(ref bboxSubgraph, ref matrix);
				}
				if (!bboxSubgraph.IsEmpty)
				{
					bboxSubgraph.X += _offset.X;
					bboxSubgraph.Y += _offset.Y;
				}
				Rect? value3 = ScrollableAreaClipField.GetValue(this);
				if (value3.HasValue)
				{
					bboxSubgraph.Intersect(value3.Value);
				}
				if (DoubleUtil.RectHasNaN(bboxSubgraph))
				{
					bboxSubgraph.X = double.NegativeInfinity;
					bboxSubgraph.Y = double.NegativeInfinity;
					bboxSubgraph.Width = double.PositiveInfinity;
					bboxSubgraph.Height = double.PositiveInfinity;
				}
				return;
			}
			finally
			{
				Exit();
			}
		}
		bboxSubgraph = default(Rect);
	}

	internal void Render(RenderContext ctx, uint childIndex)
	{
		DUCE.Channel channel = ctx.Channel;
		if (CheckFlagsAnd(channel, VisualProxyFlags.IsSubtreeDirtyForRender) || !IsOnChannel(channel))
		{
			RenderRecursive(ctx);
		}
		if (IsOnChannel(channel) && !CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent) && !ctx.Root.IsNull)
		{
			DUCE.CompositionNode.InsertChildAt(ctx.Root, _proxy.GetHandle(channel), childIndex, channel);
			SetFlags(channel, value: true, VisualProxyFlags.IsConnectedToParent);
		}
	}

	internal virtual void RenderRecursive(RenderContext ctx)
	{
		if (!Enter())
		{
			return;
		}
		try
		{
			DUCE.Channel channel = ctx.Channel;
			DUCE.ResourceHandle @null = DUCE.ResourceHandle.Null;
			VisualProxyFlags visualProxyFlags = VisualProxyFlags.None;
			bool flag = IsOnChannel(channel);
			if (flag)
			{
				@null = _proxy.GetHandle(channel);
				visualProxyFlags = _proxy.GetFlags(channel);
			}
			else
			{
				@null = ((DUCE.IResource)this).AddRefOnChannel(channel);
				SetFlags(channel, value: true, VisualProxyFlags.Viewport3DVisual_IsCameraDirty | VisualProxyFlags.Viewport3DVisual_IsViewportDirty);
				visualProxyFlags = VisualProxyFlags.IsSubtreeDirtyForRender | VisualProxyFlags.IsTransformDirty | VisualProxyFlags.IsClipDirty | VisualProxyFlags.IsContentDirty | VisualProxyFlags.IsOpacityDirty | VisualProxyFlags.IsOpacityMaskDirty | VisualProxyFlags.IsOffsetDirty | VisualProxyFlags.IsClearTypeHintDirty | VisualProxyFlags.IsGuidelineCollectionDirty | VisualProxyFlags.IsEdgeModeDirty | VisualProxyFlags.IsBitmapScalingModeDirty | VisualProxyFlags.IsEffectDirty | VisualProxyFlags.IsCacheModeDirty | VisualProxyFlags.IsScrollableAreaClipDirty | VisualProxyFlags.IsTextRenderingModeDirty | VisualProxyFlags.IsTextHintingModeDirty;
			}
			UpdateCacheMode(channel, @null, visualProxyFlags, flag);
			UpdateTransform(channel, @null, visualProxyFlags, flag);
			UpdateClip(channel, @null, visualProxyFlags, flag);
			UpdateOffset(channel, @null, visualProxyFlags, flag);
			UpdateEffect(channel, @null, visualProxyFlags, flag);
			UpdateGuidelines(channel, @null, visualProxyFlags, flag);
			UpdateContent(ctx, visualProxyFlags, flag);
			UpdateOpacity(channel, @null, visualProxyFlags, flag);
			UpdateOpacityMask(channel, @null, visualProxyFlags, flag);
			UpdateRenderOptions(channel, @null, visualProxyFlags, flag);
			UpdateChildren(ctx, @null);
			UpdateScrollableAreaClip(channel, @null, visualProxyFlags, flag);
			SetFlags(channel, value: false, VisualProxyFlags.IsSubtreeDirtyForRender);
		}
		finally
		{
			Exit();
		}
	}

	internal bool Enter()
	{
		if (CheckFlagsAnd(VisualFlags.ReentrancyFlag))
		{
			return false;
		}
		SetFlags(value: true, VisualFlags.ReentrancyFlag);
		return true;
	}

	internal void Exit()
	{
		SetFlags(value: false, VisualFlags.ReentrancyFlag);
	}

	private void UpdateOpacity(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsOpacityDirty) != 0)
		{
			double value = OpacityField.GetValue(this);
			if (isOnChannel || !(value >= 1.0))
			{
				DUCE.CompositionNode.SetAlpha(handle, value, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsOpacityDirty);
		}
	}

	private void UpdateOpacityMask(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsOpacityMaskDirty) != 0)
		{
			Brush value = OpacityMaskField.GetValue(this);
			if (value != null)
			{
				DUCE.CompositionNode.SetAlphaMask(handle, ((DUCE.IResource)value).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.CompositionNode.SetAlphaMask(handle, DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsOpacityMaskDirty);
		}
	}

	private void UpdateTransform(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsTransformDirty) != 0)
		{
			Transform value = TransformField.GetValue(this);
			if (value != null)
			{
				DUCE.CompositionNode.SetTransform(handle, ((DUCE.IResource)value).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.CompositionNode.SetTransform(handle, DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsTransformDirty);
		}
	}

	private void UpdateEffect(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsEffectDirty) != 0)
		{
			Effect value = EffectField.GetValue(this);
			if (value != null)
			{
				DUCE.CompositionNode.SetEffect(handle, ((DUCE.IResource)value).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.CompositionNode.SetEffect(handle, DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsEffectDirty);
		}
	}

	private void UpdateCacheMode(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsCacheModeDirty) != 0)
		{
			CacheMode value = CacheModeField.GetValue(this);
			if (value != null)
			{
				DUCE.CompositionNode.SetCacheMode(handle, ((DUCE.IResource)value).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.CompositionNode.SetCacheMode(handle, DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsCacheModeDirty);
		}
	}

	private void UpdateClip(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsClipDirty) != 0)
		{
			Geometry value = ClipField.GetValue(this);
			if (value != null)
			{
				DUCE.CompositionNode.SetClip(handle, ((DUCE.IResource)value).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.CompositionNode.SetClip(handle, DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsClipDirty);
		}
	}

	private void UpdateScrollableAreaClip(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsScrollableAreaClipDirty) != 0)
		{
			Rect? value = ScrollableAreaClipField.GetValue(this);
			if (isOnChannel || value.HasValue)
			{
				DUCE.CompositionNode.SetScrollableAreaClip(handle, value, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsScrollableAreaClipDirty);
		}
	}

	private void UpdateOffset(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsOffsetDirty) != 0)
		{
			if (isOnChannel || _offset != default(Vector))
			{
				DUCE.CompositionNode.SetOffset(handle, _offset.X, _offset.Y, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsOffsetDirty);
		}
	}

	private void UpdateGuidelines(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsGuidelineCollectionDirty) != 0)
		{
			DoubleCollection value = GuidelinesXField.GetValue(this);
			DoubleCollection value2 = GuidelinesYField.GetValue(this);
			if (isOnChannel || value != null || value2 != null)
			{
				DUCE.CompositionNode.SetGuidelineCollection(handle, value, value2, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsGuidelineCollectionDirty);
		}
	}

	private void UpdateRenderOptions(DUCE.Channel channel, DUCE.ResourceHandle handle, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsEdgeModeDirty) != 0 || (flags & VisualProxyFlags.IsBitmapScalingModeDirty) != 0 || (flags & VisualProxyFlags.IsClearTypeHintDirty) != 0 || (flags & VisualProxyFlags.IsTextRenderingModeDirty) != 0 || (flags & VisualProxyFlags.IsTextHintingModeDirty) != 0)
		{
			MilRenderOptions renderOptions = default(MilRenderOptions);
			EdgeMode value = EdgeModeField.GetValue(this);
			if (isOnChannel || value != 0)
			{
				renderOptions.Flags |= MilRenderOptionFlags.EdgeMode;
				renderOptions.EdgeMode = value;
			}
			BitmapScalingMode value2 = BitmapScalingModeField.GetValue(this);
			if (isOnChannel || value2 != 0)
			{
				renderOptions.Flags |= MilRenderOptionFlags.BitmapScalingMode;
				renderOptions.BitmapScalingMode = value2;
			}
			ClearTypeHint value3 = ClearTypeHintField.GetValue(this);
			if (isOnChannel || value3 != 0)
			{
				renderOptions.Flags |= MilRenderOptionFlags.ClearTypeHint;
				renderOptions.ClearTypeHint = value3;
			}
			TextRenderingMode value4 = TextRenderingModeField.GetValue(this);
			if (isOnChannel || value4 != 0)
			{
				renderOptions.Flags |= MilRenderOptionFlags.TextRenderingMode;
				renderOptions.TextRenderingMode = value4;
			}
			TextHintingMode value5 = TextHintingModeField.GetValue(this);
			if (isOnChannel || value5 != 0)
			{
				renderOptions.Flags |= MilRenderOptionFlags.TextHintingMode;
				renderOptions.TextHintingMode = value5;
			}
			if (renderOptions.Flags != 0)
			{
				DUCE.CompositionNode.SetRenderOptions(handle, renderOptions, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.IsClearTypeHintDirty | VisualProxyFlags.IsEdgeModeDirty | VisualProxyFlags.IsBitmapScalingModeDirty | VisualProxyFlags.IsTextRenderingModeDirty | VisualProxyFlags.IsTextHintingModeDirty);
		}
	}

	private void UpdateContent(RenderContext ctx, VisualProxyFlags flags, bool isOnChannel)
	{
		if ((flags & VisualProxyFlags.IsContentDirty) != 0)
		{
			RenderContent(ctx, isOnChannel);
			SetFlags(ctx.Channel, value: false, VisualProxyFlags.IsContentDirty);
		}
	}

	private void UpdateChildren(RenderContext ctx, DUCE.ResourceHandle handle)
	{
		DUCE.Channel channel = ctx.Channel;
		uint num = (CheckFlagsAnd(channel, VisualProxyFlags.IsContentNodeConnected) ? 1u : 0u);
		bool flag = CheckFlagsAnd(channel, VisualProxyFlags.IsChildrenZOrderDirty);
		int visualChildrenCount = VisualChildrenCount;
		if (flag)
		{
			DUCE.CompositionNode.RemoveAllChildren(handle, channel);
		}
		for (int i = 0; i < visualChildrenCount; i++)
		{
			Visual visualChild = GetVisualChild(i);
			if (visualChild == null)
			{
				continue;
			}
			if (visualChild.CheckFlagsAnd(channel, VisualProxyFlags.IsSubtreeDirtyForRender) || !visualChild.IsOnChannel(channel))
			{
				visualChild.RenderRecursive(ctx);
			}
			if (visualChild.IsOnChannel(channel))
			{
				if (!visualChild.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent) || flag)
				{
					DUCE.CompositionNode.InsertChildAt(handle, ((DUCE.IResource)visualChild).GetHandle(channel), num, channel);
					visualChild.SetFlags(channel, value: true, VisualProxyFlags.IsConnectedToParent);
				}
				num++;
			}
		}
		SetFlags(channel, value: false, VisualProxyFlags.IsChildrenZOrderDirty);
	}

	internal void InvalidateHitTestBounds()
	{
		VerifyAPIReadWrite();
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.None);
	}

	internal virtual Rect GetHitTestBounds()
	{
		return GetContentBounds();
	}

	internal HitTestResult HitTest(Point point)
	{
		return HitTest(point, include2DOn3D: true);
	}

	internal HitTestResult HitTest(Point point, bool include2DOn3D)
	{
		TopMostHitResult topMostHitResult = new TopMostHitResult();
		VisualTreeHelper.HitTest(this, include2DOn3D ? null : new HitTestFilterCallback(topMostHitResult.NoNested2DFilter), topMostHitResult.HitTestResult, new PointHitTestParameters(point));
		return topMostHitResult._hitResult;
	}

	internal void HitTest(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters hitTestParameters)
	{
		if (resultCallback == null)
		{
			throw new ArgumentNullException("resultCallback");
		}
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		VerifyAPIReadWrite();
		Precompute();
		if (hitTestParameters is PointHitTestParameters { HitPoint: var hitPoint } pointHitTestParameters)
		{
			try
			{
				HitTestPoint(filterCallback, resultCallback, pointHitTestParameters);
				return;
			}
			catch
			{
				pointHitTestParameters.SetHitPoint(hitPoint);
				throw;
			}
			finally
			{
			}
		}
		if (hitTestParameters is GeometryHitTestParameters geometryHitTestParameters)
		{
			try
			{
				HitTestGeometry(filterCallback, resultCallback, geometryHitTestParameters);
				return;
			}
			catch
			{
				geometryHitTestParameters.EmergencyRestoreOriginalTransform();
				throw;
			}
		}
		Invariant.Assert(condition: false, string.Format(CultureInfo.InvariantCulture, "'{0}' HitTestParameters are not supported on {1}.", hitTestParameters.GetType().Name, GetType().Name));
	}

	internal HitTestResultBehavior HitTestPoint(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters pointParams)
	{
		Geometry visualClip = VisualClip;
		if (_bboxSubgraph.Contains(pointParams.HitPoint) && (visualClip == null || visualClip.FillContains(pointParams.HitPoint)))
		{
			HitTestFilterBehavior hitTestFilterBehavior = HitTestFilterBehavior.Continue;
			if (filterCallback != null)
			{
				hitTestFilterBehavior = filterCallback(this);
				switch (hitTestFilterBehavior)
				{
				case HitTestFilterBehavior.ContinueSkipSelfAndChildren:
					return HitTestResultBehavior.Continue;
				case HitTestFilterBehavior.Stop:
					return HitTestResultBehavior.Stop;
				}
			}
			Point hitPoint = pointParams.HitPoint;
			Point point = hitPoint;
			if (CheckFlagsAnd(VisualFlags.NodeHasEffect))
			{
				Effect value = EffectField.GetValue(this);
				if (value != null)
				{
					GeneralTransform inverse = value.EffectMapping.Inverse;
					if (inverse != Transform.Identity)
					{
						bool flag = false;
						Point? point2 = Effect.WorldToUnit(hitPoint, _bboxSubgraph);
						if (point2.HasValue)
						{
							Point result = default(Point);
							if (inverse.TryTransform(point2.Value, out result))
							{
								Point? point3 = Effect.UnitToWorld(result, _bboxSubgraph);
								if (point3.HasValue)
								{
									point = point3.Value;
									flag = true;
								}
							}
						}
						if (!flag)
						{
							return HitTestResultBehavior.Continue;
						}
					}
				}
			}
			if (hitTestFilterBehavior != HitTestFilterBehavior.ContinueSkipChildren)
			{
				for (int num = VisualChildrenCount - 1; num >= 0; num--)
				{
					Visual visualChild = GetVisualChild(num);
					if (visualChild == null)
					{
						continue;
					}
					Rect? value2 = ScrollableAreaClipField.GetValue(visualChild);
					if (value2.HasValue && !value2.Value.Contains(point))
					{
						continue;
					}
					Point hitPoint2 = point;
					hitPoint2 -= visualChild._offset;
					Transform value3 = TransformField.GetValue(visualChild);
					if (value3 != null)
					{
						Matrix value4 = value3.Value;
						if (!value4.HasInverse)
						{
							continue;
						}
						value4.Invert();
						hitPoint2 *= value4;
					}
					pointParams.SetHitPoint(hitPoint2);
					HitTestResultBehavior num2 = visualChild.HitTestPoint(filterCallback, resultCallback, pointParams);
					pointParams.SetHitPoint(hitPoint);
					if (num2 == HitTestResultBehavior.Stop)
					{
						return HitTestResultBehavior.Stop;
					}
				}
			}
			if (hitTestFilterBehavior != HitTestFilterBehavior.ContinueSkipSelf)
			{
				pointParams.SetHitPoint(point);
				HitTestResultBehavior num3 = HitTestPointInternal(filterCallback, resultCallback, pointParams);
				pointParams.SetHitPoint(hitPoint);
				if (num3 == HitTestResultBehavior.Stop)
				{
					return HitTestResultBehavior.Stop;
				}
			}
		}
		return HitTestResultBehavior.Continue;
	}

	internal GeneralTransform TransformToOuterSpace()
	{
		Matrix matrix = Matrix.Identity;
		GeneralTransformGroup generalTransformGroup = null;
		GeneralTransform generalTransform = null;
		if (CheckFlagsAnd(VisualFlags.NodeHasEffect))
		{
			Effect value = EffectField.GetValue(this);
			if (value != null)
			{
				GeneralTransform generalTransform2 = value.CoerceToUnitSpaceGeneralTransform(value.EffectMapping, VisualDescendantBounds);
				Transform affineTransform = generalTransform2.AffineTransform;
				if (affineTransform != null)
				{
					Matrix matrix2 = affineTransform.Value;
					MatrixUtil.MultiplyMatrix(ref matrix, ref matrix2);
				}
				else
				{
					generalTransformGroup = new GeneralTransformGroup();
					generalTransformGroup.Children.Add(generalTransform2);
				}
			}
			else
			{
				BitmapEffectStateField.GetValue(this);
			}
		}
		Transform value2 = TransformField.GetValue(this);
		if (value2 != null)
		{
			Matrix matrix3 = value2.Value;
			MatrixUtil.MultiplyMatrix(ref matrix, ref matrix3);
		}
		matrix.Translate(_offset.X, _offset.Y);
		if (generalTransformGroup == null)
		{
			generalTransform = new MatrixTransform(matrix);
		}
		else
		{
			generalTransformGroup.Children.Add(new MatrixTransform(matrix));
			generalTransform = generalTransformGroup;
		}
		generalTransform.Freeze();
		return generalTransform;
	}

	internal HitTestResultBehavior HitTestGeometry(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, GeometryHitTestParameters geometryParams)
	{
		Geometry visualClip = VisualClip;
		if (visualClip != null && visualClip.FillContainsWithDetail(geometryParams.InternalHitGeometry) == IntersectionDetail.Empty)
		{
			return HitTestResultBehavior.Continue;
		}
		if (_bboxSubgraph.IntersectsWith(geometryParams.Bounds))
		{
			HitTestFilterBehavior hitTestFilterBehavior = HitTestFilterBehavior.Continue;
			if (filterCallback != null)
			{
				hitTestFilterBehavior = filterCallback(this);
				switch (hitTestFilterBehavior)
				{
				case HitTestFilterBehavior.ContinueSkipSelfAndChildren:
					return HitTestResultBehavior.Continue;
				case HitTestFilterBehavior.Stop:
					return HitTestResultBehavior.Stop;
				}
			}
			int visualChildrenCount = VisualChildrenCount;
			if (hitTestFilterBehavior != HitTestFilterBehavior.ContinueSkipChildren)
			{
				for (int num = visualChildrenCount - 1; num >= 0; num--)
				{
					Visual visualChild = GetVisualChild(num);
					if (visualChild == null)
					{
						continue;
					}
					Rect? value = ScrollableAreaClipField.GetValue(visualChild);
					if (value.HasValue && new RectangleGeometry(value.Value).FillContainsWithDetail(geometryParams.InternalHitGeometry) == IntersectionDetail.Empty)
					{
						continue;
					}
					Matrix matrix = Matrix.Identity;
					matrix.Translate(0.0 - visualChild._offset.X, 0.0 - visualChild._offset.Y);
					Transform value2 = TransformField.GetValue(visualChild);
					if (value2 != null)
					{
						Matrix matrix2 = value2.Value;
						if (!matrix2.HasInverse)
						{
							continue;
						}
						matrix2.Invert();
						MatrixUtil.MultiplyMatrix(ref matrix, ref matrix2);
					}
					geometryParams.PushMatrix(ref matrix);
					HitTestResultBehavior num2 = visualChild.HitTestGeometry(filterCallback, resultCallback, geometryParams);
					geometryParams.PopMatrix();
					if (num2 == HitTestResultBehavior.Stop)
					{
						return HitTestResultBehavior.Stop;
					}
				}
			}
			if (hitTestFilterBehavior != HitTestFilterBehavior.ContinueSkipSelf)
			{
				GeometryHitTestResult geometryHitTestResult = HitTestCore(geometryParams);
				if (geometryHitTestResult != null)
				{
					return resultCallback(geometryHitTestResult);
				}
			}
		}
		return HitTestResultBehavior.Continue;
	}

	internal virtual HitTestResultBehavior HitTestPointInternal(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
	{
		HitTestResult hitTestResult = HitTestCore(hitTestParameters);
		if (hitTestResult != null)
		{
			return resultCallback(hitTestResult);
		}
		return HitTestResultBehavior.Continue;
	}

	/// <summary>Determines whether a point coordinate value is within the bounds of the visual object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.HitTestResult" /> that represents the <see cref="T:System.Windows.Media.Visual" /> that is returned from a hit test.</returns>
	/// <param name="hitTestParameters">A <see cref="T:System.Windows.Media.PointHitTestParameters" /> object that specifies the <see cref="T:System.Windows.Point" /> to hit test against.</param>
	protected virtual HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		if (GetHitTestBounds().Contains(hitTestParameters.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		return null;
	}

	/// <summary>Determines whether a geometry value is within the bounds of the visual object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.GeometryHitTestResult" /> that represents the result of the hit test.</returns>
	/// <param name="hitTestParameters">A <see cref="T:System.Windows.Media.GeometryHitTestParameters" /> object that specifies the <see cref="T:System.Windows.Media.Geometry" /> to hit test against.</param>
	protected virtual GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		if (hitTestParameters == null)
		{
			throw new ArgumentNullException("hitTestParameters");
		}
		IntersectionDetail intersectionDetail = new RectangleGeometry(GetHitTestBounds()).FillContainsWithDetail(hitTestParameters.InternalHitGeometry);
		if (intersectionDetail != IntersectionDetail.Empty)
		{
			return new GeometryHitTestResult(this, intersectionDetail);
		}
		return null;
	}

	/// <summary>Returns the specified <see cref="T:System.Windows.Media.Visual" /> in the parent <see cref="T:System.Windows.Media.VisualCollection" />. </summary>
	/// <returns>The child in the <see cref="T:System.Windows.Media.VisualCollection" /> at the specified <paramref name="index" /> value.</returns>
	/// <param name="index">The index of the visual object in the <see cref="T:System.Windows.Media.VisualCollection" />.</param>
	protected virtual Visual GetVisualChild(int index)
	{
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	internal Visual InternalGetVisualChild(int index)
	{
		return GetVisualChild(index);
	}

	internal virtual DependencyObject InternalGet2DOr3DVisualChild(int index)
	{
		return GetVisualChild(index);
	}

	internal void InternalAddVisualChild(Visual child)
	{
		AddVisualChild(child);
	}

	internal void InternalRemoveVisualChild(Visual child)
	{
		RemoveVisualChild(child);
	}

	/// <summary>Defines the parent-child relationship between two visuals.</summary>
	/// <param name="child">The child visual object to add to parent visual.</param>
	protected void AddVisualChild(Visual child)
	{
		if (child == null)
		{
			return;
		}
		if (child._parent != null)
		{
			throw new ArgumentException(SR.Visual_HasParent);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		SetFlags(value: true, VisualFlags.HasChildren);
		child._parent = this;
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		PropagateFlags(child, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		UIElement.PropagateResumeLayout(this, child);
		if (HwndTarget.IsProcessPerMonitorDpiAware == true && HwndTarget.IsPerMonitorDpiScalingEnabled)
		{
			bool flag = CheckFlagsAnd(VisualFlags.DpiScaleFlag1);
			bool flag2 = CheckFlagsAnd(VisualFlags.DpiScaleFlag2);
			int index = 0;
			if (flag && flag2)
			{
				index = DpiIndex.GetValue(this);
			}
			child.RecursiveSetDpiScaleVisualFlags(new DpiRecursiveChangeArgs(new DpiFlags(flag, flag2, index), child.GetDpi(), GetDpi()));
		}
		OnVisualChildrenChanged(child, null);
		child.FireOnVisualParentChanged(null);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: true);
	}

	/// <summary>Removes the parent-child relationship between two visuals.</summary>
	/// <param name="child">The child visual object to remove from the parent visual.</param>
	protected void RemoveVisualChild(Visual child)
	{
		if (child == null || child._parent == null)
		{
			return;
		}
		if (child._parent != this)
		{
			throw new ArgumentException(SR.Visual_NotChild);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: false);
		if (InternalVisual2DOr3DChildrenCount == 0)
		{
			SetFlags(value: false, VisualFlags.HasChildren);
		}
		for (int i = 0; i < _proxy.Count; i++)
		{
			DUCE.Channel channel = _proxy.GetChannel(i);
			if (child.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent))
			{
				child.SetFlags(channel, value: false, VisualProxyFlags.IsConnectedToParent);
				((DUCE.IResource)child).RemoveChildFromParent((DUCE.IResource)this, channel);
				((DUCE.IResource)child).ReleaseOnChannel(channel);
			}
		}
		child._parent = null;
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		UIElement.PropagateSuspendLayout(child);
		child.FireOnVisualParentChanged(this);
		OnVisualChildrenChanged(null, child);
	}

	[FriendAccessAllowed]
	internal void InvalidateZOrder()
	{
		if (VisualChildrenCount != 0)
		{
			PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsChildrenZOrderDirty);
			InputManager.SafeCurrentNotifyHitTestInvalidated();
		}
	}

	[FriendAccessAllowed]
	internal void InternalSetOffsetWorkaround(Vector offset)
	{
		VisualOffset = offset;
	}

	[FriendAccessAllowed]
	internal void InternalSetTransformWorkaround(Transform transform)
	{
		VisualTransform = transform;
	}

	internal void BitmapEffectEmulationChanged(object sender, EventArgs e)
	{
		BitmapEffectState value = UserProvidedBitmapEffectData.GetValue(this);
		BitmapEffect bitmapEffect = value?.BitmapEffect;
		BitmapEffectInput bitmapEffectInput = value?.BitmapEffectInput;
		if (bitmapEffect == null)
		{
			VisualBitmapEffectInternal = null;
			VisualBitmapEffectInputInternal = null;
			VisualEffectInternal = null;
		}
		else if (bitmapEffectInput != null)
		{
			VisualEffectInternal = null;
			VisualBitmapEffectInternal = bitmapEffect;
			VisualBitmapEffectInputInternal = bitmapEffectInput;
		}
		else if (RenderCapability.IsShaderEffectSoftwareRenderingSupported && bitmapEffect.CanBeEmulatedUsingEffectPipeline() && !CheckFlagsAnd(VisualFlags.BitmapEffectEmulationDisabled))
		{
			VisualBitmapEffectInternal = null;
			VisualBitmapEffectInputInternal = null;
			Effect emulatingEffect = bitmapEffect.GetEmulatingEffect();
			VisualEffectInternal = emulatingEffect;
		}
		else
		{
			VisualEffectInternal = null;
			VisualBitmapEffectInputInternal = null;
			VisualBitmapEffectInternal = bitmapEffect;
		}
	}

	internal void ChangeVisualClip(Geometry newClip, bool dontSetWhenClose)
	{
		VerifyAPIReadWrite(newClip);
		Geometry value = ClipField.GetValue(this);
		if (value == newClip || (dontSetWhenClose && value != null && newClip != null && value.AreClose(newClip)))
		{
			return;
		}
		if (newClip != null && !newClip.IsFrozen)
		{
			newClip.Changed += ClipChangedHandler;
		}
		if (value != null)
		{
			if (!value.IsFrozen)
			{
				value.Changed -= ClipChangedHandler;
			}
			DisconnectAttachedResource(VisualProxyFlags.IsClipDirty, value);
		}
		ClipField.SetValue(this, newClip);
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsClipDirty);
		ClipChanged(null, null);
	}

	internal void DisconnectAttachedResource(VisualProxyFlags correspondingFlag, DUCE.IResource attachedResource)
	{
		bool flag = correspondingFlag == VisualProxyFlags.IsContentConnected;
		for (int i = 0; i < _proxy.Count; i++)
		{
			DUCE.Channel channel = _proxy.GetChannel(i);
			if ((_proxy.GetFlags(i) & correspondingFlag) != 0 == flag)
			{
				SetFlags(channel, value: true, correspondingFlag);
				attachedResource.ReleaseOnChannel(channel);
				if (flag)
				{
					_proxy.SetFlags(i, value: false, VisualProxyFlags.IsContentConnected);
				}
			}
		}
	}

	internal virtual DrawingGroup GetDrawing()
	{
		VerifyAPIReadOnly();
		return null;
	}

	internal virtual void FireOnVisualParentChanged(DependencyObject oldParent)
	{
		OnVisualParentChanged(oldParent);
		if (oldParent == null)
		{
			if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
			{
				SetTreeBits(_parent, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
			}
		}
		else if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
		{
			ClearTreeBits(oldParent, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
		}
		AncestorChangedEventArgs args = new AncestorChangedEventArgs(this, oldParent);
		ProcessAncestorChangedNotificationRecursive(this, args);
	}

	/// <summary>Called when the parent of the visual object is changed.</summary>
	/// <param name="oldParent">A value of type <see cref="T:System.Windows.DependencyObject" /> that represents the previous parent of the <see cref="T:System.Windows.Media.Visual" /> object. If the <see cref="T:System.Windows.Media.Visual" /> object did not have a previous parent, the value of the parameter is null.</param>
	protected internal virtual void OnVisualParentChanged(DependencyObject oldParent)
	{
	}

	/// <summary>Called when the <see cref="T:System.Windows.Media.VisualCollection" /> of the visual object is modified.</summary>
	/// <param name="visualAdded">The <see cref="T:System.Windows.Media.Visual" /> that was added to the collection</param>
	/// <param name="visualRemoved">The <see cref="T:System.Windows.Media.Visual" /> that was removed from the collection</param>
	protected internal virtual void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
	}

	protected virtual void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
	{
	}

	internal static void ProcessAncestorChangedNotificationRecursive(DependencyObject e, AncestorChangedEventArgs args)
	{
		if (e is Visual3D)
		{
			Visual3D.ProcessAncestorChangedNotificationRecursive(e, args);
			return;
		}
		Visual visual = e as Visual;
		if (!visual.CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
		{
			return;
		}
		AncestorChangedEventField.GetValue(visual)?.Invoke(visual, args);
		int internalVisual2DOr3DChildrenCount = visual.InternalVisual2DOr3DChildrenCount;
		for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
		{
			DependencyObject dependencyObject = visual.InternalGet2DOr3DVisualChild(i);
			if (dependencyObject != null)
			{
				ProcessAncestorChangedNotificationRecursive(dependencyObject, args);
			}
		}
	}

	/// <summary>Determines whether the visual object is an ancestor of the descendant visual object.</summary>
	/// <returns>true if the visual object is an ancestor of <paramref name="descendant" />; otherwise, false.</returns>
	/// <param name="descendant">A value of type <see cref="T:System.Windows.DependencyObject" />.</param>
	public bool IsAncestorOf(DependencyObject descendant)
	{
		VisualTreeUtils.AsNonNullVisual(descendant, out var visual, out var visual3D);
		return visual3D?.IsDescendantOf(this) ?? visual.IsDescendantOf(this);
	}

	/// <summary>Determines whether the visual object is a descendant of the ancestor visual object.</summary>
	/// <returns>true if the visual object is a descendant of <paramref name="ancestor" />; otherwise, false.</returns>
	/// <param name="ancestor">A value of type <see cref="T:System.Windows.DependencyObject" />.</param>
	public bool IsDescendantOf(DependencyObject ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VisualTreeUtils.EnsureVisual(ancestor);
		DependencyObject dependencyObject = this;
		while (dependencyObject != null && dependencyObject != ancestor)
		{
			dependencyObject = ((dependencyObject is Visual visual) ? visual._parent : ((!(dependencyObject is Visual3D visual3D)) ? null : visual3D.InternalVisualParent));
		}
		return dependencyObject == ancestor;
	}

	internal void SetFlagsToRoot(bool value, VisualFlags flag)
	{
		Visual visual = this;
		do
		{
			visual.SetFlags(value, flag);
			Visual visual2 = visual._parent as Visual;
			if (visual._parent != null && visual2 == null)
			{
				((Visual3D)visual._parent).SetFlagsToRoot(value, flag);
				break;
			}
			visual = visual2;
		}
		while (visual != null);
	}

	internal DependencyObject FindFirstAncestorWithFlagsAnd(VisualFlags flag)
	{
		Visual visual = this;
		do
		{
			if (visual.CheckFlagsAnd(flag))
			{
				return visual;
			}
			DependencyObject parent = visual._parent;
			visual = parent as Visual;
			if (visual == null && parent is Visual3D visual3D)
			{
				return visual3D.FindFirstAncestorWithFlagsAnd(flag);
			}
		}
		while (visual != null);
		return null;
	}

	/// <summary>Returns the common ancestor of two visual objects.</summary>
	/// <returns>The common ancestor of the visual object and <paramref name="otherVisual" /> if one exists; otherwise, null.</returns>
	/// <param name="otherVisual">A visual object of type <see cref="T:System.Windows.DependencyObject" />.</param>
	public DependencyObject FindCommonVisualAncestor(DependencyObject otherVisual)
	{
		VerifyAPIReadOnly(otherVisual);
		if (otherVisual == null)
		{
			throw new ArgumentNullException("otherVisual");
		}
		SetFlagsToRoot(value: false, VisualFlags.FindCommonAncestor);
		VisualTreeUtils.SetFlagsToRoot(otherVisual, value: true, VisualFlags.FindCommonAncestor);
		return FindFirstAncestorWithFlagsAnd(VisualFlags.FindCommonAncestor);
	}

	internal virtual void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
	{
		UIElement.InvalidateForceInheritPropertyOnChildren(this, property);
	}

	/// <summary>Returns a transform that can be used to transform coordinates from the <see cref="T:System.Windows.Media.Visual" /> to the specified <see cref="T:System.Windows.Media.Visual" /> ancestor of the visual object.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.GeneralTransform" />.</returns>
	/// <param name="ancestor">The <see cref="T:System.Windows.Media.Visual" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ancestor" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="ancestor" /> is not an ancestor of the visual.</exception>
	/// <exception cref="T:System.InvalidOperationException">The visual objects are not related.</exception>
	public GeneralTransform TransformToAncestor(Visual ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VerifyAPIReadOnly(ancestor);
		return InternalTransformToAncestor(ancestor, inverse: false);
	}

	/// <summary>Returns a transform that can be used to transform coordinates from the <see cref="T:System.Windows.Media.Visual" /> to the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> ancestor of the visual object.</summary>
	/// <returns>A transform that can be used to transform coordinates from the <see cref="T:System.Windows.Media.Visual" /> to the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> ancestor of the visual object.</returns>
	/// <param name="ancestor">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> to which the coordinates are transformed.</param>
	public GeneralTransform2DTo3D TransformToAncestor(Visual3D ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VerifyAPIReadOnly(ancestor);
		return InternalTransformToAncestor(ancestor, inverse: false);
	}

	/// <summary>Returns a transform that can be used to transform coordinates from the <see cref="T:System.Windows.Media.Visual" /> to the specified visual object descendant.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.GeneralTransform" />.</returns>
	/// <param name="descendant">The <see cref="T:System.Windows.Media.Visual" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="descendant" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The visual is not an ancestor of the <paramref name="descendant" /> visual.</exception>
	/// <exception cref="T:System.InvalidOperationException">The visual objects are not related.</exception>
	public GeneralTransform TransformToDescendant(Visual descendant)
	{
		if (descendant == null)
		{
			throw new ArgumentNullException("descendant");
		}
		VerifyAPIReadOnly(descendant);
		return descendant.InternalTransformToAncestor(this, inverse: true);
	}

	/// <summary>Returns a transform that can be used to transform coordinates from the <see cref="T:System.Windows.Media.Visual" /> to the specified visual object.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.GeneralTransform" />.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="visual" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The visual objects are not related.</exception>
	public GeneralTransform TransformToVisual(Visual visual)
	{
		if (!(FindCommonVisualAncestor(visual) is Visual ancestor))
		{
			throw new InvalidOperationException(SR.Visual_NoCommonAncestor);
		}
		GeneralTransform generalTransform;
		Matrix simpleTransform;
		bool flag = TrySimpleTransformToAncestor(ancestor, inverse: false, out generalTransform, out simpleTransform);
		GeneralTransform generalTransform2;
		Matrix simpleTransform2;
		bool flag2 = visual.TrySimpleTransformToAncestor(ancestor, inverse: true, out generalTransform2, out simpleTransform2);
		if (flag && flag2)
		{
			MatrixUtil.MultiplyMatrix(ref simpleTransform, ref simpleTransform2);
			MatrixTransform matrixTransform = new MatrixTransform(simpleTransform);
			matrixTransform.Freeze();
			return matrixTransform;
		}
		if (flag)
		{
			generalTransform = new MatrixTransform(simpleTransform);
			generalTransform.Freeze();
		}
		else if (flag2)
		{
			generalTransform2 = new MatrixTransform(simpleTransform2);
			generalTransform2.Freeze();
		}
		if (generalTransform2 != null)
		{
			GeneralTransformGroup generalTransformGroup = new GeneralTransformGroup();
			generalTransformGroup.Children.Add(generalTransform);
			generalTransformGroup.Children.Add(generalTransform2);
			generalTransformGroup.Freeze();
			return generalTransformGroup;
		}
		return generalTransform;
	}

	private GeneralTransform InternalTransformToAncestor(Visual ancestor, bool inverse)
	{
		if (TrySimpleTransformToAncestor(ancestor, inverse, out var generalTransform, out var simpleTransform))
		{
			MatrixTransform matrixTransform = new MatrixTransform(simpleTransform);
			matrixTransform.Freeze();
			return matrixTransform;
		}
		return generalTransform;
	}

	internal bool TrySimpleTransformToAncestor(Visual ancestor, bool inverse, out GeneralTransform generalTransform, out Matrix simpleTransform)
	{
		bool flag = false;
		DependencyObject dependencyObject = this;
		Matrix matrix = Matrix.Identity;
		GeneralTransformGroup generalTransformGroup = null;
		while (VisualTreeHelper.GetParent(dependencyObject) != null && dependencyObject != ancestor)
		{
			if (dependencyObject is Visual visual)
			{
				if (visual.CheckFlagsAnd(VisualFlags.NodeHasEffect))
				{
					Effect value = EffectField.GetValue(visual);
					if (value != null)
					{
						GeneralTransform generalTransform2 = value.CoerceToUnitSpaceGeneralTransform(value.EffectMapping, visual.VisualDescendantBounds);
						Transform affineTransform = generalTransform2.AffineTransform;
						if (affineTransform != null)
						{
							Matrix matrix2 = affineTransform.Value;
							MatrixUtil.MultiplyMatrix(ref matrix, ref matrix2);
						}
						else
						{
							if (generalTransformGroup == null)
							{
								generalTransformGroup = new GeneralTransformGroup();
							}
							generalTransformGroup.Children.Add(new MatrixTransform(matrix));
							matrix = Matrix.Identity;
							generalTransformGroup.Children.Add(generalTransform2);
						}
					}
				}
				Transform value2 = TransformField.GetValue(visual);
				if (value2 != null)
				{
					Matrix matrix3 = value2.Value;
					MatrixUtil.MultiplyMatrix(ref matrix, ref matrix3);
				}
				matrix.Translate(visual._offset.X, visual._offset.Y);
				dependencyObject = visual._parent;
			}
			else
			{
				Viewport2DVisual3D viewport2DVisual3D = dependencyObject as Viewport2DVisual3D;
				if (generalTransformGroup == null)
				{
					generalTransformGroup = new GeneralTransformGroup();
				}
				generalTransformGroup.Children.Add(new MatrixTransform(matrix));
				matrix = Matrix.Identity;
				Visual visual2 = null;
				if (flag)
				{
					visual2 = viewport2DVisual3D.Visual;
				}
				else
				{
					visual2 = this;
					flag = true;
				}
				generalTransformGroup.Children.Add(new GeneralTransform2DTo3DTo2D(viewport2DVisual3D, visual2));
				dependencyObject = VisualTreeHelper.GetContainingVisual2D(viewport2DVisual3D);
			}
		}
		if (dependencyObject != ancestor)
		{
			throw new InvalidOperationException(inverse ? SR.Visual_NotADescendant : SR.Visual_NotAnAncestor);
		}
		if (generalTransformGroup != null)
		{
			if (!matrix.IsIdentity)
			{
				generalTransformGroup.Children.Add(new MatrixTransform(matrix));
			}
			if (inverse)
			{
				generalTransformGroup = (GeneralTransformGroup)generalTransformGroup.Inverse;
			}
			generalTransformGroup?.Freeze();
			generalTransform = generalTransformGroup;
			simpleTransform = default(Matrix);
			return false;
		}
		generalTransform = null;
		if (inverse)
		{
			if (!matrix.HasInverse)
			{
				simpleTransform = default(Matrix);
				return false;
			}
			matrix.Invert();
		}
		simpleTransform = matrix;
		return true;
	}

	private GeneralTransform2DTo3D InternalTransformToAncestor(Visual3D ancestor, bool inverse)
	{
		GeneralTransform2DTo3D transformTo3D = null;
		if (TrySimpleTransformToAncestor(ancestor, out transformTo3D))
		{
			transformTo3D.Freeze();
			return transformTo3D;
		}
		return null;
	}

	internal bool TrySimpleTransformToAncestor(Visual3D ancestor, out GeneralTransform2DTo3D transformTo3D)
	{
		if (!(VisualTreeHelper.GetContainingVisual3D(this) is Viewport2DVisual3D viewport2DVisual3D))
		{
			throw new InvalidOperationException(SR.Visual_NotAnAncestor);
		}
		GeneralTransform transform2D = TransformToAncestor(viewport2DVisual3D.Visual);
		GeneralTransform3D transform3D = viewport2DVisual3D.TransformToAncestor(ancestor);
		transformTo3D = new GeneralTransform2DTo3D(transform2D, viewport2DVisual3D, transform3D);
		return true;
	}

	internal DpiScale GetDpi()
	{
		DpiScale result;
		lock (UIElement.DpiLock)
		{
			if (UIElement.DpiScaleXValues.Count == 0)
			{
				return UIElement.EnsureDpiScale();
			}
			result = new DpiScale(UIElement.DpiScaleXValues[0], UIElement.DpiScaleYValues[0]);
			int num = 0;
			num = (CheckFlagsAnd(VisualFlags.DpiScaleFlag1) ? (num | 1) : num);
			num = (CheckFlagsAnd(VisualFlags.DpiScaleFlag2) ? (num | 2) : num);
			if (num < 3 && UIElement.DpiScaleXValues[num] != 0.0 && UIElement.DpiScaleYValues[num] != 0.0)
			{
				result = new DpiScale(UIElement.DpiScaleXValues[num], UIElement.DpiScaleYValues[num]);
			}
			else if (num >= 3)
			{
				int value = DpiIndex.GetValue(this);
				result = new DpiScale(UIElement.DpiScaleXValues[value], UIElement.DpiScaleYValues[value]);
			}
		}
		return result;
	}

	/// <summary>Converts a <see cref="T:System.Windows.Point" /> that represents the current coordinate system of the <see cref="T:System.Windows.Media.Visual" /> into a <see cref="T:System.Windows.Point" /> in screen coordinates.</summary>
	/// <returns>The converted <see cref="T:System.Windows.Point" /> value in screen coordinates.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> value that represents the current coordinate system of the <see cref="T:System.Windows.Media.Visual" />.</param>
	public Point PointToScreen(Point point)
	{
		VerifyAPIReadOnly();
		PresentationSource presentationSource = PresentationSource.FromVisual(this);
		if (presentationSource == null)
		{
			throw new InvalidOperationException(SR.Visual_NoPresentationSource);
		}
		GeneralTransform generalTransform = TransformToAncestor(presentationSource.RootVisual);
		if (generalTransform == null || !generalTransform.TryTransform(point, out point))
		{
			throw new InvalidOperationException(SR.Visual_CannotTransformPoint);
		}
		point = PointUtil.RootToClient(point, presentationSource);
		point = PointUtil.ClientToScreen(point, presentationSource);
		return point;
	}

	/// <summary>Converts a <see cref="T:System.Windows.Point" /> in screen coordinates into a <see cref="T:System.Windows.Point" /> that represents the current coordinate system of the <see cref="T:System.Windows.Media.Visual" />.</summary>
	/// <returns>The converted <see cref="T:System.Windows.Point" /> value that represents the current coordinate system of the <see cref="T:System.Windows.Media.Visual" />.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> value in screen coordinates.</param>
	public Point PointFromScreen(Point point)
	{
		VerifyAPIReadOnly();
		PresentationSource presentationSource = PresentationSource.FromVisual(this);
		if (presentationSource == null)
		{
			throw new InvalidOperationException(SR.Visual_NoPresentationSource);
		}
		point = PointUtil.ScreenToClient(point, presentationSource);
		point = PointUtil.ClientToRoot(point, presentationSource);
		GeneralTransform generalTransform = presentationSource.RootVisual.TransformToDescendant(this);
		if (generalTransform == null || !generalTransform.TryTransform(point, out point))
		{
			throw new InvalidOperationException(SR.Visual_CannotTransformPoint);
		}
		return point;
	}

	internal void ClipChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void ScrollableAreaClipChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void TransformChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void EffectChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void CacheModeChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void GuidelinesChanged(object sender, EventArgs e)
	{
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsGuidelineCollectionDirty);
		PropagateChangedFlags();
	}

	internal void OpacityMaskChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal virtual void ContentsChanged(object sender, EventArgs e)
	{
		PropagateChangedFlags();
	}

	internal void SetFlagsOnAllChannels(bool value, VisualProxyFlags flagsToChange)
	{
		_proxy.SetFlagsOnAllChannels(value, flagsToChange);
	}

	internal void SetFlags(DUCE.Channel channel, bool value, VisualProxyFlags flagsToChange)
	{
		_proxy.SetFlags(channel, value, flagsToChange);
	}

	internal void SetFlags(bool value, VisualFlags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	internal void SetDpiScaleVisualFlags(DpiRecursiveChangeArgs args)
	{
		_flags = (args.DpiScaleFlag1 ? (_flags | VisualFlags.DpiScaleFlag1) : ((VisualFlags)((uint)_flags & 0xFFF7FFFFu)));
		_flags = (args.DpiScaleFlag2 ? (_flags | VisualFlags.DpiScaleFlag2) : ((VisualFlags)((uint)_flags & 0xFFEFFFFFu)));
		if (args.DpiScaleFlag1 && args.DpiScaleFlag2)
		{
			DpiIndex.SetValue(this, args.Index);
		}
		if (!args.OldDpiScale.Equals(args.NewDpiScale))
		{
			OnDpiChanged(args.OldDpiScale, args.NewDpiScale);
		}
	}

	internal void RecursiveSetDpiScaleVisualFlags(DpiRecursiveChangeArgs args)
	{
		SetDpiScaleVisualFlags(args);
		int internalVisualChildrenCount = InternalVisualChildrenCount;
		for (int i = 0; i < internalVisualChildrenCount; i++)
		{
			InternalGetVisualChild(i)?.RecursiveSetDpiScaleVisualFlags(args);
		}
	}

	internal bool CheckFlagsOnAllChannels(VisualProxyFlags flagsToCheck)
	{
		return _proxy.CheckFlagsOnAllChannels(flagsToCheck);
	}

	internal bool CheckFlagsAnd(DUCE.Channel channel, VisualProxyFlags flagsToCheck)
	{
		return (_proxy.GetFlags(channel) & flagsToCheck) == flagsToCheck;
	}

	internal bool CheckFlagsAnd(VisualFlags flags)
	{
		return (_flags & flags) == flags;
	}

	internal bool CheckFlagsOr(DUCE.Channel channel, VisualProxyFlags flagsToCheck)
	{
		return (_proxy.GetFlags(channel) & flagsToCheck) != 0;
	}

	internal bool CheckFlagsOr(VisualFlags flags)
	{
		if (flags != 0)
		{
			return (_flags & flags) != 0;
		}
		return true;
	}

	internal static void SetTreeBits(DependencyObject e, VisualFlags treeFlag, VisualFlags nodeFlag)
	{
		if (e != null)
		{
			if (e is Visual visual)
			{
				visual.SetFlags(value: true, nodeFlag);
			}
			else
			{
				((Visual3D)e).SetFlags(value: true, nodeFlag);
			}
		}
		while (e != null)
		{
			if (e is Visual visual2)
			{
				if (visual2.CheckFlagsAnd(treeFlag))
				{
					break;
				}
				visual2.SetFlags(value: true, treeFlag);
			}
			else
			{
				Visual3D visual3D = e as Visual3D;
				if (visual3D.CheckFlagsAnd(treeFlag))
				{
					break;
				}
				visual3D.SetFlags(value: true, treeFlag);
			}
			e = VisualTreeHelper.GetParent(e);
		}
	}

	internal static void ClearTreeBits(DependencyObject e, VisualFlags treeFlag, VisualFlags nodeFlag)
	{
		if (e != null)
		{
			if (e is Visual visual)
			{
				visual.SetFlags(value: false, nodeFlag);
			}
			else
			{
				((Visual3D)e).SetFlags(value: false, nodeFlag);
			}
		}
		while (e != null)
		{
			if (e is Visual visual2)
			{
				if (visual2.CheckFlagsAnd(nodeFlag) || DoAnyChildrenHaveABitSet(visual2, treeFlag))
				{
					break;
				}
				visual2.SetFlags(value: false, treeFlag);
			}
			else
			{
				Visual3D visual3D = e as Visual3D;
				if (visual3D.CheckFlagsAnd(nodeFlag) || Visual3D.DoAnyChildrenHaveABitSet(visual3D, treeFlag))
				{
					break;
				}
				visual3D.SetFlags(value: false, treeFlag);
			}
			e = VisualTreeHelper.GetParent(e);
		}
	}

	private static bool DoAnyChildrenHaveABitSet(Visual pe, VisualFlags flag)
	{
		int visualChildrenCount = pe.VisualChildrenCount;
		for (int i = 0; i < visualChildrenCount; i++)
		{
			Visual visualChild = pe.GetVisualChild(i);
			if (visualChild != null && visualChild.CheckFlagsAnd(flag))
			{
				return true;
			}
		}
		return false;
	}

	internal static void PropagateFlags(Visual e, VisualFlags flags, VisualProxyFlags proxyFlags)
	{
		while (e != null && (!e.CheckFlagsAnd(flags) || !e.CheckFlagsOnAllChannels(proxyFlags)))
		{
			if (e.CheckFlagsOr(VisualFlags.ShouldPostRender))
			{
				MediaContext mediaContext = MediaContext.From(e.Dispatcher);
				if (mediaContext.Channel != null)
				{
					mediaContext.PostRender();
				}
			}
			else if (e.CheckFlagsAnd(VisualFlags.NodeIsCyclicBrushRoot))
			{
				foreach (ICyclicBrush key in CyclicBrushToChannelsMapField.GetValue(e).Keys)
				{
					key.FireOnChanged();
				}
			}
			e.SetFlags(value: true, flags);
			e.SetFlagsOnAllChannels(value: true, proxyFlags);
			if (e._parent == null)
			{
				break;
			}
			if (!(e._parent is Visual visual))
			{
				Visual3D.PropagateFlags((Visual3D)e._parent, flags, proxyFlags);
				break;
			}
			e = visual;
		}
	}

	internal void PropagateChangedFlags()
	{
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
	}
}
