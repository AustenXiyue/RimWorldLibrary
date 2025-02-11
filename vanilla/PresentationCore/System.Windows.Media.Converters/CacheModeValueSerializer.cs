using System.Windows.Markup;

namespace System.Windows.Media.Converters;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Media.CacheMode" />.</summary>
public class CacheModeValueSerializer : ValueSerializer
{
	/// <summary>Determines whether the specified <see cref="T:System.String" /> can be converted to an instance of <see cref="T:System.Windows.Media.CacheMode" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted; otherwise, false. </returns>
	/// <param name="value">A <see cref="T:System.String" /> to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines whether the specified instance of <see cref="T:System.Windows.Media.CacheMode" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">An instance of <see cref="T:System.Windows.Media.CacheMode" /> to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is CacheMode))
		{
			return false;
		}
		return ((CacheMode)value).CanSerializeToString();
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.Windows.Media.CacheMode" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.CacheMode" /> based on the specified <paramref name="value" />.</returns>
	/// <param name="value">A <see cref="T:System.String" /> value to convert into a <see cref="T:System.Windows.Media.CacheMode" />.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value != null)
		{
			return CacheMode.Parse(value);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.CacheMode" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the specified <see cref="T:System.Windows.Media.CacheMode" /> object.</returns>
	/// <param name="value">An instance of <see cref="T:System.Windows.Media.CacheMode" /> to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is CacheMode)
		{
			CacheMode cacheMode = (CacheMode)value;
			if (!cacheMode.CanSerializeToString())
			{
				return base.ConvertToString(value, context);
			}
			return cacheMode.ConvertToString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Converters.CacheModeValueSerializer" /> class. </summary>
	public CacheModeValueSerializer()
	{
	}
}
