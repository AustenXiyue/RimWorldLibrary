using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using MS.Internal;

namespace System.Windows;

/// <summary>Converts instances of other types to and from a <see cref="T:System.Windows.CornerRadius" />. </summary>
public class CornerRadiusConverter : TypeConverter
{
	/// <summary>Indicates whether an object can be converted from a given type to a <see cref="T:System.Windows.CornerRadius" />. </summary>
	/// <returns>true if <paramref name="sourceType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="sourceType">The source <see cref="T:System.Type" /> that is being queried for conversion support.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether <see cref="T:System.Windows.CornerRadius" /> values can be converted to the specified type. </summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type this <see cref="T:System.Windows.CornerRadius" /> is being evaluated to be converted to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.CornerRadius" />.</summary>
	/// <returns>The <see cref="T:System.Windows.CornerRadius" /> created from converting <paramref name="source" />.</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="source">The object being converted.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="source" /> is not null and is not a valid type which can be converted to a <see cref="T:System.Windows.CornerRadius" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			return new CornerRadius(Convert.ToDouble(source, cultureInfo));
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.CornerRadius" /> to the specified type.</summary>
	/// <returns>The object created from converting this <see cref="T:System.Windows.CornerRadius" /> (a string).</returns>
	/// <param name="typeDescriptorContext">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Describes the <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The <see cref="T:System.Windows.CornerRadius" /> to convert.</param>
	/// <param name="destinationType">The type to convert the <see cref="T:System.Windows.CornerRadius" /> to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not null and is not a <see cref="T:System.Windows.Media.Brush" />, or if <paramref name="destinationType" /> is not one of the valid destination types.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (null == destinationType)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (!(value is CornerRadius cornerRadius))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(CornerRadius)), "value");
		}
		if (destinationType == typeof(string))
		{
			return ToString(cornerRadius, cultureInfo);
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return new InstanceDescriptor(typeof(CornerRadius).GetConstructor(new Type[4]
			{
				typeof(double),
				typeof(double),
				typeof(double),
				typeof(double)
			}), new object[4] { cornerRadius.TopLeft, cornerRadius.TopRight, cornerRadius.BottomRight, cornerRadius.BottomLeft });
		}
		throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(CornerRadius), destinationType.FullName));
	}

	internal static string ToString(CornerRadius cr, CultureInfo cultureInfo)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(cultureInfo);
		Span<char> initialBuffer = stackalloc char[64];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(0, 7, cultureInfo, initialBuffer);
		handler.AppendFormatted(cr.TopLeft);
		handler.AppendFormatted(numericListSeparator);
		handler.AppendFormatted(cr.TopRight);
		handler.AppendFormatted(numericListSeparator);
		handler.AppendFormatted(cr.BottomRight);
		handler.AppendFormatted(numericListSeparator);
		handler.AppendFormatted(cr.BottomLeft);
		return string.Create(cultureInfo, initialBuffer, ref handler);
	}

	internal static CornerRadius FromString(string s, CultureInfo cultureInfo)
	{
		TokenizerHelper tokenizerHelper = new TokenizerHelper(s, cultureInfo);
		double[] array = new double[4];
		int num = 0;
		while (tokenizerHelper.NextToken())
		{
			if (num >= 4)
			{
				num = 5;
				break;
			}
			array[num] = double.Parse(tokenizerHelper.GetCurrentToken(), cultureInfo);
			num++;
		}
		return num switch
		{
			1 => new CornerRadius(array[0]), 
			4 => new CornerRadius(array[0], array[1], array[2], array[3]), 
			_ => throw new FormatException(SR.Format(SR.InvalidStringCornerRadius, s)), 
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.CornerRadiusConverter" /> class.</summary>
	public CornerRadiusConverter()
	{
	}
}
