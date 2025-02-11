using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Text;
using MS.Internal;

namespace System.Windows;

/// <summary>Converts instances of other types to and from instances of <see cref="T:System.Windows.Thickness" />.</summary>
public class ThicknessConverter : TypeConverter
{
	/// <summary>Determines whether the type converter can create an instance of <see cref="T:System.Windows.Thickness" /> from a specified type.</summary>
	/// <returns>true if the type converter can create an instance of <see cref="T:System.Windows.Thickness" /> from the specified type; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">The context information of a type.</param>
	/// <param name="sourceType">The source type that the type converter is evaluating for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether the type converter can convert an instance of <see cref="T:System.Windows.Thickness" /> to a different type. </summary>
	/// <returns>true if the type converter can convert this instance of <see cref="T:System.Windows.Thickness" /> to the <paramref name="destinationType" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">The context information of a type.</param>
	/// <param name="destinationType">The type for which the type converter is evaluating this instance of <see cref="T:System.Windows.Thickness" /> for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Attempts to create an instance of <see cref="T:System.Windows.Thickness" /> from a specified object. </summary>
	/// <returns>An instance of <see cref="T:System.Windows.Thickness" /> created from the converted <paramref name="source" />.</returns>
	/// <param name="typeDescriptorContext">The context information for a type.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="source">The <paramref name="source" /><see cref="T:System.Object" /> being converted.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="source" /> object is a null reference (Nothing in Visual Basic).</exception>
	/// <exception cref="T:System.ArgumentException">The example object is not a null reference and is not a valid type that can be converted to a <see cref="T:System.Windows.Thickness" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			if (source is double)
			{
				return new Thickness((double)source);
			}
			return new Thickness(Convert.ToDouble(source, cultureInfo));
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Thickness" /> to a specified type. </summary>
	/// <returns>The type that is created when the type converter converts an instance of <see cref="T:System.Windows.Thickness" />.</returns>
	/// <param name="typeDescriptorContext">The context information of a type.</param>
	/// <param name="cultureInfo">The <see cref="T:System.Globalization.CultureInfo" /> of the type being converted.</param>
	/// <param name="value">The instance of <see cref="T:System.Windows.Thickness" /> to convert.</param>
	/// <param name="destinationType">The type that this instance of <see cref="T:System.Windows.Thickness" /> is converted to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="value" /> object is not a null reference (Nothing) and is not a Brush, or the <paramref name="destinationType" /> is not one of the valid types for conversion.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="value" /> object is a null reference.</exception>
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
		if (!(value is Thickness thickness))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(Thickness)), "value");
		}
		if (destinationType == typeof(string))
		{
			return ToString(thickness, cultureInfo);
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return new InstanceDescriptor(typeof(Thickness).GetConstructor(new Type[4]
			{
				typeof(double),
				typeof(double),
				typeof(double),
				typeof(double)
			}), new object[4] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom });
		}
		throw new ArgumentException(SR.Format(SR.CannotConvertType, typeof(Thickness), destinationType.FullName));
	}

	internal static string ToString(Thickness th, CultureInfo cultureInfo)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(cultureInfo);
		StringBuilder stringBuilder = new StringBuilder(64);
		stringBuilder.Append(LengthConverter.ToString(th.Left, cultureInfo));
		stringBuilder.Append(numericListSeparator);
		stringBuilder.Append(LengthConverter.ToString(th.Top, cultureInfo));
		stringBuilder.Append(numericListSeparator);
		stringBuilder.Append(LengthConverter.ToString(th.Right, cultureInfo));
		stringBuilder.Append(numericListSeparator);
		stringBuilder.Append(LengthConverter.ToString(th.Bottom, cultureInfo));
		return stringBuilder.ToString();
	}

	internal static Thickness FromString(string s, CultureInfo cultureInfo)
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
			array[num] = LengthConverter.FromString(tokenizerHelper.GetCurrentToken(), cultureInfo);
			num++;
		}
		return num switch
		{
			1 => new Thickness(array[0]), 
			2 => new Thickness(array[0], array[1], array[0], array[1]), 
			4 => new Thickness(array[0], array[1], array[2], array[3]), 
			_ => throw new FormatException(SR.Format(SR.InvalidStringThickness, s)), 
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.ThicknessConverter" /> class. </summary>
	public ThicknessConverter()
	{
	}
}
