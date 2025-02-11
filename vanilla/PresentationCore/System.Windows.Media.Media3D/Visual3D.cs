using System.Diagnostics;
using System.Globalization;
using System.Windows.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.Media;
using MS.Internal.Media3D;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides services and properties that are common to visual 3-D objects, including hit-testing, coordinate transformation, and bounding box calculations.</summary>
public abstract class Visual3D : DependencyObject, DUCE.IResource, IVisual3DContainer, IAnimatable
{
	private const VisualProxyFlags c_Model3DVisualProxyFlagsDirtyMask = VisualProxyFlags.IsSubtreeDirtyForRender | VisualProxyFlags.IsTransformDirty | VisualProxyFlags.IsContentDirty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Visual3D.Transform" /> dependency property.</summary>
	public static readonly DependencyProperty TransformProperty = DependencyProperty.Register("Transform", typeof(Transform3D), typeof(Visual3D), new PropertyMetadata(Transform3D.Identity, TransformPropertyChanged), (object _003Cp0_003E) => MediaContext.CurrentMediaContext.WriteAccessEnabled);

	internal VisualProxy _proxy;

	private static readonly UncommonField<Visual> _2DParent = new UncommonField<Visual>(null);

	private static readonly DependencyObject UseParentAsContext = new DependencyObject();

	private static readonly UncommonField<DependencyObject> _inheritanceContext = new UncommonField<DependencyObject>(UseParentAsContext);

	private static readonly UncommonField<Visual.AncestorChangedEventHandler> AncestorChangedEventField = new UncommonField<Visual.AncestorChangedEventHandler>();

	private Visual3D _3DParent;

	private int _parentIndex = -1;

	private VisualFlags _flags;

	private Rect3D _bboxContent;

	private Rect3D _bboxSubgraph = Rect3D.Empty;

	private bool _internalIsVisible;

	private static readonly ScaleTransform3D _zeroScale = new ScaleTransform3D(0.0, 0.0, 0.0);

	private Model3D _visual3DModel;

	/// <summary>Gets or sets the transformation that is applied to the 3-D object.</summary>
	/// <returns>The transformation to apply to the 3-D object. The default is the <see cref="P:System.Windows.Media.Media3D.Transform3D.Identity" /> transformation.</returns>
	public Transform3D Transform
	{
		get
		{
			return (Transform3D)GetValue(TransformProperty);
		}
		set
		{
			SetValue(TransformProperty, value);
		}
	}

	internal bool InternalIsVisible
	{
		get
		{
			return _internalIsVisible;
		}
		set
		{
			if (_internalIsVisible == value)
			{
				return;
			}
			if (value)
			{
				DisconnectAttachedResource(VisualProxyFlags.IsTransformDirty, _zeroScale);
			}
			else
			{
				Transform3D transform = Transform;
				if (transform != null)
				{
					DisconnectAttachedResource(VisualProxyFlags.IsTransformDirty, transform);
				}
			}
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsTransformDirty);
			RenderChanged(this, EventArgs.Empty);
			_internalIsVisible = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Media3D.Model3D" /> object to render.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Media3D.Model3D" /> object to render.</returns>
	protected Model3D Visual3DModel
	{
		get
		{
			VerifyAPIReadOnly();
			return _visual3DModel;
		}
		set
		{
			VerifyAPIReadWrite();
			if (value != _visual3DModel)
			{
				if (_visual3DModel != null && !_visual3DModel.IsFrozenInternal)
				{
					_visual3DModel.ChangedInternal -= Visual3DModelPropertyChanged;
				}
				Visual3DModelPropertyChanged(_visual3DModel, isSubpropertyChange: false);
				_visual3DModel = value;
				if (_visual3DModel != null && !_visual3DModel.IsFrozenInternal)
				{
					_visual3DModel.ChangedInternal += Visual3DModelPropertyChanged;
				}
			}
		}
	}

	internal Rect3D VisualContentBounds
	{
		get
		{
			VerifyAPIReadWrite();
			return GetContentBounds();
		}
	}

	[FriendAccessAllowed]
	internal Rect Visual2DContentBounds
	{
		get
		{
			VerifyAPIReadWrite();
			Rect result = Rect.Empty;
			Viewport3DVisual viewport3DVisual = (Viewport3DVisual)VisualTreeHelper.GetContainingVisual2D(this);
			if (viewport3DVisual != null)
			{
				result = TransformToAncestor(viewport3DVisual).TransformBounds(VisualContentBounds);
			}
			return result;
		}
	}

	internal Rect3D BBoxSubgraph
	{
		get
		{
			if (CheckFlagsAnd(VisualFlags.IsSubtreeDirtyForPrecompute))
			{
				PrecomputeRecursive(out var _);
			}
			return _bboxSubgraph;
		}
	}

	internal Rect3D VisualDescendantBounds
	{
		get
		{
			VerifyAPIReadWrite();
			return CalculateSubgraphBoundsInnerSpace();
		}
	}

	/// <summary>Gets the number of child elements for the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object.</summary>
	/// <returns>The number of child elements.</returns>
	protected virtual int Visual3DChildrenCount => 0;

	internal override DependencyObject InheritanceContext
	{
		get
		{
			DependencyObject value = _inheritanceContext.GetValue(this);
			if (value == UseParentAsContext)
			{
				return InternalVisualParent;
			}
			return value;
		}
	}

	internal override bool HasMultipleInheritanceContexts => base.HasMultipleInheritanceContexts;

	internal DependencyObject InternalVisualParent
	{
		get
		{
			if (_3DParent != null)
			{
				return _3DParent;
			}
			return _2DParent.GetValue(this);
		}
	}

	internal int ParentIndex
	{
		get
		{
			return _parentIndex;
		}
		set
		{
			_parentIndex = value;
		}
	}

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

	internal virtual int InternalVisual2DOr3DChildrenCount => Visual3DChildrenCount;

	/// <summary>Gets a value that indicates whether this <see cref="T:System.Windows.Media.Media3D.Visual3D" /> has any animated properties.</summary>
	/// <returns>true if this element has animations; otherwise, false.</returns>
	public bool HasAnimatedProperties
	{
		get
		{
			VerifyAccess();
			return base.IAnimatable_HasAnimatedProperties;
		}
	}

	internal event Visual.AncestorChangedEventHandler VisualAncestorChanged
	{
		add
		{
			Visual.AncestorChangedEventHandler value2 = AncestorChangedEventField.GetValue(this);
			value2 = ((value2 != null) ? ((Visual.AncestorChangedEventHandler)Delegate.Combine(value2, value)) : value);
			AncestorChangedEventField.SetValue(this, value2);
			Visual.SetTreeBits(this, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
		}
		remove
		{
			if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
			{
				Visual.ClearTreeBits(this, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
			}
			Visual.AncestorChangedEventHandler value2 = AncestorChangedEventField.GetValue(this);
			if (value2 != null)
			{
				value2 = (Visual.AncestorChangedEventHandler)Delegate.Remove(value2, value);
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

	internal Visual3D()
	{
		_internalIsVisible = true;
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
		return _proxy.GetHandle(channel);
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		_proxy.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VISUAL3D);
		return _proxy.GetHandle(channel);
	}

	void DUCE.IResource.RemoveChildFromParent(DUCE.IResource parent, DUCE.Channel channel)
	{
		DUCE.Visual3DNode.RemoveChild(parent.Get3DHandle(channel), _proxy.GetHandle(channel), channel);
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _proxy.Count;
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _proxy.GetChannel(index);
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Visual3D visual3D = (Visual3D)d;
		if (!e.IsASubPropertyChange)
		{
			if (e.OldValue != null)
			{
				visual3D.DisconnectAttachedResource(VisualProxyFlags.IsTransformDirty, (DUCE.IResource)e.OldValue);
			}
			visual3D.SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsTransformDirty);
		}
		visual3D.RenderChanged(visual3D, EventArgs.Empty);
	}

	/// <summary>Defines the parent-child relationship between two 3-D visuals.</summary>
	/// <param name="child">The child 3-D visual object to add to the parent 3-D visual object.</param>
	/// <exception cref="T:System.InvalidOperationException">The children collection cannot be modified when a visual children iteration is in progress.</exception>
	protected void AddVisual3DChild(Visual3D child)
	{
		if (IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		child.SetParent(this);
		ProvideSelfAsInheritanceContext(child, null);
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		PropagateFlags(child, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		OnVisualChildrenChanged(child, null);
		child.FireOnVisualParentChanged(null);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: true);
	}

	/// <summary>Removes the parent-child relationship between two 3-D visuals.</summary>
	/// <param name="child">The child 3-D visual object to remove from the parent visual.</param>
	protected void RemoveVisual3DChild(Visual3D child)
	{
		if (IsVisualChildrenIterationInProgress)
		{
			throw new InvalidOperationException(SR.CannotModifyVisualChildrenDuringTreeWalk);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		VisualDiagnostics.OnVisualChildChanged(this, child, isAdded: false);
		child.SetParent((Visual3D)null);
		RemoveSelfAsInheritanceContext(child, null);
		int i = 0;
		for (int count = _proxy.Count; i < count; i++)
		{
			DUCE.Channel channel = _proxy.GetChannel(i);
			if (child.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent))
			{
				child.SetFlags(channel, value: false, VisualProxyFlags.IsConnectedToParent);
				((DUCE.IResource)child).RemoveChildFromParent((DUCE.IResource)this, channel);
				((DUCE.IResource)child).ReleaseOnChannel(channel);
			}
		}
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
		child.FireOnVisualParentChanged(this);
		OnVisualChildrenChanged(null, child);
	}

	private void Visual3DModelPropertyChanged(Model3D oldValue, bool isSubpropertyChange)
	{
		if (!isSubpropertyChange)
		{
			if (oldValue != null)
			{
				DisconnectAttachedResource(VisualProxyFlags.IsContentDirty, oldValue);
			}
			SetFlagsOnAllChannels(value: true, VisualProxyFlags.IsContentDirty);
		}
		SetFlags(value: false, VisualFlags.Are3DContentBoundsValid);
		RenderChanged(this, EventArgs.Empty);
	}

	private void Visual3DModelPropertyChanged(object o, EventArgs e)
	{
		Visual3DModelPropertyChanged(null, isSubpropertyChange: true);
	}

	internal virtual void FireOnVisualParentChanged(DependencyObject oldParent)
	{
		OnVisualParentChanged(oldParent);
		if (oldParent == null)
		{
			if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
			{
				Visual.SetTreeBits(VisualTreeHelper.GetParent(this), VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
			}
		}
		else if (CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
		{
			Visual.ClearTreeBits(oldParent, VisualFlags.SubTreeHoldsAncestorChanged, VisualFlags.RegisteredForAncestorChanged);
		}
		AncestorChangedEventArgs args = new AncestorChangedEventArgs(this, oldParent);
		ProcessAncestorChangedNotificationRecursive(this, args);
	}

	internal static void ProcessAncestorChangedNotificationRecursive(DependencyObject e, AncestorChangedEventArgs args)
	{
		if (e is Visual)
		{
			Visual.ProcessAncestorChangedNotificationRecursive(e, args);
			return;
		}
		Visual3D visual3D = e as Visual3D;
		if (!visual3D.CheckFlagsAnd(VisualFlags.SubTreeHoldsAncestorChanged))
		{
			return;
		}
		AncestorChangedEventField.GetValue(visual3D)?.Invoke(visual3D, args);
		int internalVisual2DOr3DChildrenCount = visual3D.InternalVisual2DOr3DChildrenCount;
		for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
		{
			DependencyObject dependencyObject = visual3D.InternalGet2DOr3DVisualChild(i);
			if (dependencyObject != null)
			{
				ProcessAncestorChangedNotificationRecursive(dependencyObject, args);
			}
		}
	}

	/// <summary>Called when the parent of the 3-D visual object is changed.</summary>
	/// <param name="oldParent">A value of type <see cref="T:System.Windows.DependencyObject" /> that represents the previous parent of the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object. If the <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object did not have a previous parent, the value of the parameter is null.</param>
	protected internal virtual void OnVisualParentChanged(DependencyObject oldParent)
	{
	}

	/// <summary>Called when the <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" /> of the visual object is modified.</summary>
	/// <param name="visualAdded">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> that was added to the collection.</param>
	/// <param name="visualRemoved">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> that was removed from the collection.</param>
	protected internal virtual void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
	}

	internal bool DoesRayHitSubgraphBounds(RayHitTestParameters rayParams)
	{
		rayParams.GetLocalLine(out var origin, out var direction);
		Rect3D box = VisualDescendantBounds;
		return LineUtil.ComputeLineBoxIntersection(ref origin, ref direction, ref box, rayParams.IsRay);
	}

	internal void HitTest(HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, HitTestParameters3D hitTestParameters)
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
		if (hitTestParameters is RayHitTestParameters rayHitTestParameters)
		{
			rayHitTestParameters.ClearResults();
			HitTestResultBehavior lastResult = RayHitTest(filterCallback, rayHitTestParameters);
			rayHitTestParameters.RaiseCallback(resultCallback, filterCallback, lastResult);
		}
		else
		{
			Invariant.Assert(condition: false, string.Format(CultureInfo.InvariantCulture, "'{0}' HitTestParameters3D are not supported on {1}.", hitTestParameters.GetType().Name, GetType().Name));
		}
	}

	internal HitTestResultBehavior RayHitTest(HitTestFilterCallback filterCallback, RayHitTestParameters rayParams)
	{
		if (DoesRayHitSubgraphBounds(rayParams))
		{
			HitTestFilterBehavior behavior = HitTestFilterBehavior.Continue;
			if (filterCallback != null)
			{
				behavior = filterCallback(this);
				if (HTFBInterpreter.SkipSubgraph(behavior))
				{
					return HitTestResultBehavior.Continue;
				}
				if (HTFBInterpreter.Stop(behavior))
				{
					return HitTestResultBehavior.Stop;
				}
			}
			if (HTFBInterpreter.IncludeChildren(behavior) && HitTestChildren(filterCallback, rayParams) == HitTestResultBehavior.Stop)
			{
				return HitTestResultBehavior.Stop;
			}
			if (HTFBInterpreter.DoHitTest(behavior))
			{
				RayHitTestInternal(filterCallback, rayParams);
			}
		}
		return HitTestResultBehavior.Continue;
	}

	internal HitTestResultBehavior HitTestChildren(HitTestFilterCallback filterCallback, RayHitTestParameters rayParams)
	{
		return HitTestChildren(filterCallback, rayParams, this);
	}

	internal static HitTestResultBehavior HitTestChildren(HitTestFilterCallback filterCallback, RayHitTestParameters rayParams, IVisual3DContainer container)
	{
		if (container != null)
		{
			for (int num = container.GetChildrenCount() - 1; num >= 0; num--)
			{
				Visual3D child = container.GetChild(num);
				Transform3D transform = child.Transform;
				rayParams.PushVisualTransform(transform);
				HitTestResultBehavior num2 = child.RayHitTest(filterCallback, rayParams);
				rayParams.PopTransform(transform);
				if (num2 == HitTestResultBehavior.Stop)
				{
					return HitTestResultBehavior.Stop;
				}
			}
		}
		return HitTestResultBehavior.Continue;
	}

	internal void RayHitTestInternal(HitTestFilterCallback filterCallback, RayHitTestParameters rayParams)
	{
		Model3D visual3DModel = _visual3DModel;
		if (visual3DModel != null)
		{
			rayParams.CurrentVisual = this;
			visual3DModel.RayHitTest(rayParams);
		}
	}

	internal void RenderChanged(object sender, EventArgs e)
	{
		PropagateFlags(this, VisualFlags.IsSubtreeDirtyForPrecompute, VisualProxyFlags.IsSubtreeDirtyForRender);
	}

	internal Rect3D GetContentBounds()
	{
		Model3D visual3DModel = _visual3DModel;
		if (visual3DModel == null)
		{
			return Rect3D.Empty;
		}
		if (!CheckFlagsAnd(VisualFlags.Are3DContentBoundsValid))
		{
			_bboxContent = visual3DModel.CalculateSubgraphBoundsOuterSpace();
			SetFlags(value: true, VisualFlags.Are3DContentBoundsValid);
		}
		return _bboxContent;
	}

	internal Rect3D CalculateSubgraphBoundsOuterSpace()
	{
		Rect3D originalBox = CalculateSubgraphBoundsInnerSpace();
		return M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref originalBox, Transform);
	}

	internal Rect3D CalculateSubgraphBoundsInnerSpace()
	{
		return BBoxSubgraph;
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

	internal void VerifyAPIReadOnly()
	{
		VerifyAccess();
	}

	internal void VerifyAPIReadOnly(DependencyObject other)
	{
		VerifyAPIReadOnly();
		if (other != null)
		{
			MediaSystem.AssertSameContext(this, other);
		}
	}

	internal void VerifyAPIReadWrite()
	{
		VerifyAPIReadOnly();
		MediaContext.From(base.Dispatcher).VerifyWriteAccess();
	}

	internal void VerifyAPIReadWrite(DependencyObject other)
	{
		VerifyAPIReadWrite();
		if (other != null)
		{
			MediaSystem.AssertSameContext(this, other);
		}
	}

	internal void SetParent(Visual newParent)
	{
		_2DParent.SetValue(this, newParent);
		_3DParent = null;
	}

	internal void SetParent(Visual3D newParent)
	{
		_2DParent.ClearValue(this);
		_3DParent = newParent;
	}

	/// <summary>Returns the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> in the parent <see cref="T:System.Windows.Media.Media3D.Visual3DCollection" />.</summary>
	/// <returns>The child in the collection at the specified <paramref name="index" /> value.</returns>
	/// <param name="index">The index of the 3-D visual object in the collection.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> value is not valid.</exception>
	protected virtual Visual3D GetVisual3DChild(int index)
	{
		throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
	}

	void IVisual3DContainer.AddChild(Visual3D child)
	{
		AddVisual3DChild(child);
	}

	void IVisual3DContainer.RemoveChild(Visual3D child)
	{
		RemoveVisual3DChild(child);
	}

	int IVisual3DContainer.GetChildrenCount()
	{
		return Visual3DChildrenCount;
	}

	Visual3D IVisual3DContainer.GetChild(int index)
	{
		return GetVisual3DChild(index);
	}

	internal virtual void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
	{
		UIElement3D.InvalidateForceInheritPropertyOnChildren(this, property);
	}

	[Conditional("DEBUG")]
	internal void Debug_VerifyBoundsEqual(Rect3D bounds1, Rect3D bounds2, string errorString)
	{
		if (!(bounds1.X < bounds2.X) && !(bounds1.X > bounds2.X) && !(bounds1.Y < bounds2.Y) && !(bounds1.Y > bounds2.Y) && !(bounds1.Z < bounds2.Z) && !(bounds1.Z > bounds2.Z) && !(bounds1.SizeX < bounds2.SizeX) && !(bounds1.SizeX > bounds2.SizeX) && !(bounds1.SizeY < bounds2.SizeY) && !(bounds1.SizeY > bounds2.SizeY))
		{
			if (!(bounds1.SizeZ < bounds2.SizeZ))
			{
				_ = !(bounds1.SizeZ > bounds2.SizeZ);
			}
			else
				_ = 0;
		}
		else
			_ = 0;
	}

	[Conditional("DEBUG")]
	internal void Debug_VerifyCachedSubgraphBounds()
	{
		_ = Rect3D.Empty;
		M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref _bboxSubgraph, Transform);
	}

	[Conditional("DEBUG")]
	internal void Debug_VerifyCachedContentBounds()
	{
		_ = _visual3DModel;
	}

	internal void PrecomputeRecursive(out Rect3D bboxSubgraph)
	{
		if (CheckFlagsAnd(VisualFlags.IsSubtreeDirtyForPrecompute))
		{
			_bboxSubgraph = GetContentBounds();
			int i = 0;
			for (int visual3DChildrenCount = Visual3DChildrenCount; i < visual3DChildrenCount; i++)
			{
				GetVisual3DChild(i).PrecomputeRecursive(out var bboxSubgraph2);
				_bboxSubgraph.Union(bboxSubgraph2);
			}
			SetFlags(value: false, VisualFlags.IsSubtreeDirtyForPrecompute);
		}
		bboxSubgraph = M3DUtil.ComputeTransformedAxisAlignedBoundingBox(ref _bboxSubgraph, Transform);
	}

	internal void RenderRecursive(RenderContext ctx)
	{
		DUCE.Channel channel = ctx.Channel;
		DUCE.ResourceHandle @null = DUCE.ResourceHandle.Null;
		VisualProxyFlags visualProxyFlags = VisualProxyFlags.IsSubtreeDirtyForRender | VisualProxyFlags.IsTransformDirty | VisualProxyFlags.IsContentDirty;
		bool flag = IsOnChannel(channel);
		if (flag)
		{
			@null = _proxy.GetHandle(channel);
			visualProxyFlags = _proxy.GetFlags(channel);
		}
		else
		{
			@null = ((DUCE.IResource)this).AddRefOnChannel(channel);
		}
		if ((visualProxyFlags & VisualProxyFlags.IsContentDirty) != 0)
		{
			RenderContent(ctx, flag);
		}
		if ((visualProxyFlags & VisualProxyFlags.IsTransformDirty) != 0)
		{
			Transform3D transform = Transform;
			if (transform != null && InternalIsVisible)
			{
				DUCE.Visual3DNode.SetTransform(@null, ((DUCE.IResource)transform).AddRefOnChannel(channel), channel);
			}
			else if (!InternalIsVisible)
			{
				DUCE.Visual3DNode.SetTransform(@null, ((DUCE.IResource)_zeroScale).AddRefOnChannel(channel), channel);
			}
			else if (!flag)
			{
				DUCE.Visual3DNode.SetTransform(@null, DUCE.ResourceHandle.Null, channel);
			}
		}
		for (int i = 0; i < Visual3DChildrenCount; i++)
		{
			Visual3D visual3DChild = GetVisual3DChild(i);
			if (visual3DChild != null)
			{
				if (visual3DChild.CheckFlagsAnd(channel, VisualProxyFlags.IsSubtreeDirtyForRender) || !visual3DChild.IsOnChannel(channel))
				{
					visual3DChild.RenderRecursive(ctx);
				}
				if (visual3DChild.IsOnChannel(ctx.Channel) && !visual3DChild.CheckFlagsAnd(channel, VisualProxyFlags.IsConnectedToParent))
				{
					DUCE.Visual3DNode.InsertChildAt(@null, ((DUCE.IResource)visual3DChild).GetHandle(channel), (uint)i, ctx.Channel);
					visual3DChild.SetFlags(channel, value: true, VisualProxyFlags.IsConnectedToParent);
				}
			}
		}
		SetFlags(channel, value: false, VisualProxyFlags.IsSubtreeDirtyForRender | VisualProxyFlags.IsTransformDirty | VisualProxyFlags.IsContentDirty);
	}

	internal void RenderContent(RenderContext ctx, bool isOnChannel)
	{
		DUCE.Channel channel = ctx.Channel;
		if (_visual3DModel != null)
		{
			DUCE.Visual3DNode.SetContent(((DUCE.IResource)this).GetHandle(channel), ((DUCE.IResource)_visual3DModel).AddRefOnChannel(channel), channel);
			SetFlags(channel, value: true, VisualProxyFlags.IsContentConnected);
		}
		else if (isOnChannel)
		{
			DUCE.Visual3DNode.SetContent(((DUCE.IResource)this).GetHandle(channel), DUCE.ResourceHandle.Null, channel);
		}
	}

	/// <summary>Determines whether the visual object is an ancestor of the descendant visual object.</summary>
	/// <returns>true if the visual object is an ancestor of <paramref name="descendant" />; otherwise, false.</returns>
	/// <param name="descendant">A visual object that is a possible descendant.</param>
	public bool IsAncestorOf(DependencyObject descendant)
	{
		VisualTreeUtils.AsNonNullVisual(descendant, out var visual, out var visual3D);
		return visual?.IsDescendantOf(this) ?? visual3D.IsDescendantOf(this);
	}

	/// <summary>Determines whether the visual object is a descendant of the ancestor visual object. </summary>
	/// <returns>true if the visual object is a descendant of <paramref name="ancestor" />; otherwise, false.</returns>
	/// <param name="ancestor">A visual object that is a possible ancestor.</param>
	public bool IsDescendantOf(DependencyObject ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VisualTreeUtils.EnsureVisual(ancestor);
		Visual3D visual3D = this;
		while (visual3D != null && visual3D != ancestor)
		{
			if (visual3D._3DParent == null)
			{
				DependencyObject internalVisualParent = visual3D.InternalVisualParent;
				if (internalVisualParent != null)
				{
					return ((Visual)internalVisualParent).IsDescendantOf(ancestor);
				}
			}
			visual3D = visual3D._3DParent;
		}
		return visual3D == ancestor;
	}

	internal void SetFlagsToRoot(bool value, VisualFlags flag)
	{
		Visual3D visual3D = this;
		do
		{
			visual3D.SetFlags(value, flag);
			if (visual3D._3DParent == null)
			{
				VisualTreeUtils.SetFlagsToRoot(InternalVisualParent, value, flag);
				break;
			}
			visual3D = visual3D._3DParent;
		}
		while (visual3D != null);
	}

	internal DependencyObject FindFirstAncestorWithFlagsAnd(VisualFlags flag)
	{
		Visual3D visual3D = this;
		do
		{
			if (visual3D.CheckFlagsAnd(flag))
			{
				return visual3D;
			}
			if (visual3D._3DParent == null)
			{
				return VisualTreeUtils.FindFirstAncestorWithFlagsAnd(InternalVisualParent, flag);
			}
			visual3D = visual3D._3DParent;
		}
		while (visual3D != null);
		return null;
	}

	/// <summary>Returns the common ancestor of the visual object and another specified visual object.</summary>
	/// <returns>The common ancestor of the current visual object and <paramref name="otherVisual" />; or null if no common ancestor is found.</returns>
	/// <param name="otherVisual">The visual object with which to find a common ancestor.</param>
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

	internal void FreeDUCEResources(DUCE.Channel channel)
	{
		Transform3D transform = Transform;
		if (!CheckFlagsAnd(channel, VisualProxyFlags.IsTransformDirty))
		{
			if (InternalIsVisible)
			{
				((DUCE.IResource)transform)?.ReleaseOnChannel(channel);
			}
			else
			{
				((DUCE.IResource)_zeroScale).ReleaseOnChannel(channel);
			}
		}
		Model3D visual3DModel = _visual3DModel;
		if (visual3DModel != null && !CheckFlagsAnd(channel, VisualProxyFlags.IsContentDirty))
		{
			((DUCE.IResource)visual3DModel).ReleaseOnChannel(channel);
		}
		_proxy.ReleaseOnChannel(channel);
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		ReleaseOnChannelCore(channel);
	}

	internal void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (IsOnChannel(channel))
		{
			SetFlags(channel, value: false, VisualProxyFlags.IsConnectedToParent);
			FreeDUCEResources(channel);
			for (int i = 0; i < Visual3DChildrenCount; i++)
			{
				((DUCE.IResource)GetVisual3DChild(i)).ReleaseOnChannel(channel);
			}
		}
	}

	internal void DisconnectAttachedResource(VisualProxyFlags correspondingFlag, DUCE.IResource attachedResource)
	{
		for (int i = 0; i < _proxy.Count; i++)
		{
			if ((_proxy.GetFlags(i) & correspondingFlag) == 0)
			{
				DUCE.Channel channel = _proxy.GetChannel(i);
				SetFlags(channel, value: true, correspondingFlag);
				if (correspondingFlag == VisualProxyFlags.IsContentDirty)
				{
					_proxy.SetFlags(i, value: false, VisualProxyFlags.IsContentConnected);
				}
				attachedResource.ReleaseOnChannel(channel);
			}
		}
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		base.AddInheritanceContext(context, property);
		AddOrRemoveInheritanceContext(context);
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		base.RemoveInheritanceContext(context, property);
		AddOrRemoveInheritanceContext(null);
	}

	private void AddOrRemoveInheritanceContext(DependencyObject newInheritanceContext)
	{
		if (InheritanceContext != newInheritanceContext || (_inheritanceContext.GetValue(this) == UseParentAsContext && newInheritanceContext == InternalVisualParent))
		{
			SetInheritanceContext(newInheritanceContext);
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	internal override void OnInheritanceContextChangedCore(EventArgs args)
	{
		base.OnInheritanceContextChangedCore(args);
		for (int i = 0; i < Visual3DChildrenCount; i++)
		{
			GetVisual3DChild(i).OnInheritanceContextChanged(args);
		}
	}

	/// <summary>Returns a transform that can be used to transform coordinates from this <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object to the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> ancestor of the object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object; or null, if the transform cannot be created.</returns>
	/// <param name="ancestor">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ancestor" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The specified <paramref name="ancestor" /> object is not an ancestor of this object.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects are not related.</exception>
	public GeneralTransform3D TransformToAncestor(Visual3D ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VerifyAPIReadOnly(ancestor);
		return InternalTransformToAncestor(ancestor, inverse: false);
	}

	/// <summary>Returns a transform that can be used to transform coordinates from this <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object to the specified <see cref="T:System.Windows.Media.Media3D.Visual3D" /> descent object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object; or null, if the transform from <paramref name="descendant" /> to this object is non-invertible.</returns>
	/// <param name="descendant">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="descendant" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">This object is not an ancestor of the specified <paramref name="descendant" /> object.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Media.Media3D.Visual3D" /> objects are not related.</exception>
	public GeneralTransform3D TransformToDescendant(Visual3D descendant)
	{
		if (descendant == null)
		{
			throw new ArgumentNullException("descendant");
		}
		VerifyAPIReadOnly(descendant);
		return descendant.InternalTransformToAncestor(this, inverse: true);
	}

	private GeneralTransform3D InternalTransformToAncestor(Visual3D ancestor, bool inverse)
	{
		bool flag = true;
		DependencyObject dependencyObject = this;
		Visual3D visual3D = null;
		Matrix3D matrix = Matrix3D.Identity;
		GeneralTransform3DGroup generalTransform3DGroup = null;
		while (VisualTreeHelper.GetParent(dependencyObject) != null && dependencyObject != ancestor)
		{
			if (dependencyObject is Visual3D { Transform: var transform } visual3D2)
			{
				transform?.Append(ref matrix);
				visual3D = visual3D2;
				dependencyObject = VisualTreeHelper.GetParent(visual3D2);
				continue;
			}
			if (generalTransform3DGroup == null)
			{
				generalTransform3DGroup = new GeneralTransform3DGroup();
			}
			generalTransform3DGroup.Children.Add(new MatrixTransform3D(matrix));
			matrix = Matrix3D.Identity;
			Visual visual = dependencyObject as Visual;
			GeneralTransform3DTo2D generalTransform3DTo2D = visual3D.TransformToAncestor(visual);
			Visual3D containingVisual3D = VisualTreeHelper.GetContainingVisual3D(visual);
			if (containingVisual3D == null)
			{
				break;
			}
			GeneralTransform2DTo3D generalTransform2DTo3D = visual.TransformToAncestor(containingVisual3D);
			if (generalTransform3DTo2D == null || generalTransform2DTo3D == null)
			{
				flag = false;
			}
			else
			{
				generalTransform3DGroup.Children.Add(new GeneralTransform3DTo2DTo3D(generalTransform3DTo2D, generalTransform2DTo3D));
			}
			dependencyObject = containingVisual3D;
		}
		if (dependencyObject != ancestor)
		{
			throw new InvalidOperationException(inverse ? SR.Visual_NotADescendant : SR.Visual_NotAnAncestor);
		}
		GeneralTransform3D generalTransform3D = null;
		if (flag)
		{
			generalTransform3D = ((generalTransform3DGroup == null) ? ((GeneralTransform3D)new MatrixTransform3D(matrix)) : ((GeneralTransform3D)generalTransform3DGroup));
			if (inverse)
			{
				generalTransform3D = generalTransform3D.Inverse;
			}
		}
		generalTransform3D?.Freeze();
		return generalTransform3D;
	}

	/// <summary>Returns a transform that can be used to transform coordinates from this <see cref="T:System.Windows.Media.Media3D.Visual3D" /> object to the specified <see cref="T:System.Windows.Media.Visual" /> ancestor of the object.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Media3D.GeneralTransform3DTo2D" /> object; or null, if the transform cannot be created.</returns>
	/// <param name="ancestor">The <see cref="T:System.Windows.Media.Visual" /> to which the coordinates are transformed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="ancestor" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The specified <paramref name="ancestor" /> object is not an ancestor of this object.</exception>
	public GeneralTransform3DTo2D TransformToAncestor(Visual ancestor)
	{
		if (ancestor == null)
		{
			throw new ArgumentNullException("ancestor");
		}
		VerifyAPIReadOnly(ancestor);
		return InternalTransformToAncestor(ancestor);
	}

	internal GeneralTransform3DTo2D InternalTransformToAncestor(Visual ancestor)
	{
		if (!M3DUtil.TryTransformToViewport3DVisual(this, out var viewport, out var matrix))
		{
			return null;
		}
		GeneralTransform transformBetween2D = viewport.TransformToAncestor(ancestor);
		GeneralTransform3DTo2D generalTransform3DTo2D = new GeneralTransform3DTo2D(matrix, transformBetween2D);
		generalTransform3DTo2D.Freeze();
		return generalTransform3DTo2D;
	}

	internal void SetFlagsOnAllChannels(bool value, VisualProxyFlags flagsToChange)
	{
		_proxy.SetFlagsOnAllChannels(value, flagsToChange);
	}

	internal void SetFlags(DUCE.Channel channel, bool value, VisualProxyFlags flagsToChange)
	{
		_proxy.SetFlags(channel, value, flagsToChange);
	}

	internal void SetFlags(bool value, VisualFlags Flags)
	{
		_flags = (value ? (_flags | Flags) : (_flags & ~Flags));
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

	internal virtual DependencyObject InternalGet2DOr3DVisualChild(int index)
	{
		return GetVisual3DChild(index);
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

	internal static bool DoAnyChildrenHaveABitSet(Visual3D pe, VisualFlags flag)
	{
		int internalVisual2DOr3DChildrenCount = pe.InternalVisual2DOr3DChildrenCount;
		for (int i = 0; i < internalVisual2DOr3DChildrenCount; i++)
		{
			DependencyObject element = pe.InternalGet2DOr3DVisualChild(i);
			Visual visual = null;
			Visual3D visual3D = null;
			VisualTreeUtils.AsNonNullVisual(element, out visual, out visual3D);
			if (visual != null && visual.CheckFlagsAnd(flag))
			{
				return true;
			}
			if (visual3D != null && visual3D.CheckFlagsAnd(flag))
			{
				return true;
			}
		}
		return false;
	}

	internal static void PropagateFlags(Visual3D e, VisualFlags flags, VisualProxyFlags proxyFlags)
	{
		while (e != null && (!e.CheckFlagsAnd(flags) || !e.CheckFlagsOnAllChannels(proxyFlags)))
		{
			e.SetFlags(value: true, flags);
			e.SetFlagsOnAllChannels(value: true, proxyFlags);
			if (e._3DParent == null)
			{
				if (e.InternalVisualParent is Viewport3DVisual viewport3DVisual)
				{
					viewport3DVisual.Visual3DTreeChanged();
					Visual.PropagateFlags(viewport3DVisual, flags, proxyFlags);
				}
				break;
			}
			e = e._3DParent;
		}
	}

	private void SetInheritanceContext(DependencyObject newInheritanceContext)
	{
		if (newInheritanceContext == InternalVisualParent)
		{
			_inheritanceContext.ClearValue(this);
		}
		else
		{
			_inheritanceContext.SetValue(this, newInheritanceContext);
		}
	}

	/// <summary>Applies the effect of a given <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to a given dependency property.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> to animate.</param>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that animates the property.</param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock)
	{
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Applies the effect of a given <see cref="T:System.Windows.Media.Animation.AnimationClock" /> to a given dependency property. The effect of the new <see cref="T:System.Windows.Media.Animation.AnimationClock" /> on any current animations is determined by the value of the <paramref name="handoffBehavior" /> parameter.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> to animate.</param>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that animates the property.</param>
	/// <param name="handoffBehavior">The object that specifies how to interact with all relevant animation sequences.</param>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (clock != null && !AnimationStorage.IsAnimationValid(dp, clock.Timeline))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, clock.Timeline.GetType(), dp.Name, dp.PropertyType), "clock");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.ApplyAnimationClock(this, dp, clock, handoffBehavior);
	}

	/// <summary>Initiates an animation sequence for the <see cref="T:System.Windows.DependencyProperty" /> object, based on the specified <see cref="T:System.Windows.Media.Animation.AnimationTimeline" />.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> object to animate.</param>
	/// <param name="animation">The timeline that has the necessary functionality to animate the property.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation)
	{
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);
	}

	/// <summary>Initiates an animation sequence for the <see cref="T:System.Windows.DependencyProperty" /> object, based on both the specified <see cref="T:System.Windows.Media.Animation.AnimationTimeline" /> and <see cref="T:System.Windows.Media.Animation.HandoffBehavior" />.</summary>
	/// <param name="dp">The <see cref="T:System.Windows.DependencyProperty" /> object to animate.</param>
	/// <param name="animation">The timeline that has the necessary functionality to customize the new animation.</param>
	/// <param name="handoffBehavior">The object that specifies how to interact with all relevant animation sequences.</param>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!AnimationStorage.IsPropertyAnimatable(this, dp))
		{
			throw new ArgumentException(SR.Format(SR.Animation_DependencyPropertyIsNotAnimatable, dp.Name, GetType()), "dp");
		}
		if (animation != null && !AnimationStorage.IsAnimationValid(dp, animation))
		{
			throw new ArgumentException(SR.Format(SR.Animation_AnimationTimelineTypeMismatch, animation.GetType(), dp.Name, dp.PropertyType), "animation");
		}
		if (!HandoffBehaviorEnum.IsDefined(handoffBehavior))
		{
			throw new ArgumentException(SR.Animation_UnrecognizedHandoffBehavior);
		}
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.IAnimatable_CantAnimateSealedDO, dp, GetType()));
		}
		AnimationStorage.BeginAnimation(this, dp, animation, handoffBehavior);
	}

	/// <summary>Retrieves the base value of the specified <see cref="T:System.Windows.DependencyProperty" /> object.</summary>
	/// <returns>The object that represents the base value of <paramref name="dp" />.</returns>
	/// <param name="dp">The object for which the base value is being requested.</param>
	public object GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		return GetValueEntry(LookupEntry(dp.GlobalIndex), dp, null, RequestFlags.AnimationBaseValue).Value;
	}

	internal sealed override void EvaluateAnimatedValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry entry)
	{
		if (base.IAnimatable_HasAnimatedProperties)
		{
			AnimationStorage.GetStorage(this, dp)?.EvaluateAnimatedValue(metadata, ref entry);
		}
	}
}
