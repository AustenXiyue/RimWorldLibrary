using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Converts a <see cref="T:System.Windows.Input.Cursor" /> object to and from other types.</summary>
public class CursorConverter : TypeConverter
{
	private StandardValuesCollection _standardValues;

	/// <summary>Determines whether an object of the specified type can be converted to an instance of <see cref="T:System.Windows.Input.Cursor" />, using the specified context.</summary>
	/// <returns>true if <paramref name="sourceType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="sourceType">The type being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Input.Cursor" /> can be converted to the specified type, using the specified context.</summary>
	/// <returns>true if <paramref name="destinationType" /> is of type <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="destinationType">The type being evaluated for conversion.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	private PropertyInfo[] GetProperties()
	{
		return typeof(Cursors).GetProperties(BindingFlags.Static | BindingFlags.Public);
	}

	/// <summary>Gets a collection of standard cursor values, using the specified context.</summary>
	/// <returns>A collection that holds a standard set of valid values.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		if (_standardValues == null)
		{
			ArrayList arrayList = new ArrayList();
			PropertyInfo[] properties = GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				object[] index = null;
				arrayList.Add(propertyInfo.GetValue(null, index));
			}
			_standardValues = new StandardValuesCollection(arrayList.ToArray());
		}
		return _standardValues;
	}

	/// <summary>Determines whether this object supports a standard set of values that can be picked from a list, using the specified context.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Attempts to convert the specified object to a <see cref="T:System.Windows.Input.Cursor" />, using the specified context.</summary>
	/// <returns>The converted object, or null if <paramref name="value" /> is an empty string.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> cannot be converted</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string text = ((string)value).Trim();
			if (!(text != string.Empty))
			{
				return null;
			}
			if (text.LastIndexOf('.') == -1)
			{
				switch ((CursorType)Enum.Parse(typeof(CursorType), text))
				{
				case CursorType.Arrow:
					return Cursors.Arrow;
				case CursorType.AppStarting:
					return Cursors.AppStarting;
				case CursorType.Cross:
					return Cursors.Cross;
				case CursorType.Help:
					return Cursors.Help;
				case CursorType.IBeam:
					return Cursors.IBeam;
				case CursorType.SizeAll:
					return Cursors.SizeAll;
				case CursorType.SizeNESW:
					return Cursors.SizeNESW;
				case CursorType.SizeNS:
					return Cursors.SizeNS;
				case CursorType.SizeNWSE:
					return Cursors.SizeNWSE;
				case CursorType.SizeWE:
					return Cursors.SizeWE;
				case CursorType.UpArrow:
					return Cursors.UpArrow;
				case CursorType.Wait:
					return Cursors.Wait;
				case CursorType.Hand:
					return Cursors.Hand;
				case CursorType.No:
					return Cursors.No;
				case CursorType.None:
					return Cursors.None;
				case CursorType.Pen:
					return Cursors.Pen;
				case CursorType.ScrollNS:
					return Cursors.ScrollNS;
				case CursorType.ScrollWE:
					return Cursors.ScrollWE;
				case CursorType.ScrollAll:
					return Cursors.ScrollAll;
				case CursorType.ScrollN:
					return Cursors.ScrollN;
				case CursorType.ScrollS:
					return Cursors.ScrollS;
				case CursorType.ScrollW:
					return Cursors.ScrollW;
				case CursorType.ScrollE:
					return Cursors.ScrollE;
				case CursorType.ScrollNW:
					return Cursors.ScrollNW;
				case CursorType.ScrollNE:
					return Cursors.ScrollNE;
				case CursorType.ScrollSW:
					return Cursors.ScrollSW;
				case CursorType.ScrollSE:
					return Cursors.ScrollSE;
				case CursorType.ArrowCD:
					return Cursors.ArrowCD;
				}
			}
			else if (text.EndsWith(".cur", StringComparison.OrdinalIgnoreCase) || text.EndsWith(".ani", StringComparison.OrdinalIgnoreCase))
			{
				UriHolder uriFromUriContext = TypeConverterHelper.GetUriFromUriContext(context, text);
				Uri resolvedUri = BindUriHelper.GetResolvedUri(uriFromUriContext.BaseUri, uriFromUriContext.OriginalUri);
				if (resolvedUri.IsAbsoluteUri && resolvedUri.IsFile)
				{
					return new Cursor(resolvedUri.LocalPath);
				}
				WebRequest request = WpfWebRequestHelper.CreateRequest(resolvedUri);
				WpfWebRequestHelper.ConfigCachePolicy(request, isRefresh: false);
				return new Cursor(WpfWebRequestHelper.GetResponseStream(request));
			}
		}
		throw GetConvertFromException(value);
	}

	/// <summary>Attempts to convert a <see cref="T:System.Windows.Input.Cursor" /> to the specified type, using the specified context.</summary>
	/// <returns>The converted object, or an empty string if <paramref name="value" /> is null.</returns>
	/// <param name="context">A format context that provides information about the environment from which this converter is being invoked.</param>
	/// <param name="culture">Culture specific information.</param>
	/// <param name="value">The object to convert.</param>
	/// <param name="destinationType">The type to convert the object to.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="source" /> cannot be converted.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string))
		{
			if (value is Cursor cursor)
			{
				return cursor.ToString();
			}
			return string.Empty;
		}
		throw GetConvertToException(value, destinationType);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.CursorConverter" /> class. </summary>
	public CursorConverter()
	{
	}
}
