using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Media;

/// <summary>Represents a collection of guide lines that can assist in adjusting rendered figures to a device pixel grid.</summary>
public sealed class GuidelineSet : Animatable, DUCE.IResource
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesX" /> dependency property.</returns>
	public static readonly DependencyProperty GuidelinesXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesY" /> dependency property.</returns>
	public static readonly DependencyProperty GuidelinesYProperty;

	internal static readonly DependencyProperty IsDynamicProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static DoubleCollection s_GuidelinesX;

	internal static DoubleCollection s_GuidelinesY;

	internal const bool c_IsDynamic = false;

	/// <summary>Gets or sets a series of coordinate values that represent guide lines on the X-axis.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.DoubleCollection" /> of values that represent guide lines on the X-axis.</returns>
	public DoubleCollection GuidelinesX
	{
		get
		{
			return (DoubleCollection)GetValue(GuidelinesXProperty);
		}
		set
		{
			SetValueInternal(GuidelinesXProperty, value);
		}
	}

	/// <summary>Gets or sets a series of coordinate values that represent guide lines on the Y-axis.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.DoubleCollection" /> of values that represent guide lines on the Y-axis.</returns>
	public DoubleCollection GuidelinesY
	{
		get
		{
			return (DoubleCollection)GetValue(GuidelinesYProperty);
		}
		set
		{
			SetValueInternal(GuidelinesYProperty, value);
		}
	}

	internal bool IsDynamic
	{
		get
		{
			return (bool)GetValue(IsDynamicProperty);
		}
		set
		{
			SetValueInternal(IsDynamicProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GuidelineSet" /> class.</summary>
	public GuidelineSet()
	{
	}

	internal GuidelineSet(double[] guidelinesX, double[] guidelinesY, bool isDynamic)
	{
		if (guidelinesX != null)
		{
			GuidelinesX = new DoubleCollection(guidelinesX);
		}
		if (guidelinesY != null)
		{
			GuidelinesY = new DoubleCollection(guidelinesY);
		}
		IsDynamic = isDynamic;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GuidelineSet" /> class with the specified <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesX" /> and <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesY" /> values.</summary>
	/// <param name="guidelinesX">The value of the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesX" /> property.</param>
	/// <param name="guidelinesY">The value of the <see cref="P:System.Windows.Media.GuidelineSet.GuidelinesY" /> property.</param>
	public GuidelineSet(double[] guidelinesX, double[] guidelinesY)
	{
		if (guidelinesX != null)
		{
			GuidelinesX = new DoubleCollection(guidelinesX);
		}
		if (guidelinesY != null)
		{
			GuidelinesY = new DoubleCollection(guidelinesY);
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GuidelineSet" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GuidelineSet Clone()
	{
		return (GuidelineSet)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GuidelineSet" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GuidelineSet CloneCurrentValue()
	{
		return (GuidelineSet)base.CloneCurrentValue();
	}

	private static void GuidelinesXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GuidelineSet)d).PropertyChanged(GuidelinesXProperty);
	}

	private static void GuidelinesYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GuidelineSet)d).PropertyChanged(GuidelinesYProperty);
	}

	private static void IsDynamicPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((GuidelineSet)d).PropertyChanged(IsDynamicProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GuidelineSet();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DoubleCollection guidelinesX = GuidelinesX;
			DoubleCollection guidelinesY = GuidelinesY;
			int num = guidelinesX?.Count ?? 0;
			int num2 = guidelinesY?.Count ?? 0;
			DUCE.MILCMD_GUIDELINESET mILCMD_GUIDELINESET = default(DUCE.MILCMD_GUIDELINESET);
			mILCMD_GUIDELINESET.Type = MILCMD.MilCmdGuidelineSet;
			mILCMD_GUIDELINESET.Handle = _duceResource.GetHandle(channel);
			mILCMD_GUIDELINESET.GuidelinesXSize = (uint)(8 * num);
			mILCMD_GUIDELINESET.GuidelinesYSize = (uint)(8 * num2);
			mILCMD_GUIDELINESET.IsDynamic = CompositionResourceManager.BooleanToUInt32(IsDynamic);
			channel.BeginCommand((byte*)(&mILCMD_GUIDELINESET), sizeof(DUCE.MILCMD_GUIDELINESET), (int)(mILCMD_GUIDELINESET.GuidelinesXSize + mILCMD_GUIDELINESET.GuidelinesYSize));
			for (int i = 0; i < num; i++)
			{
				double num3 = guidelinesX.Internal_GetItem(i);
				channel.AppendCommandData((byte*)(&num3), 8);
			}
			for (int j = 0; j < num2; j++)
			{
				double num4 = guidelinesY.Internal_GetItem(j);
				channel.AppendCommandData((byte*)(&num4), 8);
			}
			channel.EndCommand();
		}
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GUIDELINESET))
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

	static GuidelineSet()
	{
		s_GuidelinesX = DoubleCollection.Empty;
		s_GuidelinesY = DoubleCollection.Empty;
		Type typeFromHandle = typeof(GuidelineSet);
		GuidelinesXProperty = Animatable.RegisterProperty("GuidelinesX", typeof(DoubleCollection), typeFromHandle, new FreezableDefaultValueFactory(DoubleCollection.Empty), GuidelinesXPropertyChanged, null, isIndependentlyAnimated: false, null);
		GuidelinesYProperty = Animatable.RegisterProperty("GuidelinesY", typeof(DoubleCollection), typeFromHandle, new FreezableDefaultValueFactory(DoubleCollection.Empty), GuidelinesYPropertyChanged, null, isIndependentlyAnimated: false, null);
		IsDynamicProperty = Animatable.RegisterProperty("IsDynamic", typeof(bool), typeFromHandle, false, IsDynamicPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
