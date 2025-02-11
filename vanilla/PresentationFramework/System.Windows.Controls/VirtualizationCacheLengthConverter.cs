using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Converts objects to and from a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
public class VirtualizationCacheLengthConverter : TypeConverter
{
	/// <summary>Determines whether the <see cref="T:System.Windows.Controls.VirtualizationCacheLengthConverter" /> can convert an object of the specified type to a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />, using the specified context.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.VirtualizationCacheLengthConverter" /> can convert the specified type to a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An object that provides a format context.</param>
	/// <param name="sourceType">The type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
	{
		TypeCode typeCode = Type.GetTypeCode(sourceType);
		if ((uint)(typeCode - 7) <= 8u || typeCode == TypeCode.String)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether the <see cref="T:System.Windows.Controls.VirtualizationCacheLengthConverter" /> can convert a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> to the specified type.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.VirtualizationCacheLengthConverter" /> can convert a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> to the specified type; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">An object that provides a format context.</param>
	/// <param name="destinationType">The type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="typeDescriptorContext">An object that provides a format context.</param>
	/// <param name="cultureInfo">An object that provides the culture information that is used during conversion.</param>
	/// <param name="source">The object to convert to a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> is null.--or--<paramref name="source" /> cannot be converted to a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
	{
		if (source != null)
		{
			if (source is string)
			{
				return FromString((string)source, cultureInfo);
			}
			return new VirtualizationCacheLength(Convert.ToDouble(source, cultureInfo));
		}
		throw GetConvertFromException(source);
	}

	/// <summary>Converts the specified <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> to an object of the specified type.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="typeDescriptorContext">An object that provides a format context.</param>
	/// <param name="cultureInfo">An object that provides the culture information that is used during conversion.</param>
	/// <param name="value">A <see cref="T:System.Windows.Controls.VirtualizationCacheLength" /> to convert to another type.</param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is <paramref name="null" />.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null.--or--<paramref name="value" /> is not a <see cref="T:System.Windows.Controls.VirtualizationCacheLength" />.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value != null && value is VirtualizationCacheLength virtualizationCacheLength)
		{
			if (destinationType == typeof(string))
			{
				return ToString(virtualizationCacheLength, cultureInfo);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(VirtualizationCacheLength).GetConstructor(new Type[2]
				{
					typeof(double),
					typeof(VirtualizationCacheLengthUnit)
				}), new object[2] { virtualizationCacheLength.CacheBeforeViewport, virtualizationCacheLength.CacheAfterViewport });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	internal static string ToString(VirtualizationCacheLength cacheLength, CultureInfo cultureInfo)
	{
		char numericListSeparator = TokenizerHelper.GetNumericListSeparator(cultureInfo);
		Span<char> initialBuffer = stackalloc char[128];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(0, 3, cultureInfo, initialBuffer);
		handler.AppendFormatted(cacheLength.CacheBeforeViewport);
		handler.AppendFormatted(numericListSeparator);
		handler.AppendFormatted(cacheLength.CacheAfterViewport);
		return string.Create(cultureInfo, initialBuffer, ref handler);
	}

	internal static VirtualizationCacheLength FromString(string s, CultureInfo cultureInfo)
	{
		TokenizerHelper tokenizerHelper = new TokenizerHelper(s, cultureInfo);
		double[] array = new double[2];
		int num = 0;
		while (tokenizerHelper.NextToken())
		{
			if (num >= 2)
			{
				num = 3;
				break;
			}
			array[num] = double.Parse(tokenizerHelper.GetCurrentToken(), cultureInfo);
			num++;
		}
		return num switch
		{
			1 => new VirtualizationCacheLength(array[0]), 
			2 => new VirtualizationCacheLength(array[0], array[1]), 
			_ => throw new FormatException(SR.Format(SR.InvalidStringVirtualizationCacheLength, s)), 
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.VirtualizationCacheLengthConverter" /> class.</summary>
	public VirtualizationCacheLengthConverter()
	{
	}
}
