using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Diagnostics;
using System.Windows.Markup;
using System.Windows.Media.Composition;
using System.Windows.Media.Effects;
using MS.Internal;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Renders the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> children within the specified 2D viewport bounds.</summary>
[ContentProperty("Children")]
public sealed class Viewport3DVisual : Visual, DUCE.IResource, IVisual3DContainer
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport3DVisual.Camera" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Viewport3DVisual.Camera" /> dependency property.</returns>
	public static readonly DependencyProperty CameraProperty = DependencyProperty.Register("Camera", typeof(Camera), typeof(Viewport3DVisual), new PropertyMetadata(FreezableOperations.GetAsFrozen(new PerspectiveCamera()), CameraPropertyChanged), (object _003Cp0_003E) => MediaContext.CurrentMediaContext.WriteAccessEnabled);

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Viewport3DVisual.Viewport" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Viewport3DVisual.Viewport" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register("Viewport", typeof(Rect), typeof(Viewport3DVisual), new PropertyMetadata(Rect.Empty, ViewportPropertyChanged), (object _003Cp0_003E) => MediaContext.CurrentMediaContext.WriteAccessEnabled);

	private VisualProxy _proxy3D;

	private Rect3D _bboxChildrenSubgraph3D;

	private readonly Visual3DCollection _children;

	private DependencyObject _inheritanceContextForChildren;

	/// <summary>Gets the parent <see cref="T:System.Windows.Media.Visual" /> for the Viewport3DVisual.</summary>
	/// <returns>Visual parent of the Viewport3DVisual.</returns>
	public DependencyObject Parent => base.VisualParent;

	/// <summary>Gets or sets the clipping region of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Geometry" /> that defines the clipping area.</returns>
	public Geometry Clip
	{
		get
		{
			return base.VisualClip;
		}
		set
		{
			base.VisualClip = value;
		}
	}

	/// <summary>Gets or sets the opacity of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>The value of the Opacity property is expressed as a value between 0 and 1, specifying a range from fully transparent to fully opaque. A value of 0 indicates that the Opacity is completely transparent, while a value of 1 indicates that the Opacity is completely opaque. A value 0.5 would indicate the Opacity is 50% opaque, a value of 0.725 would indicate the Opacity is 72.5% opaque, and so on. Values less than 0 are treated as 0, while values greater than 1 are treated as 1.</returns>
	public double Opacity
	{
		get
		{
			return base.VisualOpacity;
		}
		set
		{
			base.VisualOpacity = value;
		}
	}

	/// <summary>Gets or sets the opacity mask value of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Brush" /> that represents the opacity mask value of the Viewport3DVisual.</returns>
	public Brush OpacityMask
	{
		get
		{
			return base.VisualOpacityMask;
		}
		set
		{
			base.VisualOpacityMask = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> applied to the Viewport3DVisual.</summary>
	/// <returns>BitmapEffect applied to the Viewport3DVisual.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffect BitmapEffect
	{
		get
		{
			return base.VisualBitmapEffect;
		}
		set
		{
			base.VisualBitmapEffect = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> applied to the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />. </summary>
	/// <returns>BitmapEffectInput applied to the Viewport3DVisual.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapEffectInput BitmapEffectInput
	{
		get
		{
			return base.VisualBitmapEffectInput;
		}
		set
		{
			base.VisualBitmapEffectInput = value;
		}
	}

	/// <summary>Gets the bounding box for the contents of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Rect" /> that defines the bounding box.</returns>
	public Rect ContentBounds => base.VisualContentBounds;

	/// <summary>Gets or sets the transform value of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Transform" /> applied to the Viewport3DVisual.</returns>
	public Transform Transform
	{
		get
		{
			return base.VisualTransform;
		}
		set
		{
			base.VisualTransform = value;
		}
	}

	/// <summary>Gets or sets the offset value of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Vector" /> that represents the offset value of the Viewport3DVisual.</returns>
	public Vector Offset
	{
		get
		{
			return base.VisualOffset;
		}
		set
		{
			base.VisualOffset = value;
		}
	}

	/// <summary>Gets the union of all of the content bounding boxes for all of the descendants of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />, but not including the contents of the Viewport3DVisual.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Rect" /> that defines the union.</returns>
	public Rect DescendantBounds => base.VisualDescendantBounds;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Camera" /> used by the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.  </summary>
	/// <returns>Camera used by the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</returns>
	public Camera Camera
	{
		get
		{
			return (Camera)GetValue(CameraProperty);
		}
		set
		{
			SetValue(CameraProperty, value);
		}
	}

	/// <summary>Gets or sets the rectangle in which the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" /> will be rendered.  </summary>
	/// <returns>Rectangle in which the contents of the Viewport3D will be rendered.</returns>
	public Rect Viewport
	{
		get
		{
			return (Rect)GetValue(ViewportProperty);
		}
		set
		{
			SetValue(ViewportProperty, value);
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects contained by <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />. </summary>
	/// <returns>Collection of the objects contained by the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public Visual3DCollection Children => _children;

	internal override int InternalVisual2DOr3DChildrenCount => Children.Count;

	private Rect3D BBoxSubgraph => _bboxChildrenSubgraph3D;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" /> class.</summary>
	public Viewport3DVisual()
		: base(DUCE.ResourceType.TYPE_VIEWPORT3DVISUAL)
	{
		_children = new Visual3DCollection(this);
	}

	/// <summary>Returns the top-most visual object of a hit test performed at a specified <see cref="T:System.Windows.Point" />.</summary>
	/// <returns>The hit test result of the visual returned as a <see cref="T:System.Windows.Media.HitTestResult" /> type.</returns>
	/// <param name="point">Point against which to hit test.</param>
	public new HitTestResult HitTest(Point point)
	{
		return base.HitTest(point);
	}

	/// <summary>Initiate a hit test on the <see cref="T:System.Windows.Media.Media3D.Viewport3DVisual" /> by using the <see cref="T:System.Windows.Media.HitTestFilterCallback" /> and <see cref="T:System.Windows.Media.HitTestResultCallback" /> objects.</summary>
	/// <param name="filterCallback">Value of type HitTestFilterCallback.</param>
	/// <param name="resultCallback">Value of type HitTestResultCallback.</param>
	/// <param name="hitTestParameters">Value of type <see cref="T:System.Windows.Media.HitTestParameters" />.</param>
	public new void HitTest(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters hitTestParameters)
	{
		base.HitTest(filterCallback, resultCallback, hitTestParameters);
	}

	private static void CameraPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Viewport3DVisual viewport3DVisual = (Viewport3DVisual)d;
		if (!e.IsASubPropertyChange)
		{
			if (e.OldValue != null)
			{
				viewport3DVisual.DisconnectAttachedResource(VisualProxyFlags.Viewport3DVisual_IsCameraDirty, (DUCE.IResource)e.OldValue);
			}
			viewport3DVisual.SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty | VisualProxyFlags.Viewport3DVisual_IsCameraDirty);
		}
		viewport3DVisual.ContentsChanged(viewport3DVisual, EventArgs.Empty);
	}

	private static void ViewportPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Viewport3DVisual obj = (Viewport3DVisual)d;
		obj.SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty | VisualProxyFlags.Viewport3DVisual_IsViewportDirty);
		obj.ContentsChanged(obj, EventArgs.Empty);
	}

	void IVisual3DContainer.VerifyAPIReadOnly()
	{
		VerifyAPIReadOnly();
	}

	void IVisual3DContainer.VerifyAPIReadOnly(DependencyObject other)
	{
		VerifyAPIReadOnly(other);
	}

	void IVisual3DContainer.VerifyAPIReadWrite()
	{
		VerifyAPIReadWrite();
	}

	void IVisual3DContainer.VerifyAPIReadWrite(DependencyObject other)
	{
		VerifyAPIReadWrite(other);
	}

	void IVisual3DContainer.AddChild(Visual3D child)
	{
		if (base.IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		child.SetParent(this);
		if (_inheritanceContextForChildren != null)
		{
			_inheritanceContextForChildren.ProvideSelfAsInheritanceContext(child, null);
		}
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		Visual.PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		Visual3D.PropagateFlags(child, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		OnVisualChildrenChanged(child, null);
		child.FireOnVisualParentChanged(null);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: true);
	}

	void IVisual3DContainer.RemoveChild(Visual3D child)
	{
		_ = child.ParentIndex;
		if (base.IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: false);
		child.SetParent((Visual)null);
		if (_inheritanceContextForChildren != null)
		{
			_inheritanceContextForChildren.RemoveSelfAsInheritanceContext(child, null);
		}
		int i = 0;
		for (int count = _proxy3D.Count; i < count; i++)
		{
			DUCE.Channel channel = _proxy3D.GetChannel(i);
			if (child.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent))
			{
				child.SetFlags(channel, value: false, VisualProxyFlags.IsConnectedToParent);
				((DUCE.IResource)child).RemoveChildFromParent((DUCE.IResource)this, channel);
				((DUCE.IResource)child).ReleaseOnChannel(channel);
			}
		}
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		Visual.PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		child.FireOnVisualParentChanged(this);
		OnVisualChildrenChanged(null, child);
	}

	int IVisual3DContainer.GetChildrenCount()
	{
		return InternalVisual2DOr3DChildrenCount;
	}

	Visual3D IVisual3DContainer.GetChild(int index)
	{
		return (Visual3D)InternalGet2DOr3DVisualChild(index);
	}

	internal override DependencyObject InternalGet2DOr3DVisualChild(int index)
	{
		return Children[index];
	}

	internal override HitTestResultBehavior HitTestPointInternal(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
	{
		if (_children.Count != 0)
		{
			double distanceAdjustment;
			RayHitTestParameters rayHitTestParameters = Camera.RayFromViewportPoint(hitTestParameters.HitPoint, Viewport.Size, BBoxSubgraph, out distanceAdjustment);
			HitTestResultBehavior lastResult = Visual3D.HitTestChildren(filterCallback, rayHitTestParameters, this);
			return rayHitTestParameters.RaiseCallback(resultCallback, filterCallback, lastResult, distanceAdjustment);
		}
		return HitTestResultBehavior.Continue;
	}

	protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
	{
		throw new NotSupportedException(SR.Format(SR.HitTest_Invalid, typeof(GeometryHitTestParameters).Name, GetType().Name));
	}

	internal Point WorldToViewport(Point4D point)
	{
		double aspectRatio = M3DUtil.GetAspectRatio(Viewport.Size);
		Camera camera = Camera;
		if (camera != null)
		{
			Matrix3D matrix3D = camera.GetViewMatrix() * camera.GetProjectionMatrix(aspectRatio);
			point *= matrix3D;
			return new Point(point.X / point.W, point.Y / point.W) * M3DUtil.GetHomogeneousToViewportTransform(Viewport);
		}
		return new Point(0.0, 0.0);
	}

	internal override Rect GetHitTestBounds()
	{
		return CalculateSubgraphBoundsInnerSpace();
	}

	internal override Rect CalculateSubgraphBoundsInnerSpace(bool renderBounds)
	{
		Camera camera = Camera;
		if (camera == null)
		{
			return Rect.Empty;
		}
		_bboxChildrenSubgraph3D = ComputeSubgraphBounds3D();
		if (_bboxChildrenSubgraph3D.IsEmpty)
		{
			return Rect.Empty;
		}
		Rect viewport = Viewport;
		if (viewport.IsEmpty)
		{
			return Rect.Empty;
		}
		double aspectRatio = M3DUtil.GetAspectRatio(viewport.Size);
		Matrix3D viewProjMatrix = camera.GetViewMatrix() * camera.GetProjectionMatrix(aspectRatio);
		Rect rect = MILUtilities.ProjectBounds(ref viewProjMatrix, ref _bboxChildrenSubgraph3D);
		Matrix matrix = M3DUtil.GetHomogeneousToViewportTransform(viewport);
		MatrixUtil.TransformRect(ref rect, ref matrix);
		return rect;
	}

	internal Rect3D ComputeSubgraphBounds3D()
	{
		Rect3D empty = Rect3D.Empty;
		int i = 0;
		for (int internalCount = _children.InternalCount; i < internalCount; i++)
		{
			Visual3D visual3D = _children.InternalGetItem(i);
			empty.Union(visual3D.CalculateSubgraphBoundsOuterSpace());
		}
		return empty;
	}

	[Conditional("DEBUG")]
	private void Debug_VerifyCachedSubgraphBounds()
	{
		Rect3D empty = Rect3D.Empty;
		empty = ComputeSubgraphBounds3D();
		Rect3D bboxChildrenSubgraph3D = _bboxChildrenSubgraph3D;
		if (!(bboxChildrenSubgraph3D.X < empty.X) && !(bboxChildrenSubgraph3D.X > empty.X) && !(bboxChildrenSubgraph3D.Y < empty.Y) && !(bboxChildrenSubgraph3D.Y > empty.Y) && !(bboxChildrenSubgraph3D.Z < empty.Z) && !(bboxChildrenSubgraph3D.Z > empty.Z) && !(bboxChildrenSubgraph3D.SizeX < empty.SizeX) && !(bboxChildrenSubgraph3D.SizeX > empty.SizeX) && !(bboxChildrenSubgraph3D.SizeY < empty.SizeY) && !(bboxChildrenSubgraph3D.SizeY > empty.SizeY))
		{
			if (!(bboxChildrenSubgraph3D.SizeZ < empty.SizeZ))
			{
				_ = !(bboxChildrenSubgraph3D.SizeZ > empty.SizeZ);
			}
			else
				_ = 0;
		}
		else
			_ = 0;
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		DUCE.ResourceHandle resourceHandle = base.AddRefOnChannelCore(channel);
		if (_proxy3D.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VISUAL3D))
		{
			DUCE.Viewport3DVisualNode.Set3DChild(resourceHandle, _proxy3D.GetHandle(channel), channel);
		}
		return resourceHandle;
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		base.ReleaseOnChannelCore(channel);
		_proxy3D.ReleaseOnChannel(channel);
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _proxy.Count;
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _proxy.GetChannel(index);
	}

	internal override void PrecomputeContent()
	{
		base.PrecomputeContent();
		if (_children != null)
		{
			int i = 0;
			for (int internalCount = _children.InternalCount; i < internalCount; i++)
			{
				_children.InternalGetItem(i)?.PrecomputeRecursive(out var _);
			}
		}
	}

	internal override void RenderContent(RenderContext ctx, bool isOnChannel)
	{
		DUCE.Channel channel = ctx.Channel;
		VisualProxyFlags flags = _proxy.GetFlags(channel);
		if ((flags & VisualProxyFlags.Viewport3DVisual_IsCameraDirty) != 0)
		{
			Camera camera = Camera;
			if (camera != null)
			{
				DUCE.Viewport3DVisualNode.SetCamera(((DUCE.IResource)this).GetHandle(channel), ((DUCE.IResource)camera).AddRefOnChannel(channel), channel);
			}
			else if (isOnChannel)
			{
				DUCE.Viewport3DVisualNode.SetCamera(((DUCE.IResource)this).GetHandle(channel), DUCE.ResourceHandle.Null, channel);
			}
			SetFlags(channel, value: false, VisualProxyFlags.Viewport3DVisual_IsCameraDirty);
		}
		if ((flags & VisualProxyFlags.Viewport3DVisual_IsViewportDirty) != 0)
		{
			DUCE.Viewport3DVisualNode.SetViewport(((DUCE.IResource)this).GetHandle(channel), Viewport, channel);
			SetFlags(channel, value: false, VisualProxyFlags.Viewport3DVisual_IsViewportDirty);
		}
		if (_children == null)
		{
			return;
		}
		for (uint num = 0u; num < _children.InternalCount; num++)
		{
			Visual3D visual3D = _children.InternalGetItem((int)num);
			if (visual3D != null)
			{
				if (visual3D.CheckFlagsAnd(channel, VisualProxyFlags.IsSubtreeDirtyForRender) || !visual3D.IsOnChannel(channel))
				{
					visual3D.RenderRecursive(ctx);
				}
				if (visual3D.IsOnChannel(channel) && !visual3D.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent))
				{
					DUCE.Visual3DNode.InsertChildAt(_proxy3D.GetHandle(channel), ((DUCE.IResource)visual3D).GetHandle(channel), num, channel);
					visual3D.SetFlags(channel, value: true, VisualProxyFlags.IsConnectedToParent);
				}
			}
		}
	}

	internal override void FreeContent(DUCE.Channel channel)
	{
		Camera camera = Camera;
		if (camera != null && !CheckFlagsAnd(channel, VisualProxyFlags.Viewport3DVisual_IsCameraDirty))
		{
			((DUCE.IResource)camera).ReleaseOnChannel(channel);
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.Viewport3DVisual_IsCameraDirty);
		}
		if (_children != null)
		{
			for (int i = 0; i < _children.InternalCount; i++)
			{
				((DUCE.IResource)_children.InternalGetItem(i)).ReleaseOnChannel(channel);
			}
		}
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		base.FreeContent(channel);
	}

	internal void Visual3DTreeChanged()
	{
		SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		ContentsChanged(this, EventArgs.Empty);
	}

	DUCE.ResourceHandle DUCE.IResource.Get3DHandle(DUCE.Channel channel)
	{
		return _proxy3D.GetHandle(channel);
	}

	[FriendAccessAllowed]
	internal void SetInheritanceContextForChildren(DependencyObject inheritanceContextForChildren)
	{
		_inheritanceContextForChildren = inheritanceContextForChildren;
	}
}
