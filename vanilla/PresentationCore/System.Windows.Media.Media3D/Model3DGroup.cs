using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media.Media3D;

/// <summary>Enables using a number of 3-D models as a unit. </summary>
[ContentProperty("Children")]
public sealed class Model3DGroup : Model3D
{
	private static Model3DGroup s_empty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Model3DGroup.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Model3DGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Model3DCollection s_Children;

	internal static Model3DGroup EmptyGroup
	{
		get
		{
			if (s_empty == null)
			{
				s_empty = new Model3DGroup();
				s_empty.Freeze();
			}
			return s_empty;
		}
	}

	/// <summary> Gets or sets a collection of <see cref="T:System.Windows.Media.Media3D.Model3D" /> objects.  </summary>
	/// <returns>Collection of <see cref="T:System.Windows.Media.Media3D.Model3D" /> objects.</returns>
	public Model3DCollection Children
	{
		get
		{
			return (Model3DCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Model3DGroup" /> class. </summary>
	public Model3DGroup()
	{
	}

	internal override void RayHitTestCore(RayHitTestParameters rayParams)
	{
		Model3DCollection children = Children;
		if (children != null)
		{
			for (int num = children.Count - 1; num >= 0; num--)
			{
				children.Internal_GetItem(num).RayHitTest(rayParams);
			}
		}
	}

	internal override Rect3D CalculateSubgraphBoundsInnerSpace()
	{
		Model3DCollection children = Children;
		if (children == null)
		{
			return Rect3D.Empty;
		}
		Rect3D empty = Rect3D.Empty;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			Model3D model3D = children.Internal_GetItem(i);
			empty.Union(model3D.CalculateSubgraphBoundsOuterSpace());
		}
		return empty;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Model3DGroup" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Model3DGroup Clone()
	{
		return (Model3DGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Model3DGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Model3DGroup CloneCurrentValue()
	{
		return (Model3DGroup)base.CloneCurrentValue();
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Model3DGroup model3DGroup = (Model3DGroup)d;
		Model3DCollection model3DCollection = null;
		Model3DCollection model3DCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			model3DCollection = (Model3DCollection)e.OldValue;
			if (model3DCollection != null && !model3DCollection.IsFrozen)
			{
				model3DCollection.ItemRemoved -= model3DGroup.ChildrenItemRemoved;
				model3DCollection.ItemInserted -= model3DGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			model3DCollection2 = (Model3DCollection)e.NewValue;
			if (model3DCollection2 != null && !model3DCollection2.IsFrozen)
			{
				model3DCollection2.ItemInserted += model3DGroup.ChildrenItemInserted;
				model3DCollection2.ItemRemoved += model3DGroup.ChildrenItemRemoved;
			}
		}
		if (model3DCollection != model3DCollection2 && model3DGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = model3DGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (model3DCollection2 != null)
					{
						int count = model3DCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)model3DCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (model3DCollection != null)
					{
						int count2 = model3DCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)model3DCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		model3DGroup.PropertyChanged(ChildrenProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Model3DGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3D transform = base.Transform;
			Model3DCollection children = Children;
			DUCE.ResourceHandle htransform = ((transform != null && transform != Transform3D.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			int num = children?.Count ?? 0;
			DUCE.MILCMD_MODEL3DGROUP mILCMD_MODEL3DGROUP = default(DUCE.MILCMD_MODEL3DGROUP);
			mILCMD_MODEL3DGROUP.Type = MILCMD.MilCmdModel3DGroup;
			mILCMD_MODEL3DGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_MODEL3DGROUP.htransform = htransform;
			mILCMD_MODEL3DGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			channel.BeginCommand((byte*)(&mILCMD_MODEL3DGROUP), sizeof(DUCE.MILCMD_MODEL3DGROUP), (int)mILCMD_MODEL3DGROUP.ChildrenSize);
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
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MODEL3DGROUP))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			Model3DCollection children = Children;
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
		Model3DCollection children = Children;
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

	static Model3DGroup()
	{
		s_Children = Model3DCollection.Empty;
		Type typeFromHandle = typeof(Model3DGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(Model3DCollection), typeFromHandle, new FreezableDefaultValueFactory(Model3DCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
