using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xaml;

namespace System.Windows.Markup;

/// <summary>Converts instances of <see cref="T:System.String" /> to and from instances of <see cref="T:System.DateTime" />.</summary>
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public class DateTimeValueSerializer : ValueSerializer
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.DateTimeValueSerializer" /> class.</summary>
	public DateTimeValueSerializer()
	{
	}

	/// <summary>Determines if the specified <see cref="T:System.String" /> can be convert to an instance of <see cref="T:System.DateTime" />.</summary>
	/// <returns>true if the value can be converted; otherwise, false.</returns>
	/// <param name="value">The string to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	/// <summary>Determines if the specified object can be converted to a <see cref="T:System.String" />.</summary>
	/// <returns>true if <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, false.</returns>
	/// <param name="value">The object to evaluate for conversion.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is DateTime))
		{
			return false;
		}
		return true;
	}

	/// <summary>Converts a <see cref="T:System.String" /> into a <see cref="T:System.DateTime" />.</summary>
	/// <returns>A new instance of <see cref="T:System.DateTime" /> based on the supplied <paramref name="value" />.</returns>
	/// <param name="value">The string to convert into a <see cref="T:System.DateTime" />.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is null.</exception>
	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value.Length == 0)
		{
			return DateTime.MinValue;
		}
		DateTimeFormatInfo dateTimeFormatInfo = (DateTimeFormatInfo)TypeConverterHelper.InvariantEnglishUS.GetFormat(typeof(DateTimeFormatInfo));
		DateTimeStyles styles = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.RoundtripKind;
		if (dateTimeFormatInfo != null)
		{
			return DateTime.Parse(value, dateTimeFormatInfo, styles);
		}
		return DateTime.Parse(value, TypeConverterHelper.InvariantEnglishUS, styles);
	}

	/// <summary>Converts an instance of <see cref="T:System.DateTime" /> to a <see cref="T:System.String" />.</summary>
	/// <returns>A string representation of the specified <see cref="T:System.DateTime" />.</returns>
	/// <param name="value">The object to convert into a string.</param>
	/// <param name="context">Context information that is used for conversion.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="value" /> is not a <see cref="T:System.DateTime" /> or is null.</exception>
	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value == null || !(value is DateTime dateTime))
		{
			throw GetConvertToException(value, typeof(string));
		}
		StringBuilder stringBuilder = new StringBuilder("yyyy-MM-dd");
		if (dateTime.TimeOfDay == TimeSpan.Zero)
		{
			if (dateTime.Kind != 0)
			{
				stringBuilder.Append("'T'HH':'mm");
			}
		}
		else
		{
			long num = dateTime.Ticks % 10000000;
			int second = dateTime.Second;
			stringBuilder.Append("'T'HH':'mm");
			if (second != 0 || num != 0L)
			{
				stringBuilder.Append("':'ss");
				if (num != 0L)
				{
					stringBuilder.Append("'.'FFFFFFF");
				}
			}
		}
		stringBuilder.Append('K');
		return dateTime.ToString(stringBuilder.ToString(), TypeConverterHelper.InvariantEnglishUS);
	}
}
