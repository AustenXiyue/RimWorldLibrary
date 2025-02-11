using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media;

/// <summary> Describes the location and color of a transition point in a gradient. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class GradientStop : Animatable, IFormattable
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientStop.Color" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.GradientStop.Color" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeof(GradientStop), Colors.Transparent, null, null, isIndependentlyAnimated: false, null);

	/// <summary> Identifies the <see cref="P:System.Windows.Media.GradientStop.Offset" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.GradientStop.Offset" /> dependency property identifier.</returns>
	public static readonly DependencyProperty OffsetProperty = Animatable.RegisterProperty("Offset", typeof(double), typeof(GradientStop), 0.0, null, null, isIndependentlyAnimated: false, null);

	internal static Color s_Color = Colors.Transparent;

	internal const double c_Offset = 0.0;

	/// <summary> Gets or sets the color of the gradient stop. </summary>
	/// <returns>The color of the gradient stop. The default value is <see cref="P:System.Windows.Media.Colors.Transparent" />.</returns>
	public Color Color
	{
		get
		{
			return (Color)GetValue(ColorProperty);
		}
		set
		{
			SetValueInternal(ColorProperty, value);
		}
	}

	/// <summary> Gets the location of the gradient stop within the gradient vector. </summary>
	/// <returns>The relative location of this gradient stop along the gradient vector. The default value is 0.0. </returns>
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

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GradientStop" /> class. </summary>
	public GradientStop()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GradientStop" /> class with the specified color and offset. </summary>
	/// <param name="color">The color value of the gradient stop.</param>
	/// <param name="offset">The location in the gradient where the gradient stop is placed.</param>
	public GradientStop(Color color, double offset)
	{
		Color = color;
		Offset = offset;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientStop" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GradientStop Clone()
	{
		return (GradientStop)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GradientStop" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GradientStop CloneCurrentValue()
	{
		return (GradientStop)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GradientStop();
	}

	/// <summary> Creates a string representation of this object based on the current culture. </summary>
	/// <returns>A string representation of this object that contains its <see cref="P:System.Windows.Media.GradientStop.Color" /> and <see cref="P:System.Windows.Media.GradientStop.Offset" /> values. </returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary> Creates a string representation of this object based on the specified culture-specific formatting information. </summary>
	/// <returns>A string representation of this object that contains its <see cref="P:System.Windows.Media.GradientStop.Color" /> and <see cref="P:System.Windows.Media.GradientStop.Offset" /> values.</returns>
	/// <param name="provider">Culture specific formatting information, or null to use the current culture.</param>
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

	internal string ConvertToString(string format, IFormatProvider provider)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
		return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}", numericListSeparator, Color, Offset);
	}
}
