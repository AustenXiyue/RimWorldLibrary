using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts a <see cref="T:System.Windows.Media.PixelFormat" /> to and from other data types.</summary>
public sealed class PixelFormatConverter : TypeConverter
{
	/// <summary>Determines whether the converter can convert an object of the given type to an instance of <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>true if the converter can convert the provided type to an instance of <see cref="T:System.Windows.Media.PixelFormat" />; otherwise, false.</returns>
	/// <param name="td">Type context information used to evaluate conversion.</param>
	/// <param name="t">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Media.PixelFormat" /> can be converted to a different type. </summary>
	/// <returns>true if the converter can convert this instance of <see cref="T:System.Windows.Media.PixelFormat" />; otherwise, false.</returns>
	/// <param name="context">Type context information used to evaluate conversion.</param>
	/// <param name="destinationType">The desired type to evaluate the conversion to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a string to a <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.PixelFormat" />.</returns>
	/// <param name="value">The string to convert to a <see cref="T:System.Windows.Media.PixelFormat" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> could not be converted to a known <see cref="T:System.Windows.Media.PixelFormat" />.</exception>
	public new object ConvertFromString(string value)
	{
		if (value == null)
		{
			return null;
		}
		return new PixelFormat(value);
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.Media.PixelFormat" />.</summary>
	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="td">Type context information used for conversion.</param>
	/// <param name="ci">Cultural information that is respected during conversion.</param>
	/// <param name="o">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="o" /> is null or is an invalid type.</exception>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo ci, object o)
	{
		if (o == null)
		{
			return null;
		}
		return new PixelFormat(o as string);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.PixelFormat" /> to a specified type.</summary>
	/// <returns>A new instance of the <paramref name="destinationType" />.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="culture">Cultural information that is respected during conversion.</param>
	/// <param name="value">
	///   <see cref="T:System.Windows.Media.PixelFormat" /> to convert.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a valid type.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (null == destinationType)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is PixelFormat))
		{
			throw new ArgumentException(SR.Format(SR.General_Expected_Type, "PixelFormat"));
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			ConstructorInfo? constructor = typeof(PixelFormat).GetConstructor(new Type[1] { typeof(string) });
			PixelFormat pixelFormat = (PixelFormat)value;
			return new InstanceDescriptor(constructor, new object[1] { pixelFormat.ToString() });
		}
		if (destinationType == typeof(string))
		{
			return ((PixelFormat)value/*cast due to .constrained prefix*/).ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Creates a new instance of a <see cref="T:System.Windows.Media.PixelFormatConverter" /> class.</summary>
	public PixelFormatConverter()
	{
	}
}
