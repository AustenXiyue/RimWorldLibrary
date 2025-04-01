using System.Globalization;

namespace System.Runtime.Serialization;

/// <summary>Specifies date-time format options.</summary>
public class DateTimeFormat
{
	private string formatString;

	private IFormatProvider formatProvider;

	private DateTimeStyles dateTimeStyles;

	/// <summary>Gets the format strings to control the formatting produced when a date or time is represented as a string.</summary>
	/// <returns>The format strings to control the formatting produced when a date or time is represented as a string.</returns>
	public string FormatString => formatString;

	/// <summary>Gets an object that controls formatting.</summary>
	public IFormatProvider FormatProvider => formatProvider;

	/// <summary>Gets or sets the formatting options that customize string parsing for some date and time parsing methods.</summary>
	/// <returns>The formatting options that customize string parsing for some date and time parsing methods.</returns>
	public DateTimeStyles DateTimeStyles
	{
		get
		{
			return dateTimeStyles;
		}
		set
		{
			dateTimeStyles = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.DateTimeFormat" /> class using the format string.</summary>
	/// <param name="formatString">The format string.</param>
	public DateTimeFormat(string formatString)
		: this(formatString, DateTimeFormatInfo.CurrentInfo)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.DateTimeFormat" /> class using the format string and format provider.</summary>
	/// <param name="formatString">The format sting.</param>
	/// <param name="formatProvider">The format provider.</param>
	public DateTimeFormat(string formatString, IFormatProvider formatProvider)
	{
		if (formatString == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatString");
		}
		if (formatProvider == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("formatProvider");
		}
		this.formatString = formatString;
		this.formatProvider = formatProvider;
		dateTimeStyles = DateTimeStyles.RoundtripKind;
	}
}
