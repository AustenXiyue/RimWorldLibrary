using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Converts a <see cref="T:System.Windows.Media.ImageSource" /> to and from other data types. </summary>
public class ImageSourceConverter : TypeConverter
{
	private struct OBJECTHEADER
	{
		public short signature;

		public short headersize;

		public short objectType;

		public short nameLen;

		public short classLen;

		public short nameOffset;

		public short classOffset;

		public short width;

		public short height;

		public nint pInfo;
	}

	/// <summary>Determines whether the converter can convert an object of the given type to an instance of <see cref="T:System.Windows.Media.ImageSource" />. </summary>
	/// <returns>true if the converter can convert the provided type to an instance of <see cref="T:System.Windows.Media.ImageSource" />; otherwise, false.</returns>
	/// <param name="context">Type context information used to evaluate conversion.</param>
	/// <param name="sourceType">The type of the source that is being evaluated for conversion.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string) || sourceType == typeof(Stream) || sourceType == typeof(Uri) || sourceType == typeof(byte[]))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Media.ImageSource" /> can be converted to a different type. </summary>
	/// <returns>true if the converter can convert this instance of <see cref="T:System.Windows.Media.ImageSource" />; otherwise, false.</returns>
	/// <param name="context">Type context information used to evaluate conversion.</param>
	/// <param name="destinationType">The desired type to evaluate the conversion to.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="context" /> instance is not an <see cref="T:System.Windows.Media.ImageSource" />.</exception>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (context != null && context.Instance != null)
			{
				if (!(context.Instance is ImageSource))
				{
					throw new ArgumentException(SR.Format(SR.General_Expected_Type, "ImageSource"), "context");
				}
				return ((ImageSource)context.Instance).CanSerializeToString();
			}
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Attempts to convert a specified object to an instance of <see cref="T:System.Windows.Media.ImageSource" />.  </summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.ImageSource" />.</returns>
	/// <param name="context">Type context information used for conversion.</param>
	/// <param name="culture">Cultural information that is respected during conversion.</param>
	/// <param name="value">The object being converted.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is an invalid type.</exception>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		try
		{
			if (value == null)
			{
				throw GetConvertFromException(value);
			}
			if ((value is string && !string.IsNullOrEmpty((string)value)) || value is Uri)
			{
				UriHolder uriFromUriContext = TypeConverterHelper.GetUriFromUriContext(context, value);
				return BitmapFrame.CreateFromUriOrStream(uriFromUriContext.BaseUri, uriFromUriContext.OriginalUri, null, BitmapCreateOptions.None, BitmapCacheOption.Default, null);
			}
			if (value is byte[])
			{
				byte[] array = (byte[])value;
				if (array != null)
				{
					Stream stream = null;
					stream = GetBitmapStream(array);
					if (stream == null)
					{
						stream = new MemoryStream(array);
					}
					return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
				}
			}
			else if (value is Stream)
			{
				return BitmapFrame.Create((Stream)value, BitmapCreateOptions.None, BitmapCacheOption.Default);
			}
			return base.ConvertFrom(context, culture, value);
		}
		catch (Exception ex)
		{
			if (!CriticalExceptions.IsCriticalException(ex))
			{
				if (context == null && CoreAppContextSwitches.OverrideExceptionWithNullReferenceException)
				{
					throw new NullReferenceException();
				}
				if (context?.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget provideValueTarget)
				{
					IProvidePropertyFallback providePropertyFallback = provideValueTarget.TargetObject as IProvidePropertyFallback;
					DependencyProperty dependencyProperty = provideValueTarget.TargetProperty as DependencyProperty;
					if (providePropertyFallback != null && dependencyProperty != null && providePropertyFallback.CanProvidePropertyFallback(dependencyProperty.Name))
					{
						return providePropertyFallback.ProvidePropertyFallback(dependencyProperty.Name, ex);
					}
				}
			}
			throw;
		}
	}

	/// <summary>Attempts to convert an instance of <see cref="T:System.Windows.Media.ImageSource" /> to a specified type.</summary>
	/// <returns>A new instance of the <paramref name="destinationType" />.</returns>
	/// <param name="context">Context information used for conversion.</param>
	/// <param name="culture">Cultural information that is respected during conversion.</param>
	/// <param name="value">
	///   <see cref="T:System.Windows.Media.ImageSource" /> to convert.</param>
	/// <param name="destinationType">Type being evaluated for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null or is not a valid type.-or-<paramref name="context" /> instance cannot serialize to a string.</exception>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value is ImageSource)
		{
			ImageSource imageSource = (ImageSource)value;
			if (destinationType == typeof(string))
			{
				if (context != null && context.Instance != null && !imageSource.CanSerializeToString())
				{
					throw new NotSupportedException(SR.Converter_ConvertToNotSupported);
				}
				return imageSource.ConvertToString(null, culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private unsafe Stream GetBitmapStream(byte[] rawData)
	{
		fixed (byte* ptr = rawData)
		{
			nint num = (nint)ptr;
			if (num == IntPtr.Zero)
			{
				return null;
			}
			if (Marshal.ReadInt16(num) != 7189)
			{
				return null;
			}
			OBJECTHEADER oBJECTHEADER = Marshal.PtrToStructure<OBJECTHEADER>(num);
			if (Encoding.ASCII.GetString(rawData, oBJECTHEADER.headersize + 12, 6) != "PBrush")
			{
				return null;
			}
			byte[] bytes = Encoding.ASCII.GetBytes("BM");
			for (int i = oBJECTHEADER.headersize + 18; i < oBJECTHEADER.headersize + 510; i++)
			{
				if (bytes[0] == ptr[i] && bytes[1] == ptr[i + 1])
				{
					return new MemoryStream(rawData, i, rawData.Length - i);
				}
			}
		}
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ImageSourceConverter" /> class.</summary>
	public ImageSourceConverter()
	{
	}
}
