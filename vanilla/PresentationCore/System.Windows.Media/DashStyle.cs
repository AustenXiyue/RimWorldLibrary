using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Represents the sequence of dashes and gaps that will be applied by a <see cref="T:System.Windows.Media.Pen" />. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class DashStyle : Animatable, DUCE.IResource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.DashStyle.Offset" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.DashStyle.Offset" /> dependency property identifier.</returns>
	public static readonly DependencyProperty OffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.DashStyle.Dashes" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.DashStyle.Dashes" /> dependency property.</returns>
	public static readonly DependencyProperty DashesProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_Offset = 0.0;

	internal static DoubleCollection s_Dashes;

	/// <summary>Gets or sets how far in the dash sequence the stroke will start.  </summary>
	/// <returns>The offset for the dash sequence.  The default is 0.</returns>
	public double Offset
	{
		get
		{
			return (double)GetValue(OffsetProperty);
		}
		set
		{
			SetValueInternal(OffsetProperty, value);
		}
	}

	/// <summary>Gets or sets the collection of dashes and gaps in this <see cref="T:System.Windows.Media.DashStyle" />.  </summary>
	/// <returns>The collection of dashes and gaps.  The default is an empty <see cref="T:System.Windows.Media.DoubleCollection" />.</returns>
	public DoubleCollection Dashes
	{
		get
		{
			return (DoubleCollection)GetValue(DashesProperty);
		}
		set
		{
			SetValueInternal(DashesProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DashStyle" /> class. </summary>
	public DashStyle()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DashStyle" /> class with the specified <see cref="P:System.Windows.Media.DashStyle.Dashes" /> and <see cref="P:System.Windows.Media.DashStyle.Offset" />.</summary>
	/// <param name="dashes">The <see cref="P:System.Windows.Media.DashStyle.Dashes" /> of the <see cref="T:System.Windows.Media.DashStyle" />.</param>
	/// <param name="offset">The <see cref="P:System.Windows.Media.DashStyle.Offset" /> of the <see cref="T:System.Windows.Media.DashStyle" />.</param>
	public DashStyle(IEnumerable<double> dashes, double offset)
	{
		Offset = offset;
		if (dashes != null)
		{
			Dashes = new DoubleCollection(dashes);
		}
	}

	internal unsafe void GetDashData(MIL_PEN_DATA* pData, out double[] dashArray)
	{
		DoubleCollection dashes = Dashes;
		int num = 0;
		if (dashes != null)
		{
			num = dashes.Count;
		}
		pData->DashArraySize = (uint)(num * 8);
		pData->DashOffset = Offset;
		if (num > 0)
		{
			dashArray = dashes._collection.ToArray();
		}
		else
		{
			dashArray = null;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DashStyle" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new DashStyle Clone()
	{
		return (DashStyle)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DashStyle" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new DashStyle CloneCurrentValue()
	{
		return (DashStyle)base.CloneCurrentValue();
	}

	private static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DashStyle)d).PropertyChanged(OffsetProperty);
	}

	private static void DashesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DashStyle)d).PropertyChanged(DashesProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DashStyle();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DoubleCollection dashes = Dashes;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(OffsetProperty, channel);
			int num = dashes?.Count ?? 0;
			DUCE.MILCMD_DASHSTYLE mILCMD_DASHSTYLE = default(DUCE.MILCMD_DASHSTYLE);
			mILCMD_DASHSTYLE.Type = MILCMD.MilCmdDashStyle;
			mILCMD_DASHSTYLE.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_DASHSTYLE.Offset = Offset;
			}
			mILCMD_DASHSTYLE.hOffsetAnimations = animationResourceHandle;
			mILCMD_DASHSTYLE.DashesSize = (uint)(8 * num);
			channel.BeginCommand((byte*)(&mILCMD_DASHSTYLE), sizeof(DUCE.MILCMD_DASHSTYLE), (int)mILCMD_DASHSTYLE.DashesSize);
			for (int i = 0; i < num; i++)
			{
				double num2 = dashes.Internal_GetItem(i);
				channel.AppendCommandData((byte*)(&num2), 8);
			}
			channel.EndCommand();
		}
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DASHSTYLE))
			{
				AddRefOnChannelAnimations(channel);
				UpdateResource(channel, skipOnChannelCheck: true);
			}
			return _duceResource.GetHandle(channel);
		}
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.ReleaseOnChannel(channel))
			{
				ReleaseOnChannelAnimations(channel);
			}
		}
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return _duceResource.GetHandle(channel);
		}
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _duceResource.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static DashStyle()
	{
		s_Dashes = DoubleCollection.Empty;
		Type typeFromHandle = typeof(DashStyle);
		OffsetProperty = Animatable.RegisterProperty("Offset", typeof(double), typeFromHandle, 0.0, OffsetPropertyChanged, null, isIndependentlyAnimated: true, null);
		DashesProperty = Animatable.RegisterProperty("Dashes", typeof(DoubleCollection), typeFromHandle, new FreezableDefaultValueFactory(DoubleCollection.Empty), DashesPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
