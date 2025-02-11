using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Converters;
using MS.Internal;

namespace System.Windows.Media;

/// <summary>Defines objects used to paint graphical objects. Classes that derive from <see cref="T:System.Windows.Media.Brush" /> describe how the area is painted.       </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
[TypeConverter(typeof(BrushConverter))]
[ValueSerializer(typeof(BrushValueSerializer))]
public abstract class Brush : Animatable, IFormattable, DUCE.IResource
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.Brush.Opacity" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Brush.Opacity" /> dependency property identifier.</returns>
	public static readonly DependencyProperty OpacityProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Brush.Transform" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Brush.Transform" /> dependency property identifier.</returns>
	public static readonly DependencyProperty TransformProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.Brush.RelativeTransform" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Brush.RelativeTransform" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RelativeTransformProperty;

	internal const double c_Opacity = 1.0;

	internal static Transform s_Transform;

	internal static Transform s_RelativeTransform;

	/// <summary> Gets or sets the degree of opacity of a <see cref="T:System.Windows.Media.Brush" />.  </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Media.Brush.Opacity" /> property is expressed as a value between 0.0 and 1.0. The default value is 1.0. </returns>
	public double Opacity
	{
		get
		{
			return (double)GetValue(OpacityProperty);
		}
		set
		{
			SetValueInternal(OpacityProperty, value);
		}
	}

	/// <summary> Gets or sets the transformation that is applied to the brush. This transformation is applied after the brush's output has been mapped and positioned. </summary>
	/// <returns>The transformation to apply to the brush. The default value is the <see cref="P:System.Windows.Media.Transform.Identity" /> transformation.</returns>
	public Transform Transform
	{
		get
		{
			return (Transform)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	/// <summary>Gets or sets the transformation that is applied to the brush using relative coordinates. </summary>
	/// <returns>The transformation that is applied to the brush using relative coordinates.  The default value is the <see cref="P:System.Windows.Media.Transform.Identity" /> transformation.</returns>
	public Transform RelativeTransform
	{
		get
		{
			return (Transform)GetValue(RelativeTransformProperty);
		}
		set
		{
			SetValueInternal(RelativeTransformProperty, value);
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.Brush" /> class. </summary>
	protected Brush()
	{
	}

	internal static Brush Parse(string value, ITypeDescriptorContext context)
	{
		IFreezeFreezables freezeFreezables = null;
		Brush brush;
		if (context != null)
		{
			freezeFreezables = (IFreezeFreezables)context.GetService(typeof(IFreezeFreezables));
			if (freezeFreezables != null && freezeFreezables.FreezeFreezables)
			{
				brush = (Brush)freezeFreezables.TryGetFreezable(value);
				if (brush != null)
				{
					return brush;
				}
			}
		}
		brush = Parsers.ParseBrush(value, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS, context);
		if (brush != null && freezeFreezables != null && freezeFreezables.FreezeFreezables)
		{
			freezeFreezables.TryFreeze(value, brush);
		}
		return brush;
	}

	internal virtual bool CanSerializeToString()
	{
		return false;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Brush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Brush Clone()
	{
		return (Brush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Brush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Brush CloneCurrentValue()
	{
		return (Brush)base.CloneCurrentValue();
	}

	private static void OpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Brush)d).PropertyChanged(OpacityProperty);
	}

	private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Brush brush = (Brush)d;
		Transform resource = (Transform)e.OldValue;
		Transform resource2 = (Transform)e.NewValue;
		if (brush.Dispatcher != null)
		{
			DUCE.IResource resource3 = brush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					brush.ReleaseResource(resource, channel);
					brush.AddRefResource(resource2, channel);
				}
			}
		}
		brush.PropertyChanged(TransformProperty);
	}

	private static void RelativeTransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		Brush brush = (Brush)d;
		Transform resource = (Transform)e.OldValue;
		Transform resource2 = (Transform)e.NewValue;
		if (brush.Dispatcher != null)
		{
			DUCE.IResource resource3 = brush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					brush.ReleaseResource(resource, channel);
					brush.AddRefResource(resource2, channel);
				}
			}
		}
		brush.PropertyChanged(RelativeTransformProperty);
	}

	internal abstract DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal abstract void ReleaseOnChannelCore(DUCE.Channel channel);

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal abstract DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel);

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal abstract int GetChannelCountCore();

	int DUCE.IResource.GetChannelCount()
	{
		return GetChannelCountCore();
	}

	internal abstract DUCE.Channel GetChannelCore(int index);

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return GetChannelCore(index);
	}

	/// <summary> Return string representation of this <see cref="T:System.Windows.Media.Brush" />.              </summary>
	/// <returns>A string representation of this object.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary> Creates a string representation of this object based on the specified culture-specific formatting information.              </summary>
	/// <returns>A string representation of this object.</returns>
	/// <param name="provider">Culture-specific formatting information, or null to use the default formatting of the current culture.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}

	static Brush()
	{
		s_Transform = Transform.Identity;
		s_RelativeTransform = Transform.Identity;
		Type typeFromHandle = typeof(Brush);
		OpacityProperty = Animatable.RegisterProperty("Opacity", typeof(double), typeFromHandle, 1.0, OpacityPropertyChanged, null, isIndependentlyAnimated: true, null);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform), typeFromHandle, Transform.Identity, TransformPropertyChanged, null, isIndependentlyAnimated: false, null);
		RelativeTransformProperty = Animatable.RegisterProperty("RelativeTransform", typeof(Transform), typeFromHandle, Transform.Identity, RelativeTransformPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
