using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Represents a composite <see cref="T:System.Windows.Media.Transform" /> composed of other <see cref="T:System.Windows.Media.Transform" /> objects. </summary>
[ContentProperty("Children")]
public sealed class TransformGroup : Transform
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.TransformGroup.Children" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TransformGroup.Children" /> dependency property.</returns>
	public static readonly DependencyProperty ChildrenProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static TransformCollection s_Children;

	/// <summary>Gets the <see cref="T:System.Windows.Media.Matrix" /> structure that describes the transformation represented by this <see cref="T:System.Windows.Media.TransformGroup" />. </summary>
	/// <returns>A composite of the <see cref="T:System.Windows.Media.Transform" /> objects in this <see cref="T:System.Windows.Media.TransformGroup" />.</returns>
	public override Matrix Value
	{
		get
		{
			ReadPreamble();
			TransformCollection children = Children;
			if (children == null || children.Count == 0)
			{
				return default(Matrix);
			}
			Matrix value = children.Internal_GetItem(0).Value;
			for (int i = 1; i < children.Count; i++)
			{
				value *= children.Internal_GetItem(i).Value;
			}
			return value;
		}
	}

	internal override bool IsIdentity
	{
		get
		{
			TransformCollection children = Children;
			if (children == null || children.Count == 0)
			{
				return true;
			}
			for (int i = 0; i < children.Count; i++)
			{
				if (!children.Internal_GetItem(i).IsIdentity)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.TransformCollection" /> that defines this <see cref="T:System.Windows.Media.TransformGroup" />.  </summary>
	/// <returns>A collection of <see cref="T:System.Windows.Media.Transform" /> objects that define this <see cref="T:System.Windows.Media.TransformGroup" />. The default is an empty collection. </returns>
	public TransformCollection Children
	{
		get
		{
			return (TransformCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TransformGroup" /> class.</summary>
	public TransformGroup()
	{
	}

	internal override bool CanSerializeToString()
	{
		return false;
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TransformGroup" /> by making deep copies of its values.</summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object returns false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new TransformGroup Clone()
	{
		return (TransformGroup)base.Clone();
	}

	/// <summary>Creates a modifiable copy of this <see cref="T:System.Windows.Media.TransformGroup" /> object by making deep copies of its values. This method does not copy resource references, data bindings, or animations, although it does copy their current values. </summary>
	/// <returns>A modifiable deep copy of the current object. The <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the cloned object is false even if the <see cref="P:System.Windows.Freezable.IsFrozen" /> property of the source is true.</returns>
	public new TransformGroup CloneCurrentValue()
	{
		return (TransformGroup)base.CloneCurrentValue();
	}

	private static void ChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		TransformGroup transformGroup = (TransformGroup)d;
		TransformCollection transformCollection = null;
		TransformCollection transformCollection2 = null;
		if (e.OldValueSource != BaseValueSourceInternal.Default || e.IsOldValueModified)
		{
			transformCollection = (TransformCollection)e.OldValue;
			if (transformCollection != null && !transformCollection.IsFrozen)
			{
				transformCollection.ItemRemoved -= transformGroup.ChildrenItemRemoved;
				transformCollection.ItemInserted -= transformGroup.ChildrenItemInserted;
			}
		}
		if (e.NewValueSource != BaseValueSourceInternal.Default || e.IsNewValueModified)
		{
			transformCollection2 = (TransformCollection)e.NewValue;
			if (transformCollection2 != null && !transformCollection2.IsFrozen)
			{
				transformCollection2.ItemInserted += transformGroup.ChildrenItemInserted;
				transformCollection2.ItemRemoved += transformGroup.ChildrenItemRemoved;
			}
		}
		if (transformCollection != transformCollection2 && transformGroup.Dispatcher != null)
		{
			using (CompositionEngineLock.Acquire())
			{
				DUCE.IResource resource = transformGroup;
				int channelCount = resource.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource.GetChannel(i);
					if (transformCollection2 != null)
					{
						int count = transformCollection2.Count;
						for (int j = 0; j < count; j++)
						{
							((DUCE.IResource)transformCollection2.Internal_GetItem(j)).AddRefOnChannel(channel);
						}
					}
					if (transformCollection != null)
					{
						int count2 = transformCollection.Count;
						for (int k = 0; k < count2; k++)
						{
							((DUCE.IResource)transformCollection.Internal_GetItem(k)).ReleaseOnChannel(channel);
						}
					}
				}
			}
		}
		transformGroup.PropertyChanged(ChildrenProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TransformGroup();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			TransformCollection children = Children;
			int num = children?.Count ?? 0;
			DUCE.MILCMD_TRANSFORMGROUP mILCMD_TRANSFORMGROUP = default(DUCE.MILCMD_TRANSFORMGROUP);
			mILCMD_TRANSFORMGROUP.Type = MILCMD.MilCmdTransformGroup;
			mILCMD_TRANSFORMGROUP.Handle = _duceResource.GetHandle(channel);
			mILCMD_TRANSFORMGROUP.ChildrenSize = (uint)(sizeof(DUCE.ResourceHandle) * num);
			channel.BeginCommand((byte*)(&mILCMD_TRANSFORMGROUP), sizeof(DUCE.MILCMD_TRANSFORMGROUP), (int)mILCMD_TRANSFORMGROUP.ChildrenSize);
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
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_TRANSFORMGROUP))
		{
			TransformCollection children = Children;
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
		TransformCollection children = Children;
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

	static TransformGroup()
	{
		s_Children = TransformCollection.Empty;
		Type typeFromHandle = typeof(TransformGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(TransformCollection), typeFromHandle, new FreezableDefaultValueFactory(TransformCollection.Empty), ChildrenPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
