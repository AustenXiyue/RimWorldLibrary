using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Represents a composite geometry, composed of other <see cref="T:System.Windows.Media.Geometry" /> objects. </summary>
[ContentProperty("Children")]
public sealed class GeometryGroup : Geometry
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeometryGroup.FillRule" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GeometryGroup.FillRule" /> dependency property.</returns>
	public static readonly DependencyProperty FillRuleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeometryGroup.Children" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GeometryGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const FillRule c_FillRule = FillRule.EvenOdd;

	internal static GeometryCollection s_Children;

	/// <summary> Gets or sets how the intersecting areas of the objects contained in this <see cref="T:System.Windows.Media.GeometryGroup" /> are combined.   </summary>
	/// <returns>Specifies how the intersecting areas are combined to form the resulting area.  The default value is EvenOdd.</returns>
	public FillRule FillRule
	{
		get
		{
			return (FillRule)GetValue(FillRuleProperty);
		}
		set
		{
			SetValueInternal(FillRuleProperty, FillRuleBoxes.Box(value));
		}
	}

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.GeometryCollection" /> that contains the objects that define this <see cref="T:System.Windows.Media.GeometryGroup" />.   </summary>
	/// <returns>A collection containing the children of this <see cref="T:System.Windows.Media.GeometryGroup" />.</returns>
	public GeometryCollection Children
	{
		get
		{
			return (GeometryCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryGroup" /> class. </summary>
	public GeometryGroup()
	{
	}

	internal override PathGeometryData GetPathGeometryData()
	{
		return GetAsPathGeometry().GetPathGeometryData();
	}

	internal override PathGeometry GetAsPathGeometry()
	{
		PathGeometry pathGeometry = new PathGeometry();
		pathGeometry.AddGeometry(this);
		pathGeometry.FillRule = FillRule;
		return pathGeometry;
	}

	internal override PathFigureCollection GetTransformedFigureCollection(Transform transform)
	{
		Transform transform2 = new MatrixTransform(GetCombinedMatrix(transform));
		PathFigureCollection pathFigureCollection = new PathFigureCollection();
		GeometryCollection children = Children;
		if (children != null)
		{
			for (int i = 0; i < children.Count; i++)
			{
				PathFigureCollection transformedFigureCollection = children.Internal_GetItem(i).GetTransformedFigureCollection(transform2);
				if (transformedFigureCollection != null)
				{
					int count = transformedFigureCollection.Count;
					for (int j = 0; j < count; j++)
					{
						pathFigureCollection.Add(transformedFigureCollection[j]);
					}
				}
			}
		}
		return pathFigureCollection;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.GeometryGroup" /> object is empty. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.GeometryGroup" /> is empty; otherwise, false.</returns>
	public override bool IsEmpty()
	{
		GeometryCollection children = Children;
		if (children == null)
		{
			return true;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (!children[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	internal override bool IsObviouslyEmpty()
	{
		GeometryCollection children = Children;
		if (children != null)
		{
			return children.Count == 0;
		}
		return true;
	}

	/// <summary> Determines whether this <see cref="T:System.Windows.Media.GeometryGroup" /> object may have curved segments. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.GeometryGroup" /> object may have curved segments; otherwise, false.</returns>
	public override bool MayHaveCurves()
	{
		GeometryCollection children = Children;
		if (children == null)
		{
			return false;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i].MayHaveCurves())
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryGroup" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryGroup Clone()
	{
		return (GeometryGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryGroup CloneCurrentValue()
	{
		return (GeometryGroup)base.CloneCurrentValue();
	}

	private static void FillRulePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GeometryGroup)d).PropertyChanged(FillRuleProperty);
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GeometryGroup geometryGroup = (GeometryGroup)d;
		GeometryCollection geometryCollection = null;
		GeometryCollection geometryCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			geometryCollection = (GeometryCollection)e.OldValue;
			if (geometryCollection != null && !geometryCollection.IsFrozen)
			{
				geometryCollection.ItemRemoved -= geometryGroup.ChildrenItemRemoved;
				geometryCollection.ItemInserted -= geometryGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			geometryCollection2 = (GeometryCollection)e.NewValue;
			if (geometryCollection2 != null && !geometryCollection2.IsFrozen)
			{
				geometryCollection2.ItemInserted += geometryGroup.ChildrenItemInserted;
				geometryCollection2.ItemRemoved += geometryGroup.ChildrenItemRemoved;
			}
		}
		if (geometryCollection != geometryCollection2 && geometryGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = geometryGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (geometryCollection2 != null)
					{
						int count = geometryCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)geometryCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (geometryCollection != null)
					{
						int count2 = geometryCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)geometryCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		geometryGroup.PropertyChanged(ChildrenProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeometryGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			GeometryCollection children = Children;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			int num = children?.Count ?? 0;
			DUCE.MILCMD_GEOMETRYGROUP mILCMD_GEOMETRYGROUP = default(DUCE.MILCMD_GEOMETRYGROUP);
			mILCMD_GEOMETRYGROUP.Type = MILCMD.MilCmdGeometryGroup;
			mILCMD_GEOMETRYGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_GEOMETRYGROUP.hTransform = hTransform;
			mILCMD_GEOMETRYGROUP.FillRule = FillRule;
			mILCMD_GEOMETRYGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			channel.BeginCommand((byte*)(&mILCMD_GEOMETRYGROUP), sizeof(DUCE.MILCMD_GEOMETRYGROUP), (int)mILCMD_GEOMETRYGROUP.ChildrenSize);
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
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GEOMETRYGROUP))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			GeometryCollection children = Children;
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
		((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
		GeometryCollection children = Children;
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

	static GeometryGroup()
	{
		s_Children = GeometryCollection.Empty;
		Type typeFromHandle = typeof(GeometryGroup);
		FillRuleProperty = Animatable.RegisterProperty("FillRule", typeof(FillRule), typeFromHandle, FillRule.EvenOdd, FillRulePropertyChanged, ValidateEnums.IsFillRuleValid, isIndependentlyAnimated: false, null);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(GeometryCollection), typeFromHandle, new FreezableDefaultValueFactory(GeometryCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
