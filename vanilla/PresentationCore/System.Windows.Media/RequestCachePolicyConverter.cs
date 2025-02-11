using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Net.Cache;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Parses a <see cref="T:System.Net.Cache.RequestCachePolicy" />.</summary>
public sealed class RequestCachePolicyConverter : TypeConverter
{
	/// <summary>Returns a value that indicates whether this converter can convert an object of the specified type to the type of this converter, using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="td">The format context.</param>
	/// <param name="t">The type to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether this converter can convert the object to the specified type, using the specified context.</summary>
	/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
	/// <param name="typeDescriptorContext">The format context.</param>
	/// <param name="destinationType">The type being queried for support.</param>
	public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts the specified object to a <see cref="T:System.Net.Cache.RequestCachePolicy" /> object.</summary>
	/// <returns>An object that represents the converted value.</returns>
	/// <param name="td">The format context.</param>
	/// <param name="ci">The current culture.</param>
	/// <param name="value">The object to convert.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is null or is not of a valid type.</exception>
	public override object ConvertFrom(ITypeDescriptorContext td, CultureInfo ci, object value)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (!(value is string value2))
		{
			throw new ArgumentException(SR.Format(SR.General_BadType, "ConvertFrom"), "value");
		}
		return new HttpRequestCachePolicy((HttpRequestCacheLevel)Enum.Parse(typeof(HttpRequestCacheLevel), value2, ignoreCase: true));
	}

	/// <summary>Converts this object to the specified type.</summary>
	/// <returns>The object that is constructed from the conversion.</returns>
	/// <param name="typeDescriptorContext">The format context.</param>
	/// <param name="cultureInfo">The culture to use for the conversion. </param>
	/// <param name="value">The policy to convert.</param>
	/// <param name="destinationType">The type to convert to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is HttpRequestCachePolicy httpRequestCachePolicy)
		{
			if (destinationType == typeof(string))
			{
				return httpRequestCachePolicy.Level.ToString();
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(HttpRequestCachePolicy).GetConstructor(new Type[1] { typeof(HttpRequestCachePolicy) }), new object[1] { httpRequestCachePolicy.Level });
			}
		}
		if (value is RequestCachePolicy requestCachePolicy)
		{
			if (destinationType == typeof(string))
			{
				return requestCachePolicy.Level.ToString();
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				return new InstanceDescriptor(typeof(RequestCachePolicy).GetConstructor(new Type[1] { typeof(RequestCachePolicy) }), new object[1] { requestCachePolicy.Level });
			}
		}
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.RequestCachePolicyConverter" /> class.</summary>
	public RequestCachePolicyConverter()
	{
	}
}
