using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a transformation that is a composite of the <see cref="T:System.Windows.Media.Media3D.Transform3D" /> children in its <see cref="T:System.Windows.Media.Media3D.Transform3DCollection" />. </summary>
[ContentProperty("Children")]
public sealed class Transform3DGroup : Transform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Transform3DGroup.Children" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.Transform3DGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Transform3DCollection s_Children;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> that indicates the current transform value. </summary>
	/// <returns>Matrix3D that indicates the current transform value.</returns>
	public override Matrix3D Value
	{
		get
		{
			ReadPreamble();
			Matrix3D matrix = default(Matrix3D);
			Append(ref matrix);
			return matrix;
		}
	}

	/// <summary>Gets a value that indicates whether the transformation is affine. </summary>
	/// <returns>True if the transformation is affine; false otherwise.</returns>
	public override bool IsAffine
	{
		get
		{
			ReadPreamble();
			Transform3DCollection children = Children;
			if (children != null)
			{
				int i = 0;
				for (int count = children.Count; i < count; i++)
				{
					if (!children.Internal_GetItem(i).IsAffine)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	/// <summary> Gets or sets a collection of <see cref="T:System.Windows.Media.Media3D.Transform3D" /> objects.  </summary>
	/// <returns>Collection of <see cref="T:System.Windows.Media.Media3D.Transform3D" /> objects. The default value is an empty collection.</returns>
	public Transform3DCollection Children
	{
		get
		{
			return (Transform3DCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.Transform3DGroup" /> class.</summary>
	public Transform3DGroup()
	{
	}

	internal override void Append(ref Matrix3D matrix)
	{
		Transform3DCollection children = Children;
		if (children != null)
		{
			int i = 0;
			for (int count = children.Count; i < count; i++)
			{
				children.Internal_GetItem(i).Append(ref matrix);
			}
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3DGroup" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Transform3DGroup Clone()
	{
		return (Transform3DGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Transform3DGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Transform3DGroup CloneCurrentValue()
	{
		return (Transform3DGroup)base.CloneCurrentValue();
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Transform3DGroup transform3DGroup = (Transform3DGroup)d;
		Transform3DCollection transform3DCollection = null;
		Transform3DCollection transform3DCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			transform3DCollection = (Transform3DCollection)e.OldValue;
			if (transform3DCollection != null && !transform3DCollection.IsFrozen)
			{
				transform3DCollection.ItemRemoved -= transform3DGroup.ChildrenItemRemoved;
				transform3DCollection.ItemInserted -= transform3DGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			transform3DCollection2 = (Transform3DCollection)e.NewValue;
			if (transform3DCollection2 != null && !transform3DCollection2.IsFrozen)
			{
				transform3DCollection2.ItemInserted += transform3DGroup.ChildrenItemInserted;
				transform3DCollection2.ItemRemoved += transform3DGroup.ChildrenItemRemoved;
			}
		}
		if (transform3DCollection != transform3DCollection2 && transform3DGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = transform3DGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (transform3DCollection2 != null)
					{
						int count = transform3DCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)transform3DCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (transform3DCollection != null)
					{
						int count2 = transform3DCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)transform3DCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		transform3DGroup.PropertyChanged(ChildrenProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new Transform3DGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform3DCollection children = Children;
			int num = children?.Count ?? 0;
			DUCE.MILCMD_TRANSFORM3DGROUP mILCMD_TRANSFORM3DGROUP = default(DUCE.MILCMD_TRANSFORM3DGROUP);
			mILCMD_TRANSFORM3DGROUP.Type = MILCMD.MilCmdTransform3DGroup;
			mILCMD_TRANSFORM3DGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_TRANSFORM3DGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			channel.BeginCommand((byte*)(&mILCMD_TRANSFORM3DGROUP), sizeof(DUCE.MILCMD_TRANSFORM3DGROUP), (int)mILCMD_TRANSFORM3DGROUP.ChildrenSize);
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
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_TRANSFORM3DGROUP))
		{
			Transform3DCollection children = Children;
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
		Transform3DCollection children = Children;
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

	static Transform3DGroup()
	{
		s_Children = Transform3DCollection.Empty;
		Type typeFromHandle = typeof(Transform3DGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(Transform3DCollection), typeFromHandle, new FreezableDefaultValueFactory(Transform3DCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
