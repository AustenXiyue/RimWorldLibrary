using System.Globalization;

namespace System.Windows.Markup;

internal class XamlParser
{
	internal static void ThrowException(string id, int lineNumber, int linePosition)
	{
		ThrowExceptionWithLine(SR.GetResourceString(id), lineNumber, linePosition);
	}

	internal static void ThrowException(string id, string value, int lineNumber, int linePosition)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), value), lineNumber, linePosition);
	}

	internal static void ThrowException(string id, string value1, string value2, int lineNumber, int linePosition)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), value1, value2), lineNumber, linePosition);
	}

	internal static void ThrowException(string id, string value1, string value2, string value3, int lineNumber, int linePosition)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), value1, value2, value3), lineNumber, linePosition);
	}

	internal static void ThrowException(string id, string value1, string value2, string value3, string value4, int lineNumber, int linePosition)
	{
		ThrowExceptionWithLine(SR.Format(SR.GetResourceString(id), value1, value2, value3, value4), lineNumber, linePosition);
	}

	private static void ThrowExceptionWithLine(string message, int lineNumber, int linePosition)
	{
		message += " ";
		message += SR.Format(SR.ParserLineAndOffset, lineNumber.ToString(CultureInfo.CurrentCulture), linePosition.ToString(CultureInfo.CurrentCulture));
		throw new XamlParseException(message, lineNumber, linePosition);
	}
}
