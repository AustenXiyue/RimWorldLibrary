using System.ComponentModel;
using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts a <see cref="T:System.Windows.Media.CacheMode" /> from one data type to another.</summary>
public sealed class CacheModeConverter : TypeConverter
{
	/// <summary>Determines whether this <see cref="T:System.Windows.Media.CacheModeConverter" /> can convert an instance of the specified type to a <see cref="T:System.Windows.Media.CacheMode" />, using the specified context.</summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.CacheModeConverter" /> can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that specifies the type you want to convert from. </param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether this <see cref="T:System.Windows.Media.CacheModeConverter" /> can convert a <see cref="T:System.Windows.Media.CacheMode" /> to an instance of a specified type, using the specified context.</summary>
	/// <returns>true if this <see cref="T:System.Windows.Media.CacheModeConverter" /> can perform the conversion; otherwise, false.</returns>
	/// <param name="context">A <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="destinationType">A type representing the type to convert to.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is CacheMode))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "CacheMode"), "context");
				}
				return ((CacheMode)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts a specified object to a <see cref="T:System.Windows.Media.CacheMode" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.CacheMode" /> that is created by converting <paramref name="value" />; otherwise, an exception is raised.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that holds information about a specific culture. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to be converted. </param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or not a type that can be converted to <see cref="T:System.Windows.Media.CacheMode" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value is string value2)
		{
			return CacheMode.Parse(value2);
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts a <see cref="T:System.Windows.Media.CacheMode" /> to the specified type.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that represents information about a culture, such as language and calendar system. Can be null.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert.</param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert to.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null, <paramref name="value" /> is not a <see cref="T:System.Windows.Media.CacheMode" />, or <paramref name="destinationType" /> is not a string.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is CacheMode)
		{
			CacheMode cacheMode = (CacheMode)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !cacheMode.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return cacheMode.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.CacheModeConverter" /> class. </summary>
	public CacheModeConverter()
	{
	}
}
