using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace System.Windows.Media;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.Windows.Media.ImageSource" />.</summary>
public class ImageSourceValueSerializer : ValueSerializer
{
	/// <summary>Determines if a <see cref="T:System.String" /> can be converted to an instance of <see cref="T:System.Windows.Media.ImageSource" />.</summary>
	/// <returns>true if value can be converted; otherwise, false.</returns>
	/// <param name="value">
	///   <see cref="T:System.String" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines if an instance of <see cref="T:System.Windows.Media.ImageSource" /> can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">Instance of <see cref="T:System.Windows.Media.ImageSource" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (value is ImageSource imageSource)
		{
			return imageSource.CanSerializeToString();
		}
		return false;
	}

	/// <summary>Converts a <see cref="T:System.String" /> to an <see cref="T:System.Windows.Media.ImageSource" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.ImageSource" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">String value to convert into an <see cref="T:System.Windows.Media.ImageSource" />.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (!string.IsNullOrEmpty(value))
		{
			UriHolder uriFromUriContext = TypeConverterHelper.GetUriFromUriContext(context, value);
			return BitmapFrame.CreateFromUriOrStream(uriFromUriContext.BaseUri, uriFromUriContext.OriginalUri, null, BitmapCreateOptions.None, BitmapCacheOption.Default, null);
		}
		return base.ConvertFromString(value, context);
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Media.ImageSource" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of the supplied <see cref="T:System.Windows.Media.ImageSource" /> object.</returns>
	/// <param name="value">The <see cref="T:System.Windows.Media.ImageSource" /> to evaluate for conversion.</param>
	/// <param name="context">Context information used for conversion.</param>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value is ImageSource imageSource)
		{
			return imageSource.ConvertToString(null, System.Windows.Markup.TypeConverterHelper.InvariantEnglishUS);
		}
		return base.ConvertToString(value, context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ImageSourceValueSerializer" /> class.</summary>
	public ImageSourceValueSerializer()
	{
	}
}
