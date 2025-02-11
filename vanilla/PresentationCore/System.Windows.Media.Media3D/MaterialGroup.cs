using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media.Media3D;

/// <summary>Represents a <see cref="T:System.Windows.Media.Media3D.Material" /> that is a composite of the Materials in its collection.</summary>
[ContentProperty("Children")]
public sealed class MaterialGroup : Material
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.MaterialGroup.Children" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.MaterialGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static MaterialCollection s_Children;

	/// <summary> Gets or sets a collection of child <see cref="T:System.Windows.Media.Media3D.Material" /> objects.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.MaterialCollection" /> that contains the child <see cref="T:System.Windows.Media.Media3D.Material" /> objects.</returns>
	public MaterialCollection Children
	{
		get
		{
			return (MaterialCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.MaterialGroup" />class. </summary>
	public MaterialGroup()
	{
	}

	/// <summary>Returns a modifiable copy of the <see cref="T:System.Windows.Media.Media3D.MaterialGroup" />.</summary>
	/// <returns>A modifiable copy of the <see cref="T:System.Windows.Media.Media3D.MaterialGroup" />. The returned copy is effectively a deep clone of the current object, although some copying might be deferred until necessary for improved performance. The copy's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false.</returns>
	public new MaterialGroup Clone()
	{
		return (MaterialGroup)base.Clone();
	}

	/// <summary>Returns a non-animated version of the <see cref="T:System.Windows.Media.Media3D.MaterialGroup" />. The returned <see cref="T:System.Windows.Media.Media3D.MaterialGroup" /> represents the current object's state at the time this method was called.</summary>
	/// <returns>Returns the current value of the <see cref="T:System.Windows.Media.Media3D.MaterialGroup" />. The returned <see cref="T:System.Windows.Media.Media3D.MaterialGroup" /> has the same value as the instantaneous value of the current object, but is not animated.</returns>
	public new MaterialGroup CloneCurrentValue()
	{
		return (MaterialGroup)base.CloneCurrentValue();
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		MaterialGroup materialGroup = (MaterialGroup)d;
		MaterialCollection materialCollection = null;
		MaterialCollection materialCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			materialCollection = (MaterialCollection)e.OldValue;
			if (materialCollection != null && !materialCollection.IsFrozen)
			{
				materialCollection.ItemRemoved -= materialGroup.ChildrenItemRemoved;
				materialCollection.ItemInserted -= materialGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			materialCollection2 = (MaterialCollection)e.NewValue;
			if (materialCollection2 != null && !materialCollection2.IsFrozen)
			{
				materialCollection2.ItemInserted += materialGroup.ChildrenItemInserted;
				materialCollection2.ItemRemoved += materialGroup.ChildrenItemRemoved;
			}
		}
		if (materialCollection != materialCollection2 && materialGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = materialGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (materialCollection2 != null)
					{
						int count = materialCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)materialCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (materialCollection != null)
					{
						int count2 = materialCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)materialCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		materialGroup.PropertyChanged(ChildrenProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new MaterialGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			MaterialCollection children = Children;
			int num = children?.Count ?? 0;
			DUCE.MILCMD_MATERIALGROUP mILCMD_MATERIALGROUP = default(DUCE.MILCMD_MATERIALGROUP);
			mILCMD_MATERIALGROUP.Type = MILCMD.MilCmdMaterialGroup;
			mILCMD_MATERIALGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_MATERIALGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			channel.BeginCommand((byte*)(&mILCMD_MATERIALGROUP), sizeof(DUCE.MILCMD_MATERIALGROUP), (int)mILCMD_MATERIALGROUP.ChildrenSize);
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
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MATERIALGROUP))
		{
			MaterialCollection children = Children;
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
		MaterialCollection children = Children;
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

	static MaterialGroup()
	{
		s_Children = MaterialCollection.Empty;
		Type typeFromHandle = typeof(MaterialGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(MaterialCollection), typeFromHandle, new FreezableDefaultValueFactory(MaterialCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
