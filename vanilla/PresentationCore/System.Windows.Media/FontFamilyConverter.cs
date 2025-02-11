using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Navigation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts instances of the <see cref="T:System.String" /> type to and from <see cref="T:System.Windows.Media.FontFamily" /> instances.</summary>
public class FontFamilyConverter : TypeConverter
{
	/// <summary>Determines whether a class can be converted from a given type to an instance of <see cref="T:System.Windows.Media.FontFamily" />.</summary>
	/// <returns>true if the converter can convert from the specified type to an instance of <see cref="T:System.Windows.Media.FontFamily" />; otherwise, false.</returns>
	/// <param name="td">Describes the context information of a type.</param>
	/// <param name="t">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext td, Type t)
	{
		return t == typeof(string);
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Media.FontFamily" /> can be converted to a different type.</summary>
	/// <returns>true if the converter can convert this instance of <see cref="T:System.Windows.Media.FontFamily" /> to the specified type; otherwise, false.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="destinationType">The desired type that this instance of <see cref="T:System.Windows.Media.FontFamily" /> is being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null)
			{
				if (context.Instance is FontFamily { Source: not null } fontFamily)
				{
					return fontFamily.Source.Length != 0;
				}
				return false;
			}
			return true;
		}
		if (destinationType == typeof(FontFamily))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.Media.FontFamily" />.</summary>
	/// <returns>The instance of <see cref="T:System.Windows.Media.FontFamily" /> that is created from the converted <paramref name="o" /> parameter.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="cultureInfo">Cultural-specific information that should be respected during conversion.</param>
	/// <param name="o">The object being converted.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="o" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="o" /> is not null and is not a valid type that can be converted to a <see cref="T:System.Windows.Media.FontFamily" />.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object o)
	{
		if (o != null && o.GetType() == typeof(string))
		{
			string text = o as string;
			if (text == null || text.Length == 0)
			{
				throw GetConvertFromException(text);
			}
			Uri uri = null;
			if (context != null)
			{
				IUriContext uriContext = (IUriContext)context.GetService(typeof(IUriContext));
				if (uriContext != null)
				{
					if (uriContext.BaseUri != null)
					{
						uri = uriContext.BaseUri;
						if (!uri.IsAbsoluteUri)
						{
							uri = new Uri(BaseUriHelper.BaseUri, uri);
						}
					}
					else
					{
						uri = BaseUriHelper.BaseUri;
					}
				}
			}
			return new FontFamily(uri, text);
		}
		return base.ConvertFrom(context, cultureInfo, o);
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.Media.FontFamily" />.</summary>
	/// <returns>The object that is created from the converted instance of <see cref="T:System.Windows.Media.FontFamily" />.</returns>
	/// <param name="context">Describes the context information of a type.</param>
	/// <param name="culture">Cultural-specific information that should be respected during conversion.</param>
	/// <param name="value">The object being converted.</param>
	/// <param name="destinationType">The type that this instance of <see cref="T:System.Windows.Media.FontFamily" /> is converted to.</param>
	/// <exception cref="T:System.ArgumentException">Occurs if <paramref name="value" /> or <paramref name="destinationType" /> is not a valid type for conversion.</exception>
	/// <exception cref="T:System.ArgumentNullException">Occurs if <paramref name="value" /> or <paramref name="destinationType" /> is null.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is FontFamily fontFamily))
		{
			throw new ArgumentException(SR.Format(SR.General_Expected_Type, "FontFamily"), "value");
		}
		if (null == destinationType)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			if (fontFamily.Source != null)
			{
				return fontFamily.Source;
			}
			string value2 = null;
			CultureInfo cultureInfo = null;
			if (culture != null)
			{
				if (culture.Equals(CultureInfo.InvariantCulture))
				{
					culture = null;
				}
				else
				{
					cultureInfo = culture.Parent;
					if (cultureInfo != null && (cultureInfo.Equals(CultureInfo.InvariantCulture) || cultureInfo == culture))
					{
						cultureInfo = null;
					}
				}
			}
			LanguageSpecificStringDictionary familyNames = fontFamily.FamilyNames;
			if ((culture == null || !familyNames.TryGetValue(XmlLanguage.GetLanguage(culture.IetfLanguageTag), out value2)) && (cultureInfo == null || !familyNames.TryGetValue(XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag), out value2)) && !familyNames.TryGetValue(XmlLanguage.Empty, out value2))
			{
				foreach (FontFamilyMap familyMap in fontFamily.FamilyMaps)
				{
					if (FontFamilyMap.MatchCulture(familyMap.Language, culture))
					{
						value2 = familyMap.Target;
						break;
					}
				}
				if (value2 == null)
				{
					value2 = "#GLOBAL USER INTERFACE";
				}
			}
			return value2;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.FontFamilyConverter" /> class.</summary>
	public FontFamilyConverter()
	{
	}
}
