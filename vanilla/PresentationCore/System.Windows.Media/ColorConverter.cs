using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary> Converts instances of other types to and from an instance of <see cref="T:System.Windows.Media.Color" />. </summary>
public sealed class ColorConverter : TypeConverter
{
	/// <summary> Determines whether an object can be converted from a given type to an instance of a <see cref="T:System.Windows.Media.Color" />.  </summary>
	/// <returns>true if the type can be converted to a <see cref="T:System.Windows.Media.Color" />; otherwise, false.</returns>
	/// <param name="td">Describes the context information of a type.</param>
	/// <param name="t">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary> Determines whether an instance of a <see cref="T:System.Windows.Media.Color" /> can be converted to a different type. </summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.Color" /> can be converted to <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.Media.Color" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a string to a <see cref="T:System.Windows.Media.Color" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Color" /> that represents the converted text.</returns>
	/// <param name="value">The string to convert to a <see cref="T:System.Windows.Media.Color" />.</param>
	public new static object ConvertFromString(string value)
	{
		if (value == null)
		{
			return null;
		}
		return Parsers.ParseColor(value, null);
	}

	/// <summary> Attempts to convert the specified object to a <see cref="T:System.Windows.Media.Color" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Color" /> created from converting <paramref name="value" />.</returns>
	/// <param name="td">Describes the context information of a type.</param>
	/// <param name="ci">Cultural information to respect during conversion.</param>
	/// <param name="value">The object being converted.</param>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo ci, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (!(value is string))
		{
			throw new ArgumentException(SR.Format(SR.General_BadType, "ConvertFrom"), "value");
		}
		return Parsers.ParseColor(value as string, ci, td);
	}

	/// <summary> Attempts to convert a <see cref="T:System.Windows.Media.Color" /> to a specified type. </summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.Media.Color" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.Media.Color" /> to convert.</param>
	/// <param name="destinationType">The type to convert this <see cref="T:System.Windows.Media.Color" /> to.</param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is Color)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				MethodInfo? method = typeof(Color).GetMethod("FromArgb", new Type[4]
				{
					typeof(byte),
					typeof(byte),
					typeof(byte),
					typeof(byte)
				});
				Color color = (Color)value;
				return new InstanceDescriptor(method, new object[4] { color.A, color.R, color.G, color.B });
			}
			if (destinationType == typeof(string))
			{
				return ((Color)value).ToString(culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.ColorConverter" />. </summary>
	public ColorConverter()
	{
	}
}
