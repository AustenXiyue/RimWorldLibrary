using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System.Xml;

/// <summary>Returns detailed information about the last exception.</summary>
[Serializable]
public class XmlException : SystemException
{
	private string res;

	private string[] args;

	private int lineNumber;

	private int linePosition;

	[OptionalField]
	private string sourceUri;

	private string message;

	/// <summary>Gets the line number indicating where the error occurred.</summary>
	/// <returns>The line number indicating where the error occurred.</returns>
	public int LineNumber => lineNumber;

	/// <summary>Gets the line position indicating where the error occurred.</summary>
	/// <returns>The line position indicating where the error occurred.</returns>
	public int LinePosition => linePosition;

	/// <summary>Gets the location of the XML file.</summary>
	/// <returns>The source URI for the XML data. If there is no source URI, this property returns null.</returns>
	public string SourceUri => sourceUri;

	/// <summary>Gets a message describing the current exception.</summary>
	/// <returns>The error message that explains the reason for the exception.</returns>
	public override string Message
	{
		get
		{
			if (message != null)
			{
				return message;
			}
			return base.Message;
		}
	}

	internal string ResString => res;

	/// <summary>Initializes a new instance of the XmlException class using the information in the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> and <see cref="T:System.Runtime.Serialization.StreamingContext" /> objects.</summary>
	/// <param name="info">The SerializationInfo object containing all the properties of an XmlException. </param>
	/// <param name="context">The StreamingContext object containing the context information. </param>
	protected XmlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		res = (string)info.GetValue("res", typeof(string));
		args = (string[])info.GetValue("args", typeof(string[]));
		lineNumber = (int)info.GetValue("lineNumber", typeof(int));
		linePosition = (int)info.GetValue("linePosition", typeof(int));
		sourceUri = string.Empty;
		string text = null;
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			string name = current.Name;
			if (!(name == "sourceUri"))
			{
				if (name == "version")
				{
					text = (string)current.Value;
				}
			}
			else
			{
				sourceUri = (string)current.Value;
			}
		}
		if (text == null)
		{
			message = CreateMessage(res, args, lineNumber, linePosition);
		}
		else
		{
			message = null;
		}
	}

	/// <summary>Streams all the XmlException properties into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class for the given <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="info">The SerializationInfo object. </param>
	/// <param name="context">The StreamingContext object. </param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("res", res);
		info.AddValue("args", args);
		info.AddValue("lineNumber", lineNumber);
		info.AddValue("linePosition", linePosition);
		info.AddValue("sourceUri", sourceUri);
		info.AddValue("version", "2.0");
	}

	/// <summary>Initializes a new instance of the XmlException class.</summary>
	public XmlException()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the XmlException class with a specified error message.</summary>
	/// <param name="message">The error description. </param>
	public XmlException(string message)
		: this(message, (Exception)null, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the XmlException class.</summary>
	/// <param name="message">The description of the error condition. </param>
	/// <param name="innerException">The <see cref="T:System.Exception" /> that threw the XmlException, if any. This value can be null. </param>
	public XmlException(string message, Exception innerException)
		: this(message, innerException, 0, 0)
	{
	}

	/// <summary>Initializes a new instance of the XmlException class with the specified message, inner exception, line number, and line position.</summary>
	/// <param name="message">The error description. </param>
	/// <param name="innerException">The exception that is the cause of the current exception. This value can be null. </param>
	/// <param name="lineNumber">The line number indicating where the error occurred. </param>
	/// <param name="linePosition">The line position indicating where the error occurred. </param>
	public XmlException(string message, Exception innerException, int lineNumber, int linePosition)
		: this(message, innerException, lineNumber, linePosition, null)
	{
	}

	internal XmlException(string message, Exception innerException, int lineNumber, int linePosition, string sourceUri)
		: base(FormatUserMessage(message, lineNumber, linePosition), innerException)
	{
		base.HResult = -2146232000;
		res = ((message == null) ? "An XML error has occurred." : "{0}");
		args = new string[1] { message };
		this.sourceUri = sourceUri;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	internal XmlException(string res, string[] args)
		: this(res, args, null, 0, 0, null)
	{
	}

	internal XmlException(string res, string[] args, string sourceUri)
		: this(res, args, null, 0, 0, sourceUri)
	{
	}

	internal XmlException(string res, string arg)
		: this(res, new string[1] { arg }, null, 0, 0, null)
	{
	}

	internal XmlException(string res, string arg, string sourceUri)
		: this(res, new string[1] { arg }, null, 0, 0, sourceUri)
	{
	}

	internal XmlException(string res, string arg, IXmlLineInfo lineInfo)
		: this(res, new string[1] { arg }, lineInfo, null)
	{
	}

	internal XmlException(string res, string arg, Exception innerException, IXmlLineInfo lineInfo)
		: this(res, new string[1] { arg }, innerException, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, null)
	{
	}

	internal XmlException(string res, string arg, IXmlLineInfo lineInfo, string sourceUri)
		: this(res, new string[1] { arg }, lineInfo, sourceUri)
	{
	}

	internal XmlException(string res, string[] args, IXmlLineInfo lineInfo)
		: this(res, args, lineInfo, null)
	{
	}

	internal XmlException(string res, string[] args, IXmlLineInfo lineInfo, string sourceUri)
		: this(res, args, null, lineInfo?.LineNumber ?? 0, lineInfo?.LinePosition ?? 0, sourceUri)
	{
	}

	internal XmlException(string res, int lineNumber, int linePosition)
		: this(res, null, null, lineNumber, linePosition)
	{
	}

	internal XmlException(string res, string arg, int lineNumber, int linePosition)
		: this(res, new string[1] { arg }, null, lineNumber, linePosition, null)
	{
	}

	internal XmlException(string res, string arg, int lineNumber, int linePosition, string sourceUri)
		: this(res, new string[1] { arg }, null, lineNumber, linePosition, sourceUri)
	{
	}

	internal XmlException(string res, string[] args, int lineNumber, int linePosition)
		: this(res, args, null, lineNumber, linePosition, null)
	{
	}

	internal XmlException(string res, string[] args, int lineNumber, int linePosition, string sourceUri)
		: this(res, args, null, lineNumber, linePosition, sourceUri)
	{
	}

	internal XmlException(string res, string[] args, Exception innerException, int lineNumber, int linePosition)
		: this(res, args, innerException, lineNumber, linePosition, null)
	{
	}

	internal XmlException(string res, string[] args, Exception innerException, int lineNumber, int linePosition, string sourceUri)
		: base(CreateMessage(res, args, lineNumber, linePosition), innerException)
	{
		base.HResult = -2146232000;
		this.res = res;
		this.args = args;
		this.sourceUri = sourceUri;
		this.lineNumber = lineNumber;
		this.linePosition = linePosition;
	}

	private static string FormatUserMessage(string message, int lineNumber, int linePosition)
	{
		if (message == null)
		{
			return CreateMessage("An XML error has occurred.", null, lineNumber, linePosition);
		}
		if (lineNumber == 0 && linePosition == 0)
		{
			return message;
		}
		return CreateMessage("{0}", new string[1] { message }, lineNumber, linePosition);
	}

	private static string CreateMessage(string res, string[] args, int lineNumber, int linePosition)
	{
		try
		{
			string @string;
			if (lineNumber == 0)
			{
				@string = Res.GetString(res, args);
			}
			else
			{
				string text = lineNumber.ToString(CultureInfo.InvariantCulture);
				string text2 = linePosition.ToString(CultureInfo.InvariantCulture);
				@string = Res.GetString(res, args);
				@string = Res.GetString("{0} Line {1}, position {2}.", @string, text, text2);
			}
			return @string;
		}
		catch (MissingManifestResourceException)
		{
			return "UNKNOWN(" + res + ")";
		}
	}

	internal static string[] BuildCharExceptionArgs(string data, int invCharIndex)
	{
		return BuildCharExceptionArgs(data[invCharIndex], (invCharIndex + 1 < data.Length) ? data[invCharIndex + 1] : '\0');
	}

	internal static string[] BuildCharExceptionArgs(char[] data, int invCharIndex)
	{
		return BuildCharExceptionArgs(data, data.Length, invCharIndex);
	}

	internal static string[] BuildCharExceptionArgs(char[] data, int length, int invCharIndex)
	{
		return BuildCharExceptionArgs(data[invCharIndex], (invCharIndex + 1 < length) ? data[invCharIndex + 1] : '\0');
	}

	internal static string[] BuildCharExceptionArgs(char invChar, char nextChar)
	{
		string[] array = new string[2];
		if (XmlCharType.IsHighSurrogate(invChar) && nextChar != 0)
		{
			int num = XmlCharType.CombineSurrogateChar(nextChar, invChar);
			array[0] = new string(new char[2] { invChar, nextChar });
			array[1] = string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", num);
		}
		else
		{
			if (invChar == '\0')
			{
				array[0] = ".";
			}
			else
			{
				array[0] = invChar.ToString(CultureInfo.InvariantCulture);
			}
			array[1] = string.Format(CultureInfo.InvariantCulture, "0x{0:X2}", (int)invChar);
		}
		return array;
	}

	internal static bool IsCatchableException(Exception e)
	{
		if (!(e is StackOverflowException) && !(e is OutOfMemoryException) && !(e is ThreadAbortException) && !(e is ThreadInterruptedException) && !(e is NullReferenceException))
		{
			return !(e is AccessViolationException);
		}
		return false;
	}
}
