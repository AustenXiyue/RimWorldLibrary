using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Converts instances of <see cref="T:System.Windows.FontWeight" /> to and from other data types.</summary>
public sealed class FontWeightConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether this converter can convert an object of the given type to an instance of <see cref="T:System.Windows.FontWeight" />.</summary>
	/// <returns>true if the converter can convert the provided type to an instance of <see cref="T:System.Windows.FontWeight" />; otherwise, false.</returns>
	/// <param name="td">Context information of a type.</param>
	/// <param name="t">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.FontWeight" /> can be converted to a different type.</summary>
	/// <returns>true if the converter can convert this instance of <see cref="T:System.Windows.FontWeight" />; otherwise, false.</returns>
	/// <param name="context">Context information of a type.</param>
	/// <param name="destinationType">The desired type that that this instance of <see cref="T:System.Windows.FontWeight" /> is being evaluated for conversion to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.FontWeight" />.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.FontWeight" /> created from the converted <paramref name="value" />.</returns>
	/// <param name="td">Context information of a type.</param>
	/// <param name="ci">
	///   <see cref="T:System.Globalization.CultureInfo" /> of the type being converted. </param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a valid type for conversion.</exception>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo ci, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (!(value is string s))
		{
			throw new ArgumentException(SR.Format(SR.General_BadType, "ConvertFrom"), "value");
		}
		FontWeight fontWeight = default(FontWeight);
		if (!FontWeights.FontWeightStringToKnownWeight(s, ci, ref fontWeight))
		{
			throw new FormatException(SR.Parsers_IllegalToken);
		}
		return fontWeight;
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.FontWeight" /> to a specified type.</summary>
	/// <returns>The object created from the converted instance of <see cref="T:System.Windows.FontWeight" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The instance of <see cref="T:System.Windows.FontWeight" /> to convert.</param>
	/// <param name="destinationType">The type this instance of <see cref="T:System.Windows.FontWeight" /> is converted to. </param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null-or-<paramref name="value" /> i is not an instance of <see cref="T:System.Windows.FontWeight" />-or-<paramref name="destinationType" /> is not a valid destination type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is FontWeight)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				MethodInfo? method = typeof(FontWeight).GetMethod("FromOpenTypeWeight", new Type[1] { typeof(int) });
				FontWeight fontWeight = (FontWeight)value;
				return new InstanceDescriptor(method, new object[1] { fontWeight.ToOpenTypeWeight() });
			}
			if (destinationType == typeof(string))
			{
				return ((IFormattable)(FontWeight)value).ToString((string?)null, (IFormatProvider?)culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FontWeightConverter" /> class.</summary>
	public FontWeightConverter()
	{
	}
}
